using Microsoft.EntityFrameworkCore;
using CorePOS.Domain.Entities;
using CorePOS.Domain.Interfaces;
using CorePOS.Persistence.DbContexts;

namespace CorePOS.Persistence.Repositories;

public class PurchaseRepository : BaseRepository<PurchaseInvoice>, IPurchaseRepository
{
    public PurchaseRepository(CorePOSDbContext db) : base(db) { }

    public async Task<PurchaseInvoice?> GetByInvoiceNoAsync(
        string invoiceNo, CancellationToken ct = default)
        => await _set.Include(i => i.Supplier).Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.InvoiceNo == invoiceNo, ct);

    public async Task<PurchaseInvoice?> GetByIdWithItemsAsync(
        int id, CancellationToken ct = default)
        => await _set
            .Include(i => i.Supplier)
            .Include(i => i.Branch)
            .Include(i => i.Warehouse)
            .Include(i => i.User)
            .Include(i => i.Items).ThenInclude(it => it.Product)
            .Include(i => i.Items).ThenInclude(it => it.Unit)
            .FirstOrDefaultAsync(i => i.Id == id, ct);

    public async Task<IReadOnlyList<PurchaseInvoice>> GetByDateRangeAsync(
        DateTime from, DateTime to, int? branchId = null, CancellationToken ct = default)
        => await _set
            .Include(i => i.Supplier)
            .Where(i => i.InvoiceDate >= from && i.InvoiceDate <= to
                     && (branchId == null || i.BranchId == branchId))
            .OrderByDescending(i => i.InvoiceDate).ToListAsync(ct);

    public async Task<IReadOnlyList<PurchaseInvoice>> GetBySupplierAsync(
        int supplierId, DateTime? from = null, DateTime? to = null,
        CancellationToken ct = default)
    {
        var q = _set.Where(i => i.SupplierId == supplierId);
        if (from.HasValue) q = q.Where(i => i.InvoiceDate >= from.Value);
        if (to.HasValue)   q = q.Where(i => i.InvoiceDate <= to.Value);
        return await q.OrderByDescending(i => i.InvoiceDate).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PurchaseInvoice>> GetPendingApprovalAsync(
        CancellationToken ct = default)
        => await _set.Include(i => i.Supplier)
            .Where(i => i.Status == Domain.Enums.PurchaseInvoiceStatus.Draft)
            .OrderByDescending(i => i.InvoiceDate).ToListAsync(ct);

    public async Task<string> GenerateInvoiceNoAsync(
        string branchCode, CancellationToken ct = default)
    {
        var count = await _set.CountAsync(ct);
        return $"P-{branchCode}-{DateTime.Today:yyyyMMdd}-{(count + 1):D5}";
    }

    public async Task<PurchaseReturn?> GetReturnByIdAsync(
        int returnId, CancellationToken ct = default)
        => await _db.PurchaseReturns
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == returnId, ct);

    public async Task AddReturnAsync(
        PurchaseReturn purchaseReturn, CancellationToken ct = default)
        => await _db.PurchaseReturns.AddAsync(purchaseReturn, ct);

    public async Task<string> GenerateReturnNoAsync(
        string branchCode, CancellationToken ct = default)
    {
        var count = await _db.PurchaseReturns.CountAsync(ct);
        return $"PR-{branchCode}-{DateTime.Today:yyyyMMdd}-{(count + 1):D5}";
    }

    public async Task<decimal> GetTotalPurchasesAsync(
        DateTime from, DateTime to, int? branchId = null, CancellationToken ct = default)
        => await _set
            .Where(i => i.InvoiceDate >= from && i.InvoiceDate <= to
                     && i.Status == Domain.Enums.PurchaseInvoiceStatus.Approved
                     && (branchId == null || i.BranchId == branchId))
            .SumAsync(i => (decimal?)i.TotalAmount, ct) ?? 0;
}
