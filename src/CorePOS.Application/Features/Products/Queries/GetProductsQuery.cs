using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Products.DTOs;

namespace CorePOS.Application.Features.Products.Queries;

public record GetProductsQuery(
    int?   CategoryId  = null,
    bool?  IsActive    = null,
    string? Search     = null,
    int    PageNumber  = 1,
    int    PageSize    = 50,
    int?   WarehouseId = null
) : IRequest<Result<PagedResult<ProductListDto>>>;
