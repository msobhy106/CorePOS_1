using Microsoft.EntityFrameworkCore;
using CorePOS.Domain.Entities;
using CorePOS.Domain.Interfaces;
using CorePOS.Persistence.DbContexts;

namespace CorePOS.Persistence.Repositories;

public class BranchRepository : BaseRepository<Branch>, IBranchRepository
{
    public BranchRepository(CorePOSDbContext db) : base(db) { }

    public async Task<IReadOnlyList<Branch>> GetActiveAsync(CancellationToken ct = default)
        => await _set.Where(b => b.IsActive).OrderBy(b => b.Name).ToListAsync(ct);

    public async Task<Branch?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await _set.FirstOrDefaultAsync(b => b.Code == code.ToUpperInvariant(), ct);

    public async Task<Branch?> GetMainBranchAsync(CancellationToken ct = default)
        => await _set.FirstOrDefaultAsync(b => b.IsMain && b.IsActive, ct);

    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default)
        => await _set.AnyAsync(b =>
            b.Code == code.ToUpperInvariant() && (excludeId == null || b.Id != excludeId), ct);
}
