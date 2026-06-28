using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Inventory.Commands;
using CorePOS.Domain.Enums;
using CorePOS.Domain.Interfaces;

namespace CorePOS.Application.Features.Inventory.Handlers;

public class ApproveInventorySessionHandler : IRequestHandler<ApproveInventorySessionCommand, Result>
{
    private readonly IUnitOfWork _uow;
    public ApproveInventorySessionHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(ApproveInventorySessionCommand cmd, CancellationToken ct)
    {
        var session = await _uow.Inventory.GetSessionByIdAsync(cmd.SessionId, ct);
        if (session is null) return Result.NotFound("جلسة الجرد غير موجودة");

        try { session.Approve(cmd.ApprovedBy); }
        catch (InvalidOperationException ex) { return Result.Failure(ex.Message); }

        foreach (var item in session.GetDifferences())
        {
            if (item.Difference > 0)
                await _uow.Inventory.LogTransactionAsync(
                    item.ProductId, session.WarehouseId, item.Difference,
                    StockDirection.In, InventoryTransactionType.InventoryCountAdjust,
                    item.UnitCost, session.Id, "InventorySession", null, cmd.ApprovedBy, ct);
            else
                await _uow.Inventory.LogTransactionAsync(
                    item.ProductId, session.WarehouseId, Math.Abs(item.Difference),
                    StockDirection.Out, InventoryTransactionType.InventoryCountAdjust,
                    item.UnitCost, session.Id, "InventorySession", null, cmd.ApprovedBy, ct);
        }

        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
