using CorePOS.Domain.Entities;

namespace CorePOS.Domain.Interfaces;

public interface IExpenseRepository : IRepository<Expense>
{
    Task<IReadOnlyList<Expense>> GetByDateRangeAsync(DateTime from, DateTime to, int? branchId = null, CancellationToken ct = default);
    Task<IReadOnlyList<Expense>> GetByShiftAsync(int shiftId, CancellationToken ct = default);
    Task<decimal> GetTotalByDateRangeAsync(DateTime from, DateTime to, int? branchId = null, CancellationToken ct = default);
    Task<string> GenerateExpenseNoAsync(CancellationToken ct = default);
}
