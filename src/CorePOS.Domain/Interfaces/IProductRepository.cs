using CorePOS.Domain.Entities;

namespace CorePOS.Domain.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetByBarcodeAsync(string barcode, CancellationToken ct = default);
    Task<Product?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<Product?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);

    /// <summary>Fast POS search — by name, barcode, or code. Returns top N results.</summary>
    Task<IReadOnlyList<Product>> SearchAsync(string term, int maxResults = 20, CancellationToken ct = default);

    Task<IReadOnlyList<Product>> GetByCategoryAsync(int categoryId, bool includeSubCategories = true, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetLowStockAsync(int? warehouseId = null, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetExpiringAsync(int daysAhead = 30, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetActiveAsync(CancellationToken ct = default);

    Task<bool> BarcodeExistsAsync(string barcode, int? excludeProductId = null, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, int? excludeProductId = null, CancellationToken ct = default);

    Task<ProductStock?> GetStockAsync(int productId, int warehouseId, CancellationToken ct = default);
    Task<IReadOnlyList<ProductStock>> GetAllStockAsync(int productId, CancellationToken ct = default);
    Task<decimal> GetTotalStockAsync(int productId, CancellationToken ct = default);

    Task<string> GenerateNextCodeAsync(CancellationToken ct = default);
}
