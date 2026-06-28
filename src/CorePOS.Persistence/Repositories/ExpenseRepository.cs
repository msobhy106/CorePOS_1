using Microsoft.EntityFrameworkCore;
using CorePOS.Domain.Entities;
using CorePOS.Domain.Interfaces;
using CorePOS.Persistence.DbContexts;

namespace CorePOS.Persistence.Repositories;

public class ExpenseRepository : BaseRepository<Expense>, IExpenseRepository
{
    public ExpenseRepository(CorePOSDbContext db) : base(db) { }

    public async Task<IReadOnlyList<Expense>> GetByDateRangeAsync(
        DateTime from, DateTime to, int? branchId = null, CancellationToken ct = default)
    {
        var fromDate = DateOnly.FromDateTime(from);
        var toDate   = DateOnly.FromDateTime(to);
        return await _set
            .Include(e => e.Category)
            .Include(e => e.Branch)
            .Where(e => e.ExpenseDate >= fromDate
                     && e.ExpenseDate <= toDate
                     && (branchId == null || e.BranchId == branchId))
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Expense>> GetByShiftAsync(int shiftId, CancellationToken ct = default)
        => await _set
            .Include(e => e.Category)
            .Where(e => e.ShiftId == shiftId)
            .ToListAsync(ct);

    public async Task<decimal> GetTotalByDateRangeAsync(
        DateTime from, DateTime to, int? branchId = null, CancellationToken ct = default)
    {
        var fromDate = DateOnly.FromDateTime(from);
        var toDate   = DateOnly.FromDateTime(to);
        return await _set
            .Where(e => e.ExpenseDate >= fromDate
                     && e.ExpenseDate <= toDate
                     && (branchId == null || e.BranchId == branchId))
            .SumAsync(e => e.Amount, ct);
    }

    public async Task<string> GenerateExpenseNoAsync(CancellationToken ct = default)
    {
        var today  = DateTime.Today.ToString("yyyyMMdd");
        var todayD = DateOnly.FromDateTime(DateTime.Today);
        var count  = await _set.CountAsync(e => e.ExpenseDate == todayD, ct);
        return $"EXP-{today}-{count + 1:D3}";
    }
}
