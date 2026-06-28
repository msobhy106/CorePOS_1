using Microsoft.EntityFrameworkCore;
using CorePOS.Domain.Entities;
using CorePOS.Domain.Interfaces;
using CorePOS.Persistence.DbContexts;

namespace CorePOS.Persistence.Repositories;

public class BackupRepository : BaseRepository<Backup>, IBackupRepository
{
    public BackupRepository(CorePOSDbContext db) : base(db) { }

    public async Task<IReadOnlyList<Backup>> GetRecentAsync(int count = 20, CancellationToken ct = default)
        => await _set.OrderByDescending(b => b.CreatedAt).Take(count).ToListAsync(ct);

    public async Task<Backup?> GetLastSuccessfulAsync(CancellationToken ct = default)
        => await _set
            .Where(b => b.IsSuccessful)
            .OrderByDescending(b => b.CreatedAt)
            .FirstOrDefaultAsync(ct);
}
