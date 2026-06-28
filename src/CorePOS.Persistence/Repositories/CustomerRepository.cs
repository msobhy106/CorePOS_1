using Microsoft.EntityFrameworkCore;
using CorePOS.Domain.Entities;
using CorePOS.Domain.Interfaces;
using CorePOS.Persistence.DbContexts;

namespace CorePOS.Persistence.Repositories;

public class CustomerRepository : BaseRepository<Customer>, ICustomerRepository
{
    public CustomerRepository(CorePOSDbContext db) : base(db) { }

    public async Task<Customer?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await _set.FirstOrDefaultAsync(c => c.Code == code.ToUpperInvariant(), ct);

    public async Task<Customer?> GetByPhoneAsync(string phone, CancellationToken ct = default)
        => await _set.FirstOrDefaultAsync(c => c.Phone == phone || c.Phone2 == phone, ct);

    public async Task<Customer?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default)
        => await _set
            .Include(c => c.Group)
            .Include(c => c.PriceList)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<Customer>> SearchAsync(
        string term, int maxResults = 20, CancellationToken ct = default)
        => await _set
            .Where(c => c.IsActive &&
                       (c.Name.Contains(term) || c.Code.Contains(term) ||
                       (c.Phone != null && c.Phone.Contains(term))))
            .Take(maxResults)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Customer>> GetActiveAsync(CancellationToken ct = default)
        => await _set.Include(c => c.Group).Where(c => c.IsActive).ToListAsync(ct);

    public async Task<IReadOnlyList<Customer>> GetWithDebtAsync(CancellationToken ct = default)
        => await _set.Where(c => c.IsActive && c.CurrentBalance > 0).ToListAsync(ct);

    public async Task<IReadOnlyList<Customer>> GetByGroupAsync(
        int groupId, CancellationToken ct = default)
        => await _set.Where(c => c.GroupId == groupId && c.IsActive).ToListAsync(ct);

    public async Task<bool> CodeExistsAsync(
        string code, int? excludeId = null, CancellationToken ct = default)
        => await _set.AnyAsync(c =>
            c.Code == code.ToUpperInvariant() &&
            (excludeId == null || c.Id != excludeId), ct);

    public async Task<string> GenerateNextCodeAsync(CancellationToken ct = default)
    {
        var count = await _set.CountAsync(ct);
        return $"C{(count + 1):D6}";
    }

    public async Task UpdateBalanceAsync(
        int customerId, decimal newBalance, CancellationToken ct = default)
        => await _db.Database.ExecuteSqlRawAsync(
            $"UPDATE Customers SET CurrentBalance = {newBalance} WHERE Id = {customerId}", ct);

    public async Task UpdatePointsAsync(
        int customerId, decimal newPoints, CancellationToken ct = default)
        => await _db.Database.ExecuteSqlRawAsync(
            $"UPDATE Customers SET TotalPoints = {newPoints} WHERE Id = {customerId}", ct);
}
