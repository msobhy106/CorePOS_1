using MediatR;
using CorePOS.Application.Common;

namespace CorePOS.Application.Features.Branches.Commands;

public record CreateBranchCommand(
    string  Code,
    string  Name,
    string  NameAr,
    string? Address     = null,
    string? Phone       = null,
    string? ManagerName = null,
    bool    IsMain      = false
) : IRequest<Result<int>>;
