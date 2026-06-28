using Microsoft.EntityFrameworkCore;
using CorePOS.Domain.Entities;
using CorePOS.Domain.Interfaces;
using CorePOS.Persistence.DbContexts;

namespace CorePOS.Persistence.Repositories;

public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    public ProductRepository(CorePOSDbContext db) : base(db) { }

    public async Task<Product?> GetByBarcodeAsync(string barcode, CancellationToken ct = default)
        => await _set
            .Include(p => p.Category)
            .Include(p => p.SaleUnit)
            .Include(p => p.BaseUnit)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => (p.Barcode == barcode || p.SecondBarcode == barcode)
                                   && p.IsActive, ct);

    public async Task<Product?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await _set.FirstOrDefaultAsync(p => p.Code == code.ToUpperInvariant(), ct);

    public async Task<Product?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default)
        => await _set
            .Include(p => p.Category).ThenInclude(c => c!.Parent)
            .Include(p => p.BaseUnit)
            .Include(p => p.SaleUnit)
            .Include(p => p.PurchaseUnit)
            .Include(p => p.DefaultSupplier)
            .Include(p => p.Images)
            .Include(p => p.Units).ThenInclude(u => u.Unit)
            .Include(p => p.Stock)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Product>> SearchAsync(
        string term, int maxResults = 20, CancellationToken ct = default)
    {
        var q = _set
            .Include(p => p.SaleUnit)
            .Include(p => p.Images)
            .Where(p => p.IsActive)
            .AsQueryable();

        // Search by barcode first (exact), then name/code (contains)
        if (term.All(char.IsDigit))
            q = q.Where(p => p.Barcode == term || p.SecondBarcode == term
                           || p.Code.Contains(term) || p.NameAr.Contains(term));
        else
            q = q.Where(p => p.NameAr.Contains(term) || p.Code.Contains(term)
                           || (p.NameEn != null && p.NameEn.Contains(term)));

        return await q.Take(maxResults).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetByCategoryAsync(
        int categoryId, bool includeSubCategories = true, CancellationToken ct = default)
    {
        if (!includeSubCategories)
            return await _set.Where(p => p.CategoryId == categoryId && p.IsActive).ToListAsync(ct);

        // Get all sub-category IDs
        var subIds = await _db.Categories
            .Where(c => c.ParentId == categoryId)
            .Select(c => c.Id)
            .ToListAsync(ct);
        subIds.Add(categoryId);

        return await _set
            .Include(p => p.SaleUnit)
            .Where(p => subIds.Contains(p.CategoryId) && p.IsActive)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetLowStockAsync(
        int? warehouseId = null, CancellationToken ct = default)
    {
        var query = _db.ProductStocks
            .Include(ps => ps.Product).ThenInclude(p => p.Category)
            .Include(ps => ps.Product).ThenInclude(p => p.SaleUnit)
            .Where(ps => ps.Product.IsActive
                      && ps.Product.MinStock > 0
                      && ps.Quantity <= ps.Product.MinStock);

        if (warehouseId.HasValue)
            query = query.Where(ps => ps.WarehouseId == warehouseId.Value);

        var stocks = await query.ToListAsync(ct);
        return stocks.Select(s => s.Product).Distinct().ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<Product>> GetExpiringAsync(
        int daysAhead = 30, CancellationToken ct = default)
    {
        var cutoff = DateOnly.FromDateTime(DateTime.Today.AddDays(daysAhead));
        return await _set
            .Where(p => p.HasExpiry && p.ExpiryDate.HasValue && p.ExpiryDate <= cutoff && p.IsActive)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetActiveAsync(CancellationToken ct = default)
        => await _set.Where(p => p.IsActive).ToListAsync(ct);

    public async Task<bool> BarcodeExistsAsync(
        string barcode, int? excludeProductId = null, CancellationToken ct = default)
        => await _set.AnyAsync(p =>
            (p.Barcode == barcode || p.SecondBarcode == barcode)
            && (excludeProductId == null || p.Id != excludeProductId), ct);

    public async Task<bool> CodeExistsAsync(
        string code, int? excludeProductId = null, CancellationToken ct = default)
        => await _set.AnyAsync(p =>
            p.Code == code.ToUpperInvariant()
            && (excludeProductId == null || p.Id != excludeProductId), ct);

    public async Task<ProductStock?> GetStockAsync(
        int productId, int warehouseId, CancellationToken ct = default)
        => await _db.ProductStocks
            .FirstOrDefaultAsync(s => s.ProductId == productId
                                   && s.WarehouseId == warehouseId, ct);

    public async Task<IReadOnlyList<ProductStock>> GetAllStockAsync(
        int productId, CancellationToken ct = default)
        => await _db.ProductStocks
            .Include(s => s.Warehouse)
            .Where(s => s.ProductId == productId)
            .ToListAsync(ct);

    public async Task<decimal> GetTotalStockAsync(int productId, CancellationToken ct = default)
        => await _db.ProductStocks
            .Where(s => s.ProductId == productId)
            .SumAsync(s => s.Quantity, ct);

    public async Task<string> GenerateNextCodeAsync(CancellationToken ct = default)
    {
        var seq = await _db.Database.ExecuteSqlRawAsync(
            "EXEC usp_GetNextSequence @SequenceKey = N'ProductCode'", ct);
        var val = await _db.Set<Sequence>()
            .Where(s => s.SequenceKey == "ProductCode")
            .Select(s => s.CurrentValue)
            .FirstOrDefaultAsync(ct);
        return $"PRD{val:D6}";
    }
}

// Internal helper to access Sequences table
internal class Sequence
{
    public int    Id           { get; set; }
    public string SequenceKey  { get; set; } = string.Empty;
    public int    CurrentValue { get; set; }
    public string? Prefix      { get; set; }
}
