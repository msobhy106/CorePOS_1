using CorePOS.Domain.Entities;

namespace CorePOS.Domain.Interfaces;

public interface ISupplierRepository : IRepository<Supplier>
{
    Task<Supplier?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<Supplier>> SearchAsync(string term, int maxResults = 20, CancellationToken ct = default);
    Task<IReadOnlyList<Supplier>> GetActiveAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Supplier>> GetWithPayablesAsync(CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default);
    Task<string> GenerateNextCodeAsync(CancellationToken ct = default);
    Task UpdateBalanceAsync(int supplierId, decimal newBalance, CancellationToken ct = default);
}
