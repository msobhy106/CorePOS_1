using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Sales.Commands;
using CorePOS.Application.Interfaces;
using CorePOS.Domain.Entities;
using CorePOS.Domain.Enums;
using CorePOS.Domain.Interfaces;

namespace CorePOS.Application.Features.Sales.Handlers;

public class CreateSaleReturnHandler : IRequestHandler<CreateSaleReturnCommand, Result<int>>
{
    private readonly IUnitOfWork      _uow;
    private readonly ISequenceService _seq;

    public CreateSaleReturnHandler(IUnitOfWork uow, ISequenceService seq)
    {
        _uow = uow;
        _seq = seq;
    }

    public async Task<Result<int>> Handle(CreateSaleReturnCommand cmd, CancellationToken ct)
    {
        var original = await _uow.Sales.GetByIdWithItemsAsync(cmd.OriginalInvoiceId, ct);
        if (original is null) return Result<int>.NotFound("الفاتورة الأصلية غير موجودة");

        if (original.Status == SaleInvoiceStatus.FullReturn)
            return Result<int>.Failure("تم إرجاع هذه الفاتورة بالكامل مسبقاً");

        var returnNo = await _seq.NextSaleReturnNoAsync("BR", ct);
        var ret = SalesReturn.Create(returnNo, cmd.OriginalInvoiceId,
            cmd.BranchId, cmd.WarehouseId, cmd.UserId, cmd.ReturnType,
            cmd.RefundMethod, original.CustomerId, cmd.ShiftId, cmd.Notes);

        foreach (var item in cmd.Items)
        {
            var origItem = original.Items.FirstOrDefault(i => i.Id == item.InvoiceItemId);
            if (origItem is null)
                return Result<int>.Failure($"بند المرتجع غير موجود في الفاتورة الأصلية");
            if (item.Quantity > origItem.RemainingQty)
                return Result<int>.Failure($"الكمية المرتجعة أكبر من الكمية المتاحة للإرجاع");

            ret.AddItem(item.InvoiceItemId, item.ProductId, item.UnitId,
                item.ProductNameAr, item.Quantity, item.UnitPrice);
        }

        await _uow.Sales.AddReturnAsync(ret, ct);

        // Return stock
        foreach (var item in ret.Items)
        {
            await _uow.Inventory.LogTransactionAsync(
                item.ProductId, cmd.WarehouseId, item.Quantity,
                StockDirection.In, InventoryTransactionType.SaleReturnIn,
                item.UnitPrice, ret.Id, "SalesReturn",
                null, cmd.UserId, ct);
        }

        // Update invoice status
        var totalReturnedValue = ret.TotalAmount;
        if (totalReturnedValue >= original.TotalAmount)
            original.MarkFullReturn();
        else
            original.MarkPartialReturn();

        _uow.Sales.Update(original);

        // Reduce customer debt if cash refund
        if (cmd.RefundMethod == RefundMethod.Cash && original.CustomerId.HasValue)
        {
            var customer = await _uow.Customers.GetByIdAsync(original.CustomerId.Value, ct);
            customer?.ReduceDebt(ret.TotalAmount);
            if (customer != null) _uow.Customers.Update(customer);
        }

        await _uow.SaveChangesAsync(ct);
        return Result<int>.Success(ret.Id);
    }
}
