using MediatR;
using CorePOS.Application.Common;

namespace CorePOS.Application.Features.Inventory.Commands;

public record SessionItemInput(int ProductId, decimal ActualQuantity, string? Notes = null);

public record CreateInventorySessionCommand(
    int                   WarehouseId,
    bool                  IsFull,
    List<SessionItemInput> Items,
    int                   UserId,
    string?               Notes = null
) : IRequest<Result<int>>;

public record ApproveInventorySessionCommand(int SessionId, int ApprovedBy) : IRequest<Result>;
public record CancelInventorySessionCommand(int SessionId) : IRequest<Result>;
