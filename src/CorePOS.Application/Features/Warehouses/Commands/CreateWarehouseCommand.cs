using MediatR;
using CorePOS.Application.Common;

namespace CorePOS.Application.Features.Warehouses.Commands;

public record CreateWarehouseCommand(
    string  Code,
    string  Name,
    string  NameAr,
    int     BranchId,
    string? Address     = null,
    string? ManagerName = null,
    bool    IsMain      = false
) : IRequest<Result<int>>;
