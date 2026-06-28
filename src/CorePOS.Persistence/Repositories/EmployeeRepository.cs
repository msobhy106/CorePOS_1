using Microsoft.EntityFrameworkCore;
using CorePOS.Domain.Entities;
using CorePOS.Domain.Interfaces;
using CorePOS.Persistence.DbContexts;

namespace CorePOS.Persistence.Repositories;

public class EmployeeRepository : BaseRepository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(CorePOSDbContext db) : base(db) { }

    public async Task<IReadOnlyList<Employee>> GetActiveAsync(CancellationToken ct = default)
        => await _set.Where(e => e.IsActive).OrderBy(e => e.Name).ToListAsync(ct);

    public async Task<IReadOnlyList<Employee>> GetByBranchAsync(int branchId, CancellationToken ct = default)
        => await _set.Where(e => e.BranchId == branchId && e.IsActive).ToListAsync(ct);

    public async Task<Employee?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await _set.FirstOrDefaultAsync(e => e.Code == code.ToUpperInvariant(), ct);

    public async Task<string> GenerateNextCodeAsync(CancellationToken ct = default)
    {
        var count = await _set.CountAsync(ct);
        return $"EMP-{count + 1:D4}";
    }

    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default)
        => await _set.AnyAsync(e =>
            e.Code == code.ToUpperInvariant() && (excludeId == null || e.Id != excludeId), ct);
}
