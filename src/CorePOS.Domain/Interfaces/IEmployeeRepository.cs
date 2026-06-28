using CorePOS.Domain.Entities;

namespace CorePOS.Domain.Interfaces;

public interface IEmployeeRepository : IRepository<Employee>
{
    Task<IReadOnlyList<Employee>> GetActiveAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Employee>> GetByBranchAsync(int branchId, CancellationToken ct = default);
    Task<Employee?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<string> GenerateNextCodeAsync(CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default);
}
