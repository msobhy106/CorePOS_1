using Microsoft.EntityFrameworkCore;
using CorePOS.Domain.Entities;
using CorePOS.Domain.Interfaces;
using CorePOS.Persistence.DbContexts;

namespace CorePOS.Persistence.Repositories;

public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
{
    public CategoryRepository(CorePOSDbContext db) : base(db) { }

    public async Task<IReadOnlyList<Category>> GetActiveAsync(CancellationToken ct = default)
        => await _set.Where(c => c.IsActive).OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToListAsync(ct);

    public async Task<IReadOnlyList<Category>> GetRootCategoriesAsync(CancellationToken ct = default)
        => await _set.Where(c => c.ParentId == null && c.IsActive)
            .OrderBy(c => c.SortOrder).ToListAsync(ct);

    public async Task<IReadOnlyList<Category>> GetChildrenAsync(int parentId, CancellationToken ct = default)
        => await _set.Where(c => c.ParentId == parentId && c.IsActive)
            .OrderBy(c => c.SortOrder).ToListAsync(ct);

    public async Task<Category?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await _set.FirstOrDefaultAsync(c => c.Code == code.ToUpperInvariant(), ct);

    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default)
        => await _set.AnyAsync(c =>
            c.Code == code.ToUpperInvariant() && (excludeId == null || c.Id != excludeId), ct);
}
