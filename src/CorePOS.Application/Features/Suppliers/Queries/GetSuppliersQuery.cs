using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Suppliers.DTOs;

namespace CorePOS.Application.Features.Suppliers.Queries;

public record GetSuppliersQuery(
    string? Search   = null,
    bool?   IsActive = null,
    bool    DebtOnly = false,
    int     PageNumber = 1,
    int     PageSize   = 50
) : IRequest<Result<PagedResult<SupplierListDto>>>;

public record GetSupplierByIdQuery(int Id) : IRequest<Result<SupplierDto>>;
