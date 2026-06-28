using CorePOS.Domain.Entities;

namespace CorePOS.Domain.Interfaces;

public interface IPurchaseRepository : IRepository<PurchaseInvoice>
{
    Task<PurchaseInvoice?> GetByInvoiceNoAsync(string invoiceNo, CancellationToken ct = default);
    Task<PurchaseInvoice?> GetByIdWithItemsAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<PurchaseInvoice>> GetByDateRangeAsync(DateTime from, DateTime to,
        int? branchId = null, CancellationToken ct = default);

    Task<IReadOnlyList<PurchaseInvoice>> GetBySupplierAsync(int supplierId,
        DateTime? from = null, DateTime? to = null, CancellationToken ct = default);

    Task<IReadOnlyList<PurchaseInvoice>> GetPendingApprovalAsync(CancellationToken ct = default);

    Task<string> GenerateInvoiceNoAsync(string branchCode, CancellationToken ct = default);

    // Returns
    Task<PurchaseReturn?> GetReturnByIdAsync(int returnId, CancellationToken ct = default);
    Task AddReturnAsync(PurchaseReturn purchaseReturn, CancellationToken ct = default);
    Task<string> GenerateReturnNoAsync(string branchCode, CancellationToken ct = default);

    Task<decimal> GetTotalPurchasesAsync(DateTime from, DateTime to, int? branchId = null, CancellationToken ct = default);
}
