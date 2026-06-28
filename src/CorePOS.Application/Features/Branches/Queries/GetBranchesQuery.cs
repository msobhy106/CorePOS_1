using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Branches.DTOs;

namespace CorePOS.Application.Features.Branches.Queries;

public record GetBranchesQuery(bool? IsActive = null)
    : IRequest<Result<IReadOnlyList<BranchDto>>>;
