using CorePOS.Domain.Entities;

namespace CorePOS.Domain.Interfaces;

public interface IShiftRepository : IRepository<Shift>
{
    Task<Shift?> GetOpenShiftAsync(int userId, CancellationToken ct = default);
    Task<Shift?> GetOpenShiftByBranchAsync(int branchId, CancellationToken ct = default);
    Task<IReadOnlyList<Shift>> GetRecentAsync(int branchId, DateTime from, CancellationToken ct = default);
    Task<string?> GetLastShiftNoAsync(int branchId, CancellationToken ct = default);
    Task<string> GenerateShiftNoAsync(int branchId, CancellationToken ct = default);
}
