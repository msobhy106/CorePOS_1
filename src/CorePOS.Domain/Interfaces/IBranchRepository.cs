using CorePOS.Domain.Entities;

namespace CorePOS.Domain.Interfaces;

public interface IBranchRepository : IRepository<Branch>
{
    Task<IReadOnlyList<Branch>> GetActiveAsync(CancellationToken ct = default);
    Task<Branch?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<Branch?> GetMainBranchAsync(CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default);
}
