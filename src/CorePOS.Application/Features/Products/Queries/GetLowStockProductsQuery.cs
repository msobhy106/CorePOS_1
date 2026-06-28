using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Products.DTOs;

namespace CorePOS.Application.Features.Products.Queries;

public record GetLowStockProductsQuery(int? WarehouseId = null)
    : IRequest<Result<IReadOnlyList<ProductListDto>>>;
