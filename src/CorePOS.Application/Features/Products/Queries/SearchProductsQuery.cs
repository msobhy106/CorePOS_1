using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Products.DTOs;

namespace CorePOS.Application.Features.Products.Queries;

/// <summary>Fast POS search — by barcode, name, or code.</summary>
public record SearchProductsQuery(
    string Term,
    int    MaxResults  = 20,
    int?   WarehouseId = null
) : IRequest<Result<IReadOnlyList<ProductSearchResultDto>>>;
