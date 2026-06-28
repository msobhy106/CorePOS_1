using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Shifts.DTOs;

namespace CorePOS.Application.Features.Shifts.Queries;

public record GetCurrentShiftQuery(int UserId) : IRequest<Result<ShiftDto?>>;
public record GetShiftByIdQuery(int ShiftId)   : IRequest<Result<ShiftDto>>;
