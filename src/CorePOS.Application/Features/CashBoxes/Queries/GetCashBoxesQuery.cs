using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.CashBoxes.DTOs;

namespace CorePOS.Application.Features.CashBoxes.Queries;

public record GetCashBoxesQuery(int? BranchId = null)
    : IRequest<Result<IReadOnlyList<CashBoxDto>>>;
