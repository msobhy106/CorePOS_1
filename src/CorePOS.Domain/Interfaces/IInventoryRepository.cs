using CorePOS.Domain.Entities;
using CorePOS.Domain.Enums;

namespace CorePOS.Domain.Interfaces;

public interface IInventoryRepository
{
    // Stock
    Task<ProductStock?> GetStockAsync(int productId, int warehouseId, CancellationToken ct = default);
    Task<IReadOnlyList<ProductStock>> GetAllStockByProductAsync(int productId, CancellationToken ct = default);
    Task<IReadOnlyList<ProductStock>> GetAllStockByWarehouseAsync(int warehouseId, CancellationToken ct = default);
    Task UpsertStockAsync(ProductStock stock, CancellationToken ct = default);

    // Movement log
    Task LogTransactionAsync(int productId, int warehouseId, decimal quantity,
        StockDirection direction, InventoryTransactionType type,
        decimal unitCost, int? referenceId, string? referenceType,
        string? notes, int? userId, CancellationToken ct = default);

    Task<IReadOnlyList<InventoryTransaction>> GetMovementHistoryAsync(int productId,
        int? warehouseId = null, DateTime? from = null, DateTime? to = null,
        CancellationToken ct = default);

    // Inventory sessions
    Task<InventorySession?> GetSessionByIdAsync(int sessionId, CancellationToken ct = default);
    Task<InventorySession?> GetOpenSessionAsync(int warehouseId, CancellationToken ct = default);
    Task AddSessionAsync(InventorySession session, CancellationToken ct = default);
    Task<string> GenerateSessionNoAsync(CancellationToken ct = default);

    // Transfers
    Task<WarehouseTransfer?> GetTransferByIdAsync(int transferId, CancellationToken ct = default);
    Task AddTransferAsync(WarehouseTransfer transfer, CancellationToken ct = default);
    Task<string> GenerateTransferNoAsync(CancellationToken ct = default);

    // Adjustments
    Task<StockAdjustment?> GetAdjustmentByIdAsync(int adjustmentId, CancellationToken ct = default);
    Task AddAdjustmentAsync(StockAdjustment adjustment, CancellationToken ct = default);
    Task<string> GenerateAdjustmentNoAsync(CancellationToken ct = default);

    // Reports (BUG-010)
    Task<IReadOnlyList<ProductStock>> GetLowStockAsync(int branchId, CancellationToken ct = default);
    Task<IReadOnlyList<ProductStock>> GetSlowMovingAsync(int branchId, DateTime from, DateTime to, CancellationToken ct = default);
}
