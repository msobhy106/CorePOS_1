using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Inventory.Commands;
using CorePOS.Application.Interfaces;
using CorePOS.Domain.Entities;
using CorePOS.Domain.Enums;
using CorePOS.Domain.Interfaces;

namespace CorePOS.Application.Features.Inventory.Handlers;

public class CreateWarehouseTransferHandler : IRequestHandler<CreateWarehouseTransferCommand, Result<int>>
{
    private readonly IUnitOfWork      _uow;
    private readonly ISequenceService _seq;

    public CreateWarehouseTransferHandler(IUnitOfWork uow, ISequenceService seq)
    {
        _uow = uow; _seq = seq;
    }

    public async Task<Result<int>> Handle(CreateWarehouseTransferCommand cmd, CancellationToken ct)
    {
        // Validate stock availability
        foreach (var item in cmd.Items)
        {
            var stock = await _uow.Inventory.GetStockAsync(item.ProductId, cmd.FromWarehouseId, ct);
            if ((stock?.Quantity ?? 0) < item.Quantity)
                return Result<int>.Failure($"الصنف '{item.ProductNameAr}' غير متوفر بالكمية المطلوبة");
        }

        var transferNo = await _seq.NextTransferNoAsync(ct);
        var transfer   = WarehouseTransfer.Create(transferNo, cmd.FromWarehouseId,
            cmd.ToWarehouseId, cmd.FromBranchId, cmd.ToBranchId, cmd.UserId, cmd.Notes);

        foreach (var item in cmd.Items)
            transfer.AddItem(item.ProductId, item.ProductNameAr, item.Quantity, item.UnitCost);

        await _uow.Inventory.AddTransferAsync(transfer, ct);
        await _uow.SaveChangesAsync(ct);

        if (cmd.AutoApprove)
        {
            transfer.Approve();
            foreach (var item in transfer.Items)
            {
                await _uow.Inventory.LogTransactionAsync(
                    item.ProductId, cmd.FromWarehouseId, item.Quantity,
                    StockDirection.Out, InventoryTransactionType.TransferOut,
                    item.UnitCost, transfer.Id, "WarehouseTransfer", null, cmd.UserId, ct);

                await _uow.Inventory.LogTransactionAsync(
                    item.ProductId, cmd.ToWarehouseId, item.Quantity,
                    StockDirection.In, InventoryTransactionType.TransferIn,
                    item.UnitCost, transfer.Id, "WarehouseTransfer", null, cmd.UserId, ct);
            }
            await _uow.SaveChangesAsync(ct);
        }

        return Result<int>.Success(transfer.Id);
    }
}
