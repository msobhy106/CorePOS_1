using MediatR;
using CorePOS.Application.Common;

namespace CorePOS.Application.Features.Shifts.Commands;

public record OpenShiftCommand(
    int     UserId,
    int     BranchId,
    int     CashBoxId,
    decimal OpeningBalance = 0
) : IRequest<Result<int>>;
