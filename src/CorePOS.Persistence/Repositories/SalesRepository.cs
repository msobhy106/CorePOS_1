using Microsoft.EntityFrameworkCore;
using CorePOS.Domain.Entities;
using CorePOS.Domain.Enums;
using CorePOS.Domain.Interfaces;
using CorePOS.Persistence.DbContexts;

namespace CorePOS.Persistence.Repositories;

public class SalesRepository : BaseRepository<SalesInvoice>, ISalesRepository
{
    public SalesRepository(CorePOSDbContext db) : base(db) { }

    public async Task<SalesInvoice?> GetByInvoiceNoAsync(
        string invoiceNo, CancellationToken ct = default)
        => await _set
            .Include(i => i.Customer)
            .Include(i => i.Items).ThenInclude(it => it.Product)
            .Include(i => i.Items).ThenInclude(it => it.Unit)
            .FirstOrDefaultAsync(i => i.InvoiceNo == invoiceNo, ct);

    public async Task<SalesInvoice?> GetByIdWithItemsAsync(
        int id, CancellationToken ct = default)
        => await _set
            .Include(i => i.Customer)
            .Include(i => i.Branch)
            .Include(i => i.Warehouse)
            .Include(i => i.User)
            .Include(i => i.Items).ThenInclude(it => it.Product)
            .Include(i => i.Items).ThenInclude(it => it.Unit)
            .FirstOrDefaultAsync(i => i.Id == id, ct);

    public async Task<IReadOnlyList<SalesInvoice>> GetByDateRangeAsync(
        DateTime from, DateTime to, int? branchId = null, CancellationToken ct = default)
        => await _set
            .Include(i => i.Customer)
            .Include(i => i.User)
            .Where(i => i.InvoiceDate >= from && i.InvoiceDate <= to
                     && i.Status == SaleInvoiceStatus.Completed
                     && (branchId == null || i.BranchId == branchId))
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<SalesInvoice>> GetByCustomerAsync(
        int customerId, DateTime? from = null, DateTime? to = null,
        CancellationToken ct = default)
    {
        var q = _set.Where(i => i.CustomerId == customerId);
        if (from.HasValue) q = q.Where(i => i.InvoiceDate >= from.Value);
        if (to.HasValue)   q = q.Where(i => i.InvoiceDate <= to.Value);
        return await q.OrderByDescending(i => i.InvoiceDate).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SalesInvoice>> GetByShiftAsync(
        int shiftId, CancellationToken ct = default)
        => await _set.Where(i => i.ShiftId == shiftId).ToListAsync(ct);

    public async Task<IReadOnlyList<SalesInvoice>> GetHeldInvoicesAsync(
        int? userId = null, CancellationToken ct = default)
    {
        var q = _set.Include(i => i.Customer)
                    .Include(i => i.Items)
                    .Where(i => i.Status == SaleInvoiceStatus.Held);
        if (userId.HasValue) q = q.Where(i => i.UserId == userId.Value);
        return await q.OrderByDescending(i => i.CreatedAt).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SalesInvoice>> SearchAsync(
        string term, CancellationToken ct = default)
        => await _set
            .Include(i => i.Customer)
            .Where(i => i.InvoiceNo.Contains(term) ||
                       (i.Customer != null && i.Customer.Name.Contains(term)))
            .OrderByDescending(i => i.InvoiceDate)
            .Take(50).ToListAsync(ct);

    public async Task<string> GenerateInvoiceNoAsync(
        string branchCode, CancellationToken ct = default)
    {
        var seq = await GetNextSequenceAsync("Sales", ct);
        return $"S-{branchCode}-{DateTime.Today:yyyyMMdd}-{seq:D5}";
    }

    public async Task<SalesReturn?> GetReturnByIdAsync(
        int returnId, CancellationToken ct = default)
        => await _db.SalesReturns
            .Include(r => r.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(r => r.Id == returnId, ct);

    public async Task<IReadOnlyList<SalesReturn>> GetReturnsByInvoiceAsync(
        int invoiceId, CancellationToken ct = default)
        => await _db.SalesReturns
            .Include(r => r.Items)
            .Where(r => r.OriginalInvoiceId == invoiceId)
            .ToListAsync(ct);

    public async Task AddReturnAsync(SalesReturn salesReturn, CancellationToken ct = default)
        => await _db.SalesReturns.AddAsync(salesReturn, ct);

    public async Task<string> GenerateReturnNoAsync(
        string branchCode, CancellationToken ct = default)
    {
        var seq = await GetNextSequenceAsync("SalesReturn", ct);
        return $"SR-{branchCode}-{DateTime.Today:yyyyMMdd}-{seq:D5}";
    }

    public async Task<decimal> GetTotalRevenueAsync(
        DateTime from, DateTime to, int? branchId = null, CancellationToken ct = default)
        => await _set
            .Where(i => i.InvoiceDate >= from && i.InvoiceDate <= to
                     && i.Status == SaleInvoiceStatus.Completed
                     && (branchId == null || i.BranchId == branchId))
            .SumAsync(i => (decimal?)i.TotalAmount, ct) ?? 0;

    public async Task<int> GetInvoiceCountAsync(
        DateTime from, DateTime to, int? branchId = null, CancellationToken ct = default)
        => await _set
            .CountAsync(i => i.InvoiceDate >= from && i.InvoiceDate <= to
                          && i.Status == SaleInvoiceStatus.Completed
                          && (branchId == null || i.BranchId == branchId), ct);

    private async Task<int> GetNextSequenceAsync(string key, CancellationToken ct)
    {
        var seq = await _db.Set<SequenceRecord>()
            .FirstOrDefaultAsync(s => s.SequenceKey == key, ct);
        if (seq is null) return 1;
        return seq.CurrentValue;
    }
}

// Internal EF model for Sequences table
internal class SequenceRecord
{
    public int    Id           { get; set; }
    public string SequenceKey  { get; set; } = string.Empty;
    public int    CurrentValue { get; set; }
    public string? Prefix      { get; set; }
}
