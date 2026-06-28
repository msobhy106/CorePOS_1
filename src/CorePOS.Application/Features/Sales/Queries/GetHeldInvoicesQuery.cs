using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Sales.DTOs;

namespace CorePOS.Application.Features.Sales.Queries;

public record GetHeldInvoicesQuery(int? UserId = null)
    : IRequest<Result<IReadOnlyList<SalesInvoiceListDto>>>;
