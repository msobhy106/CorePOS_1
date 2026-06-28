using MediatR;
using CorePOS.Application.Common;

namespace CorePOS.Application.Features.Inventory.Commands;

public record AdjustmentItemInput(int ProductId, string ProductNameAr,
    decimal Quantity, decimal UnitCost = 0, string? Reason = null);

public record CreateStockAdjustmentCommand(
    int                      WarehouseId,
    int                      Type,      // 0=Increase, 1=Decrease
    List<AdjustmentItemInput> Items,
    int                      UserId,
    string?                  Notes = null
) : IRequest<Result<int>>;
