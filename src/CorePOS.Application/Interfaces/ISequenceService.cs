namespace CorePOS.Application.Interfaces;

public interface ISequenceService
{
    Task<string> NextSaleInvoiceNoAsync(string branchCode, CancellationToken ct = default);
    Task<string> NextPurchaseInvoiceNoAsync(string branchCode, CancellationToken ct = default);
    Task<string> NextSaleReturnNoAsync(string branchCode, CancellationToken ct = default);
    Task<string> NextPurchaseReturnNoAsync(string branchCode, CancellationToken ct = default);
    Task<string> NextTransferNoAsync(CancellationToken ct = default);
    Task<string> NextAdjustmentNoAsync(CancellationToken ct = default);
    Task<string> NextInventorySessionNoAsync(CancellationToken ct = default);
    Task<string> NextExpenseNoAsync(CancellationToken ct = default);
    Task<string> NextShiftNoAsync(string userCode, CancellationToken ct = default);
    Task<string> NextCustomerPaymentNoAsync(CancellationToken ct = default);
    Task<string> NextSupplierPaymentNoAsync(CancellationToken ct = default);
    Task<string> NextProductCodeAsync(CancellationToken ct = default);
    Task<string> NextCustomerCodeAsync(CancellationToken ct = default);
    Task<string> NextSupplierCodeAsync(CancellationToken ct = default);
}
