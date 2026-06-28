using MediatR;
using CorePOS.Application.Common;

namespace CorePOS.Application.Features.Shifts.Commands;

public record CloseShiftCommand(
    int     ShiftId,
    decimal ActualBalance,
    string? Notes = null
) : IRequest<Result>;
