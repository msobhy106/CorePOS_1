using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Sales.DTOs;

namespace CorePOS.Application.Features.Sales.Queries;

public record GetSaleInvoicesQuery(
    DateTime? From        = null,
    DateTime? To          = null,
    int?      CustomerId  = null,
    int?      BranchId    = null,
    int?      ShiftId     = null,
    string?   Search      = null,
    int       PageNumber  = 1,
    int       PageSize    = 30
) : IRequest<Result<PagedResult<SalesInvoiceListDto>>>;
