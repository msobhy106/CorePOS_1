using CorePOS.Domain.Entities;
using CorePOS.Domain.Enums;

namespace CorePOS.Domain.Interfaces;

public interface ISalesRepository : IRepository<SalesInvoice>
{
    Task<SalesInvoice?> GetByInvoiceNoAsync(string invoiceNo, CancellationToken ct = default);
    Task<SalesInvoice?> GetByIdWithItemsAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<SalesInvoice>> GetByDateRangeAsync(DateTime from, DateTime to,
        int? branchId = null, CancellationToken ct = default);

    Task<IReadOnlyList<SalesInvoice>> GetByCustomerAsync(int customerId,
        DateTime? from = null, DateTime? to = null, CancellationToken ct = default);

    Task<IReadOnlyList<SalesInvoice>> GetByShiftAsync(int shiftId, CancellationToken ct = default);

    Task<IReadOnlyList<SalesInvoice>> GetHeldInvoicesAsync(int? userId = null, CancellationToken ct = default);

    Task<IReadOnlyList<SalesInvoice>> SearchAsync(string term, CancellationToken ct = default);

    Task<string> GenerateInvoiceNoAsync(string branchCode, CancellationToken ct = default);

    // Returns
    Task<SalesReturn?> GetReturnByIdAsync(int returnId, CancellationToken ct = default);
    Task<IReadOnlyList<SalesReturn>> GetReturnsByInvoiceAsync(int invoiceId, CancellationToken ct = default);
    Task AddReturnAsync(SalesReturn salesReturn, CancellationToken ct = default);
    Task<string> GenerateReturnNoAsync(string branchCode, CancellationToken ct = default);

    // Reporting queries
    Task<decimal> GetTotalRevenueAsync(DateTime from, DateTime to, int? branchId = null, CancellationToken ct = default);
    Task<int>     GetInvoiceCountAsync(DateTime from, DateTime to, int? branchId = null, CancellationToken ct = default);

    // Shift-specific aggregates (BUG-009)
    Task<decimal> GetTotalByShiftAsync(int shiftId, CancellationToken ct = default);
    Task<decimal> GetReturnsTotalByShiftAsync(int shiftId, CancellationToken ct = default);
    Task<int>     GetCountByShiftAsync(int shiftId, CancellationToken ct = default);
}
