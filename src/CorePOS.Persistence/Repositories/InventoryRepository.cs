using Microsoft.EntityFrameworkCore;
using CorePOS.Domain.Entities;
using CorePOS.Domain.Enums;
using CorePOS.Domain.Interfaces;
using CorePOS.Persistence.DbContexts;

namespace CorePOS.Persistence.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly CorePOSDbContext _db;
    public InventoryRepository(CorePOSDbContext db) => _db = db;

    // ── Stock ─────────────────────────────────────────────
    public async Task<ProductStock?> GetStockAsync(
        int productId, int warehouseId, CancellationToken ct = default)
        => await _db.ProductStocks
            .FirstOrDefaultAsync(s => s.ProductId == productId
                                   && s.WarehouseId == warehouseId, ct);

    public async Task<IReadOnlyList<ProductStock>> GetAllStockByProductAsync(
        int productId, CancellationToken ct = default)
        => await _db.ProductStocks
            .Include(s => s.Warehouse)
            .Where(s => s.ProductId == productId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ProductStock>> GetAllStockByWarehouseAsync(
        int warehouseId, CancellationToken ct = default)
        => await _db.ProductStocks
            .Include(s => s.Product).ThenInclude(p => p.Category)
            .Include(s => s.Product).ThenInclude(p => p.SaleUnit)
            .Where(s => s.WarehouseId == warehouseId)
            .ToListAsync(ct);

    public async Task UpsertStockAsync(ProductStock stock, CancellationToken ct = default)
    {
        var existing = await GetStockAsync(stock.ProductId, stock.WarehouseId, ct);
        if (existing is null)
            await _db.ProductStocks.AddAsync(stock, ct);
        else
            _db.ProductStocks.Update(existing);
    }

    // ── Movement Log ──────────────────────────────────────
    public async Task LogTransactionAsync(
        int productId, int warehouseId, decimal quantity,
        StockDirection direction, InventoryTransactionType type,
        decimal unitCost, int? referenceId, string? referenceType,
        string? notes, int? userId, CancellationToken ct = default)
    {
        // Update ProductStock
        var stock = await GetStockAsync(productId, warehouseId, ct);
        if (stock is null)
        {
            stock = ProductStock.Create(productId, warehouseId);
            await _db.ProductStocks.AddAsync(stock, ct);
        }

        if (direction == StockDirection.In)
            stock.AddStock(quantity, unitCost);
        else
            stock.RemoveStock(quantity);

        // Log the transaction
        var tx = InventoryTransaction.Create(productId, warehouseId, type,
            quantity, direction, unitCost, stock.Quantity,
            referenceId, referenceType, notes, userId);
        await _db.InventoryTransactions.AddAsync(tx, ct);
    }

    public async Task<IReadOnlyList<InventoryTransaction>> GetMovementHistoryAsync(
        int productId, int? warehouseId = null,
        DateTime? from = null, DateTime? to = null,
        CancellationToken ct = default)
    {
        var q = _db.InventoryTransactions
            .Include(t => t.Warehouse)
            .Include(t => t.User)
            .Where(t => t.ProductId == productId);

        if (warehouseId.HasValue) q = q.Where(t => t.WarehouseId == warehouseId.Value);
        if (from.HasValue)        q = q.Where(t => t.TransactionDate >= from.Value);
        if (to.HasValue)          q = q.Where(t => t.TransactionDate <= to.Value);

        return await q.OrderByDescending(t => t.TransactionDate).ToListAsync(ct);
    }

    // ── Inventory Sessions ────────────────────────────────
    public async Task<InventorySession?> GetSessionByIdAsync(
        int sessionId, CancellationToken ct = default)
        => await _db.InventorySessions
            .Include(s => s.Warehouse)
            .Include(s => s.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(s => s.Id == sessionId, ct);

    public async Task<InventorySession?> GetOpenSessionAsync(
        int warehouseId, CancellationToken ct = default)
        => await _db.InventorySessions
            .FirstOrDefaultAsync(s => s.WarehouseId == warehouseId && s.Status == 0, ct);

    public async Task AddSessionAsync(
        InventorySession session, CancellationToken ct = default)
        => await _db.InventorySessions.AddAsync(session, ct);

    public async Task<string> GenerateSessionNoAsync(CancellationToken ct = default)
    {
        var count = await _db.InventorySessions.CountAsync(ct);
        return $"INV-{DateTime.Today:yyyyMMdd}-{(count + 1):D4}";
    }

    // ── Transfers ─────────────────────────────────────────
    public async Task<WarehouseTransfer?> GetTransferByIdAsync(
        int transferId, CancellationToken ct = default)
        => await _db.WarehouseTransfers
            .Include(t => t.FromWarehouse)
            .Include(t => t.ToWarehouse)
            .Include(t => t.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(t => t.Id == transferId, ct);

    public async Task AddTransferAsync(
        WarehouseTransfer transfer, CancellationToken ct = default)
        => await _db.WarehouseTransfers.AddAsync(transfer, ct);

    public async Task<string> GenerateTransferNoAsync(CancellationToken ct = default)
    {
        var count = await _db.WarehouseTransfers.CountAsync(ct);
        return $"WT-{DateTime.Today:yyyyMMdd}-{(count + 1):D4}";
    }

    // ── Adjustments ───────────────────────────────────────
    public async Task<StockAdjustment?> GetAdjustmentByIdAsync(
        int adjustmentId, CancellationToken ct = default)
        => await _db.StockAdjustments
            .Include(a => a.Warehouse)
            .Include(a => a.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(a => a.Id == adjustmentId, ct);

    public async Task AddAdjustmentAsync(
        StockAdjustment adjustment, CancellationToken ct = default)
        => await _db.StockAdjustments.AddAsync(adjustment, ct);

    public async Task<string> GenerateAdjustmentNoAsync(CancellationToken ct = default)
    {
        var count = await _db.StockAdjustments.CountAsync(ct);
        return $"ADJ-{DateTime.Today:yyyyMMdd}-{(count + 1):D4}";
    }
}
