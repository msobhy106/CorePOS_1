using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Sales.Commands;
using CorePOS.Application.Interfaces;
using CorePOS.Domain.Entities;
using CorePOS.Domain.Enums;
using CorePOS.Domain.Interfaces;

namespace CorePOS.Application.Features.Sales.Handlers;

public class CreateSaleInvoiceHandler : IRequestHandler<CreateSaleInvoiceCommand, Result<int>>
{
    private readonly IUnitOfWork         _uow;
    private readonly ISequenceService    _seq;
    private readonly ICurrentUserService _user;
    private readonly ISettingsRepository _settings;

    public CreateSaleInvoiceHandler(IUnitOfWork uow, ISequenceService seq,
        ICurrentUserService user, ISettingsRepository settings)
    {
        _uow      = uow;
        _seq      = seq;
        _user     = user;
        _settings = settings;
    }

    public async Task<Result<int>> Handle(CreateSaleInvoiceCommand cmd, CancellationToken ct)
    {
        // Check shift requirement
        var requireShift = await _settings.GetBoolAsync("POSRequireShift", true, ct);
        if (requireShift && !cmd.ShiftId.HasValue)
            return Result<int>.Failure("يجب فتح وردية قبل البيع");

        // Check stock availability
        var allowNegative = await _settings.GetBoolAsync("POSAllowNegativeStock", false, ct);
        foreach (var item in cmd.Items)
        {
            var stock = await _uow.Inventory.GetStockAsync(item.ProductId, cmd.WarehouseId, ct);
            var available = stock?.Quantity ?? 0;
            if (!allowNegative && available < item.Quantity)
                return Result<int>.Failure($"الصنف '{item.ProductNameAr}' غير متوفر بالكمية المطلوبة. المتاح: {available}");
        }

        // Get branch code for invoice number
        var branch = await _uow.Products.GetByIdAsync(cmd.BranchId, ct); // reuse - just for code
        var invoiceNo = await _seq.NextSaleInvoiceNoAsync("BR", ct);

        var invoice = SalesInvoice.Create(invoiceNo, cmd.BranchId, cmd.WarehouseId,
            cmd.UserId, cmd.ShiftId, cmd.CustomerId, cmd.InvoiceType);

        // Add items (merges duplicates per SRS requirement)
        foreach (var item in cmd.Items)
            invoice.AddOrUpdateItem(item.ProductId, item.UnitId, item.ProductNameAr,
                item.Barcode, item.Quantity, item.UnitPrice, item.PurchasePrice,
                item.DiscountPercent, item.TaxPercent);

        invoice.ApplyDiscount(cmd.DiscountPercent, cmd.DiscountAmount);
        invoice.SetTax(cmd.TaxPercent);
        invoice.SetDelivery(cmd.DeliveryCost, cmd.DeliveryAgentId);
        invoice.SetPayment(cmd.PaymentMethod, cmd.PaidAmount,
            cmd.VisaAmount, cmd.BankTransferAmount, cmd.EWalletAmount);
        invoice.SetNotes(cmd.Notes);

        await _uow.Sales.AddAsync(invoice, ct);
        await _uow.SaveChangesAsync(ct);

        // Deduct stock for each item
        foreach (var item in invoice.Items)
        {
            await _uow.Inventory.LogTransactionAsync(
                item.ProductId, cmd.WarehouseId, item.Quantity,
                StockDirection.Out, InventoryTransactionType.SaleOut,
                item.PurchasePrice, invoice.Id, "SalesInvoice",
                null, cmd.UserId, ct);
        }

        // Update customer balance if credit
        if (cmd.CustomerId.HasValue && invoice.RemainingAmount > 0)
        {
            var customer = await _uow.Customers.GetByIdAsync(cmd.CustomerId.Value, ct);
            customer?.AddDebt(invoice.RemainingAmount);
            if (customer != null) _uow.Customers.Update(customer);
        }

        invoice.Complete();
        _uow.Sales.Update(invoice);
        await _uow.SaveChangesAsync(ct);

        return Result<int>.Success(invoice.Id);
    }
}
