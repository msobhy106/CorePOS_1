using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Reports.DTOs;

namespace CorePOS.Application.Features.Reports.Queries;

public record GetDashboardQuery(int? BranchId = null, DateTime? Date = null)
    : IRequest<Result<DashboardDto>>;

public record GetSalesReportQuery(
    DateTime From,
    DateTime To,
    int?     BranchId = null
) : IRequest<Result<SalesReportDto>>;
