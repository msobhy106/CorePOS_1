using CorePOS.Domain.Entities;

namespace CorePOS.Domain.Interfaces;

public interface IWarehouseRepository : IRepository<Warehouse>
{
    Task<IReadOnlyList<Warehouse>> GetByBranchAsync(int branchId, CancellationToken ct = default);
    Task<IReadOnlyList<Warehouse>> GetActiveAsync(CancellationToken ct = default);
    Task<Warehouse?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default);
}
