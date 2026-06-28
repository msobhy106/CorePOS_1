using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using CorePOS.Application.Interfaces;
using CorePOS.Persistence.DbContexts;

namespace CorePOS.Infrastructure.Services;

public class SequenceService : ISequenceService
{
    private readonly CorePOSDbContext _db;
    private readonly SemaphoreSlim    _lock = new(1, 1);

    public SequenceService(CorePOSDbContext db) => _db = db;

    private async Task<int> GetNextAsync(string key, CancellationToken ct)
    {
        await _lock.WaitAsync(ct);
        try
        {
            // Atomic increment via raw SQL matching usp_GetNextSequence stored proc
            await _db.Database.ExecuteSqlRawAsync(
                $"EXEC usp_GetNextSequence @SequenceKey = N'{key}'", ct);

            return await _db.Database.SqlQuery<int>(
                $"SELECT CurrentValue FROM Sequences WHERE SequenceKey = {key}")
                .FirstOrDefaultAsync(ct);
        }
        finally { _lock.Release(); }
    }

    public async Task<string> NextSaleInvoiceNoAsync(string branchCode, CancellationToken ct = default)
    {
        var seq = await GetNextAsync("Sales", ct);
        return $"S-{branchCode}-{DateTime.Today:yyyyMMdd}-{seq:D5}";
    }

    public async Task<string> NextPurchaseInvoiceNoAsync(string branchCode, CancellationToken ct = default)
    {
        var seq = await GetNextAsync("Purchases", ct);
        return $"P-{branchCode}-{DateTime.Today:yyyyMMdd}-{seq:D5}";
    }

    public async Task<string> NextSaleReturnNoAsync(string branchCode, CancellationToken ct = default)
    {
        var seq = await GetNextAsync("SalesReturn", ct);
        return $"SR-{branchCode}-{DateTime.Today:yyyyMMdd}-{seq:D5}";
    }

    public async Task<string> NextPurchaseReturnNoAsync(string branchCode, CancellationToken ct = default)
    {
        var seq = await GetNextAsync("PurchaseReturn", ct);
        return $"PR-{branchCode}-{DateTime.Today:yyyyMMdd}-{seq:D5}";
    }

    public async Task<string> NextTransferNoAsync(CancellationToken ct = default)
    {
        var seq = await GetNextAsync("Transfer", ct);
        return $"WT-{DateTime.Today:yyyyMMdd}-{seq:D4}";
    }

    public async Task<string> NextAdjustmentNoAsync(CancellationToken ct = default)
    {
        var seq = await GetNextAsync("Adjustment", ct);
        return $"ADJ-{DateTime.Today:yyyyMMdd}-{seq:D4}";
    }

    public async Task<string> NextInventorySessionNoAsync(CancellationToken ct = default)
    {
        var seq = await GetNextAsync("InventoryCount", ct);
        return $"INV-{DateTime.Today:yyyyMMdd}-{seq:D4}";
    }

    public async Task<string> NextExpenseNoAsync(CancellationToken ct = default)
    {
        var seq = await GetNextAsync("Expense", ct);
        return $"EXP-{DateTime.Today:yyyyMMdd}-{seq:D4}";
    }

    public async Task<string> NextShiftNoAsync(string userCode, CancellationToken ct = default)
    {
        var seq = await GetNextAsync("Shift", ct);
        return $"SHF-{userCode}-{DateTime.Today:yyyyMMdd}-{seq:D4}";
    }

    public async Task<string> NextCustomerPaymentNoAsync(CancellationToken ct = default)
    {
        var seq = await GetNextAsync("CustomerPayment", ct);
        return $"CP-{DateTime.Today:yyyyMMdd}-{seq:D5}";
    }

    public async Task<string> NextSupplierPaymentNoAsync(CancellationToken ct = default)
    {
        var seq = await GetNextAsync("SupplierPayment", ct);
        return $"SP-{DateTime.Today:yyyyMMdd}-{seq:D5}";
    }

    public async Task<string> NextProductCodeAsync(CancellationToken ct = default)
    {
        var seq = await GetNextAsync("ProductCode", ct);
        return $"PRD{seq:D6}";
    }

    public async Task<string> NextCustomerCodeAsync(CancellationToken ct = default)
    {
        var seq = await GetNextAsync("CustomerPayment", ct);
        return $"C{seq:D6}";
    }

    public async Task<string> NextSupplierCodeAsync(CancellationToken ct = default)
    {
        var seq = await GetNextAsync("SupplierPayment", ct);
        return $"SUP{seq:D5}";
    }
}
