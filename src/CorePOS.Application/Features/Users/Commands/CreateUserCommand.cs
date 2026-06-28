using MediatR;
using CorePOS.Application.Common;

namespace CorePOS.Application.Features.Users.Commands;

public record CreateUserCommand(
    string  Username,
    string  Password,
    string  FullName,
    string? FullNameAr,
    int     RoleId,
    int?    BranchId    = null,
    int?    WarehouseId = null,
    string? Email       = null,
    string? Phone       = null
) : IRequest<Result<int>>;
