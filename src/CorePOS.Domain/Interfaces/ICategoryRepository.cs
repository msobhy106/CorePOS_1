using CorePOS.Domain.Entities;

namespace CorePOS.Domain.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IReadOnlyList<Category>> GetActiveAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetRootCategoriesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetChildrenAsync(int parentId, CancellationToken ct = default);
    Task<Category?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default);
}
