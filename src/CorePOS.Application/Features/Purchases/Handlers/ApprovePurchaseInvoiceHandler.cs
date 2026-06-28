using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Purchases.Commands;
using CorePOS.Domain.Enums;
using CorePOS.Domain.Interfaces;

namespace CorePOS.Application.Features.Purchases.Handlers;

public class ApprovePurchaseInvoiceHandler : IRequestHandler<ApprovePurchaseInvoiceCommand, Result>
{
    private readonly IUnitOfWork _uow;
    public ApprovePurchaseInvoiceHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(ApprovePurchaseInvoiceCommand cmd, CancellationToken ct)
    {
        var invoice = await _uow.Purchases.GetByIdWithItemsAsync(cmd.InvoiceId, ct);
        if (invoice is null) return Result.NotFound("فاتورة الشراء غير موجودة");

        try { invoice.Approve(cmd.ApprovedBy); }
        catch (InvalidOperationException ex) { return Result.Failure(ex.Message); }

        foreach (var item in invoice.Items)
        {
            await _uow.Inventory.LogTransactionAsync(
                item.ProductId, invoice.WarehouseId, item.Quantity,
                StockDirection.In, InventoryTransactionType.PurchaseIn,
                item.UnitCost, invoice.Id, "PurchaseInvoice",
                null, cmd.ApprovedBy, ct);

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

        _uow.Purchases.Update(invoice);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
