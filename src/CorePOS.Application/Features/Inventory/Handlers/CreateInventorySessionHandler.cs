using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Inventory.Commands;
using CorePOS.Application.Interfaces;
using CorePOS.Domain.Entities;
using CorePOS.Domain.Interfaces;

namespace CorePOS.Application.Features.Inventory.Handlers;

public class CreateInventorySessionHandler : IRequestHandler<CreateInventorySessionCommand, Result<int>>
{
    private readonly IUnitOfWork      _uow;
    private readonly ISequenceService _seq;

    public CreateInventorySessionHandler(IUnitOfWork uow, ISequenceService seq)
    {
        _uow = uow; _seq = seq;
    }

    public async Task<Result<int>> Handle(CreateInventorySessionCommand cmd, CancellationToken ct)
    {
        // Check no open session for this warehouse
        var open = await _uow.Inventory.GetOpenSessionAsync(cmd.WarehouseId, ct);
        if (open is not null)
            return Result<int>.Conflict("يوجد جلسة جرد مفتوحة بالفعل لهذا المخزن");

        var sessionNo = await _seq.NextInventorySessionNoAsync(ct);
        var session   = InventorySession.Create(sessionNo, cmd.WarehouseId,
            cmd.UserId, cmd.IsFull, cmd.Notes);

        foreach (var item in cmd.Items)
        {
            var stock = await _uow.Inventory.GetStockAsync(item.ProductId, cmd.WarehouseId, ct);
            session.AddItem(item.ProductId,
                stock?.Quantity ?? 0,
                item.ActualQuantity,
                stock?.AverageCost ?? 0);
        }

        await _uow.Inventory.AddSessionAsync(session, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<int>.Success(session.Id);
    }
}
