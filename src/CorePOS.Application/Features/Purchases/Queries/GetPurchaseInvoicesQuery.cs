using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Purchases.DTOs;

namespace CorePOS.Application.Features.Purchases.Queries;

public record GetPurchaseInvoicesQuery(
    DateTime? From       = null,
    DateTime? To         = null,
    int?      SupplierId = null,
    int?      BranchId   = null,
    string?   Search     = null,
    int       PageNumber = 1,
    int       PageSize   = 30
) : IRequest<Result<PagedResult<PurchaseInvoiceListDto>>>;

public record GetPurchaseInvoiceByIdQuery(int Id)
    : IRequest<Result<PurchaseInvoiceDto>>;
