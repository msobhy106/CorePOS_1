using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Inventory.DTOs;

namespace CorePOS.Application.Features.Inventory.Queries;

public record GetStockBalanceQuery(
    int?   WarehouseId  = null,
    int?   CategoryId   = null,
    bool   LowStockOnly = false,
    string? Search      = null,
    int    PageNumber   = 1,
    int    PageSize     = 50
) : IRequest<Result<PagedResult<StockBalanceDto>>>;

public record GetProductMovementQuery(
    int      ProductId,
    int?     WarehouseId = null,
    DateTime? From       = null,
    DateTime? To         = null
) : IRequest<Result<IReadOnlyList<StockMovementDto>>>;
