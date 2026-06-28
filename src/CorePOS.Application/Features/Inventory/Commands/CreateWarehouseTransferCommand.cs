using MediatR;
using CorePOS.Application.Common;

namespace CorePOS.Application.Features.Inventory.Commands;

public record TransferItemInput(int ProductId, string ProductNameAr, decimal Quantity, decimal UnitCost = 0);

public record CreateWarehouseTransferCommand(
    int                   FromWarehouseId,
    int                   ToWarehouseId,
    int                   FromBranchId,
    int                   ToBranchId,
    List<TransferItemInput> Items,
    int                   UserId,
    string?               Notes = null,
    bool                  AutoApprove = false
) : IRequest<Result<int>>;
