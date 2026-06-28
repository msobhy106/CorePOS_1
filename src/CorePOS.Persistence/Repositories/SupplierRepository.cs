using Microsoft.EntityFrameworkCore;
using CorePOS.Domain.Entities;
using CorePOS.Domain.Interfaces;
using CorePOS.Persistence.DbContexts;

namespace CorePOS.Persistence.Repositories;

public class SupplierRepository : BaseRepository<Supplier>, ISupplierRepository
{
    public SupplierRepository(CorePOSDbContext db) : base(db) { }

    public async Task<Supplier?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await _set.FirstOrDefaultAsync(s => s.Code == code.ToUpperInvariant(), ct);

    public async Task<IReadOnlyList<Supplier>> SearchAsync(
        string term, int maxResults = 20, CancellationToken ct = default)
        => await _set
            .Where(s => s.IsActive &&
                       (s.Name.Contains(term) || s.Code.Contains(term) ||
                       (s.Phone != null && s.Phone.Contains(term))))
            .Take(maxResults).ToListAsync(ct);

    public async Task<IReadOnlyList<Supplier>> GetActiveAsync(CancellationToken ct = default)
        => await _set.Where(s => s.IsActive).ToListAsync(ct);

    public async Task<IReadOnlyList<Supplier>> GetWithPayablesAsync(CancellationToken ct = default)
        => await _set.Where(s => s.IsActive && s.CurrentBalance > 0).ToListAsync(ct);

    public async Task<bool> CodeExistsAsync(
        string code, int? excludeId = null, CancellationToken ct = default)
        => await _set.AnyAsync(s =>
            s.Code == code.ToUpperInvariant() &&
            (excludeId == null || s.Id != excludeId), ct);

    public async Task<string> GenerateNextCodeAsync(CancellationToken ct = default)
    {
        var count = await _set.CountAsync(ct);
        return $"SUP{(count + 1):D5}";
    }

    public async Task UpdateBalanceAsync(
        int supplierId, decimal newBalance, CancellationToken ct = default)
        => await _db.Database.ExecuteSqlRawAsync(
            $"UPDATE Suppliers SET CurrentBalance = {newBalance} WHERE Id = {supplierId}", ct);
}
