using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Units.DTOs;

namespace CorePOS.Application.Features.Units.Queries;

public record GetUnitsQuery(bool? IsActive = null)
    : IRequest<Result<IReadOnlyList<UnitDto>>>;
