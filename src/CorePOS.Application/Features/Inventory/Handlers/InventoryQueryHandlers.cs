using MediatR;
using Microsoft.EntityFrameworkCore;
using CorePOS.Application.Common;
using CorePOS.Application.Interfaces;
using CorePOS.Application.Features.Inventory.Queries;
using CorePOS.Application.Features.Inventory.DTOs;

namespace CorePOS.Application.Features.Inventory.Handlers;

// ── GET STOCK BALANCE (paged) ───────────────────────────────────
public class GetStockBalanceHandler
    : IRequestHandler<GetStockBalanceQuery, Result<PagedResult<StockBalanceDto>>>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    private readonly IApplicationDbContext _db;

    public GetStockBalanceHandler(Domain.Interfaces.IUnitOfWork uow, IApplicationDbContext db)
    {
        _uow = uow;
        _db  = db;
    }

    public async Task<Result<PagedResult<StockBalanceDto>>> Handle(
        GetStockBalanceQuery request, CancellationToken ct)
    {
        try
        {
            // Use IApplicationDbContext for flexible stock queries
            var query = _db.ProductStocks
                .Include(ps => ps.Product).ThenInclude(p => p!.Category)
                .Include(ps => ps.Warehouse)
                .AsQueryable();

            if (request.WarehouseId.HasValue)
                query = query.Where(ps => ps.WarehouseId == request.WarehouseId.Value);

            if (request.CategoryId.HasValue)
                query = query.Where(ps => ps.Product != null && ps.Product.CategoryId == request.CategoryId.Value);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var s = request.Search.ToLower();
                query = query.Where(ps =>
                    ps.Product != null && (
                        ps.Product.NameAr.ToLower().Contains(s) ||
                        ps.Product.Code.ToLower().Contains(s) ||
                        (ps.Product.Barcode != null && ps.Product.Barcode.Contains(s))));
            }

            if (request.LowStockOnly)
                query = query.Where(ps =>
                    ps.Product != null && ps.Quantity <= ps.Product.MinStock);

            var total = await query.CountAsync(ct);
            var stocks = await query
                .OrderBy(ps => ps.Product!.NameAr)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(ct);

            var items = stocks.Select(ps => new StockBalanceDto
            {
                ProductId    = ps.ProductId,
                ProductCode  = ps.Product?.Code ?? "",
                Barcode      = ps.Product?.Barcode,
                ProductName  = ps.Product?.NameAr ?? "",
                CategoryName = ps.Product?.Category?.NameAr ?? "",
                WarehouseName= ps.Warehouse?.NameAr ?? "",
                CurrentStock = ps.Quantity,
                AverageCost  = ps.AverageCost,
                LastCost     = ps.LastCost,
                StockValue   = ps.Quantity * ps.AverageCost,
                MinStock     = ps.Product?.MinStock ?? 0,
                ReorderLevel = ps.Product?.ReorderLevel ?? 0,
                SalePrice    = ps.Product?.SalePrice ?? 0,
                IsLowStock   = ps.Product != null && ps.Quantity <= ps.Product.MinStock,
                StockStatus  = ps.Product != null && ps.Quantity <= 0 ? "نفد" :
                               ps.Product != null && ps.Quantity <= ps.Product.MinStock ? "منخفض" : "طبيعي"
            }).ToList();

            return Result<PagedResult<StockBalanceDto>>.Success(
                new PagedResult<StockBalanceDto>(items, total, request.PageNumber, request.PageSize));
        }
        catch (Exception ex) { return Result<PagedResult<StockBalanceDto>>.Failure(ex.Message); }
    }
}

// ── GET PRODUCT MOVEMENT ────────────────────────────────────────
public class GetProductMovementHandler
    : IRequestHandler<GetProductMovementQuery, Result<IReadOnlyList<StockMovementDto>>>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public GetProductMovementHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result<IReadOnlyList<StockMovementDto>>> Handle(
        GetProductMovementQuery request, CancellationToken ct)
    {
        try
        {
            var movements = await _uow.Inventory.GetMovementHistoryAsync(
                request.ProductId, request.WarehouseId, request.From, request.To, ct);

            var dtos = movements.Select(m => new StockMovementDto
            {
                TransactionDate = m.TransactionDate,
                TransactionType = m.TransactionType.ToString(),
                Direction       = m.Direction.ToString(),
                Quantity        = m.Quantity,
                UnitCost        = m.UnitCost,
                BalanceAfter    = m.BalanceAfter,
                ReferenceType   = m.ReferenceType,
                ReferenceId     = m.ReferenceId,
                Notes           = m.Notes
            }).ToList();

            return Result<IReadOnlyList<StockMovementDto>>.Success(dtos);
        }
        catch (Exception ex) { return Result<IReadOnlyList<StockMovementDto>>.Failure(ex.Message); }
    }
}
