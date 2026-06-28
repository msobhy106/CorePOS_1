using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Purchases.Commands;
using CorePOS.Application.Interfaces;
using CorePOS.Domain.Entities;
using CorePOS.Domain.Enums;
using CorePOS.Domain.Interfaces;

namespace CorePOS.Application.Features.Purchases.Handlers;

public class CreatePurchaseInvoiceHandler : IRequestHandler<CreatePurchaseInvoiceCommand, Result<int>>
{
    private readonly IUnitOfWork      _uow;
    private readonly ISequenceService _seq;

    public CreatePurchaseInvoiceHandler(IUnitOfWork uow, ISequenceService seq)
    {
        _uow = uow;
        _seq = seq;
    }

    public async Task<Result<int>> Handle(CreatePurchaseInvoiceCommand cmd, CancellationToken ct)
    {
        var invoiceNo = await _seq.NextPurchaseInvoiceNoAsync("BR", ct);

        var invoice = PurchaseInvoice.Create(invoiceNo, cmd.BranchId, cmd.WarehouseId,
            cmd.UserId, cmd.SupplierId, cmd.SupplierInvoiceNo);

        foreach (var item in cmd.Items)
            invoice.AddItem(item.ProductId, item.UnitId, item.ProductNameAr,
                item.Quantity, item.UnitCost, item.DiscountPercent,
                item.TaxPercent, item.SalePriceAfter);

        invoice.ApplyDiscount(cmd.DiscountPercent);
        invoice.SetPayment(cmd.PaymentMethod, cmd.PaidAmount);

        await _uow.Purchases.AddAsync(invoice, ct);
        await _uow.SaveChangesAsync(ct);

        if (cmd.AutoApprove)
        {
            invoice.Approve(cmd.UserId);
            await ApproveStockAsync(invoice, cmd.UserId, ct);
            _uow.Purchases.Update(invoice);
            await _uow.SaveChangesAsync(ct);
        }

        return Result<int>.Success(invoice.Id);
    }

    private async Task ApproveStockAsync(PurchaseInvoice invoice, int userId, CancellationToken ct)
    {
        foreach (var item in invoice.Items)
        {
            await _uow.Inventory.LogTransactionAsync(
                item.ProductId, invoice.WarehouseId, item.Quantity,
                StockDirection.In, InventoryTransactionType.PurchaseIn,
                item.UnitCost, invoice.Id, "PurchaseInvoice",
                null, userId, ct);

            if (item.SalePriceAfter.HasValue)
            {
                var product = await _uow.Products.GetByIdAsync(item.ProductId, ct);
                if (product is not null)
                {
                    product.UpdatePrices(item.UnitCost, item.SalePriceAfter.Value,
                        product.WholesalePrice, product.HalfWholesalePrice,
                        product.SpecialPrice, product.TaxPercent);
                    _uow.Products.Update(product);
                }
            }
        }

        if (invoice.SupplierId.HasValue && invoice.RemainingAmount > 0)
        {
            var supplier = await _uow.Suppliers.GetByIdAsync(invoice.SupplierId.Value, ct);
            supplier?.AddPayable(invoice.RemainingAmount);
            if (supplier is not null) _uow.Suppliers.Update(supplier);
        }
    }
}
