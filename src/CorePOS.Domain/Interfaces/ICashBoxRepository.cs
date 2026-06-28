using CorePOS.Domain.Entities;

namespace CorePOS.Domain.Interfaces;

public interface ICashBoxRepository : IRepository<CashBox>
{
    Task<IReadOnlyList<CashBox>> GetByBranchAsync(int branchId, CancellationToken ct = default);
    Task<CashBox?> GetMainCashBoxAsync(int branchId, CancellationToken ct = default);
    Task UpdateBalanceAsync(int cashBoxId, decimal newBalance, CancellationToken ct = default);
}
