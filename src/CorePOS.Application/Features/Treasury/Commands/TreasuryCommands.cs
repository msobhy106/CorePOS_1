using MediatR;
using CorePOS.Application.Common;

namespace CorePOS.Application.Features.Treasury.Commands;

public record DepositCommand(int CashBoxId, decimal Amount, string? Description, int UserId, int? ShiftId = null) : IRequest<Result>;
public record WithdrawCommand(int CashBoxId, decimal Amount, string? Description, int UserId, int? ShiftId = null) : IRequest<Result>;
public record TransferCommand(int FromCashBoxId, int ToCashBoxId, decimal Amount, string? Notes, int UserId) : IRequest<Result>;
public record DailyClosingCommand(int CashBoxId, int UserId) : IRequest<Result>;
