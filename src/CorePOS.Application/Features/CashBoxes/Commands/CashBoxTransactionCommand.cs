using MediatR;
using CorePOS.Application.Common;
using CorePOS.Domain.Enums;

namespace CorePOS.Application.Features.CashBoxes.Commands;

public record DepositToCashBoxCommand(
    int     CashBoxId,
    decimal Amount,
    string? Description = null,
    int     UserId      = 0,
    int?    ShiftId     = null
) : IRequest<Result>;

public record WithdrawFromCashBoxCommand(
    int     CashBoxId,
    decimal Amount,
    string? Description = null,
    int     UserId      = 0,
    int?    ShiftId     = null
) : IRequest<Result>;

public record TransferBetweenCashBoxesCommand(
    int     FromCashBoxId,
    int     ToCashBoxId,
    decimal Amount,
    string? Notes   = null,
    int     UserId  = 0
) : IRequest<Result>;
