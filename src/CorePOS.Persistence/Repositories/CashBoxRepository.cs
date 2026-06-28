using Microsoft.EntityFrameworkCore;
using CorePOS.Domain.Entities;
using CorePOS.Domain.Interfaces;
using CorePOS.Persistence.DbContexts;

namespace CorePOS.Persistence.Repositories;

public class CashBoxRepository : BaseRepository<CashBox>, ICashBoxRepository
{
    public CashBoxRepository(CorePOSDbContext db) : base(db) { }

    public async Task<IReadOnlyList<CashBox>> GetByBranchAsync(int branchId, CancellationToken ct = default)
        => await _set.Where(c => c.BranchId == branchId && c.IsActive).ToListAsync(ct);

    public async Task<CashBox?> GetMainCashBoxAsync(int branchId, CancellationToken ct = default)
        => await _set.FirstOrDefaultAsync(c => c.BranchId == branchId && c.IsMain && c.IsActive, ct);

    public async Task UpdateBalanceAsync(int cashBoxId, decimal newBalance, CancellationToken ct = default)
    {
        var cashBox = await _set.FindAsync(new object[] { cashBoxId }, ct);
        if (cashBox is not null)
        {
            cashBox.AdjustBalance(newBalance);
            // EF change tracking picks this up; caller must call SaveChangesAsync
        }
    }
}
