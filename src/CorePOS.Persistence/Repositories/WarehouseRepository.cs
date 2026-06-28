using Microsoft.EntityFrameworkCore;
using CorePOS.Domain.Entities;
using CorePOS.Domain.Interfaces;
using CorePOS.Persistence.DbContexts;

namespace CorePOS.Persistence.Repositories;

public class WarehouseRepository : BaseRepository<Warehouse>, IWarehouseRepository
{
    public WarehouseRepository(CorePOSDbContext db) : base(db) { }

    public async Task<IReadOnlyList<Warehouse>> GetByBranchAsync(int branchId, CancellationToken ct = default)
        => await _set.Where(w => w.BranchId == branchId && w.IsActive).ToListAsync(ct);

    public async Task<IReadOnlyList<Warehouse>> GetActiveAsync(CancellationToken ct = default)
        => await _set.Where(w => w.IsActive).OrderBy(w => w.Name).ToListAsync(ct);

    public async Task<Warehouse?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await _set.FirstOrDefaultAsync(w => w.Code == code.ToUpperInvariant(), ct);

    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default)
        => await _set.AnyAsync(w =>
            w.Code == code.ToUpperInvariant() && (excludeId == null || w.Id != excludeId), ct);
}
