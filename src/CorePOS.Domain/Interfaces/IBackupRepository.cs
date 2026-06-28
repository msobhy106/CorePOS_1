using CorePOS.Domain.Entities;

namespace CorePOS.Domain.Interfaces;

public interface IBackupRepository : IRepository<Backup>
{
    Task<IReadOnlyList<Backup>> GetRecentAsync(int count = 20, CancellationToken ct = default);
    Task<Backup?> GetLastSuccessfulAsync(CancellationToken ct = default);
}
