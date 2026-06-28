using CorePOS.Domain.Entities;

namespace CorePOS.Application.Interfaces;

// ════════════════════════════════════════════════════════════════════
// REPORT SERVICE INTERFACE
// ════════════════════════════════════════════════════════════════════
/// <summary>
/// Contract for generating reports and invoice prints.
/// Returns byte[] — caller decides whether to show preview, save, or print directly.
/// format: "pdf" | "xlsx"
/// </summary>
public interface IReportService
{
    // ── Invoice printing ──────────────────────────────────────────
    Task<byte[]> GenerateInvoicePrintAsync(
        int invoiceId,
        string format = "pdf",
        CancellationToken ct = default);

    // ── Sales reports ─────────────────────────────────────────────
    Task<byte[]> GenerateSalesReportAsync(
        DateTime from, DateTime to,
        int? branchId = null,
        string format = "pdf",
        CancellationToken ct = default);

    // ── Profit report ─────────────────────────────────────────────
    Task<byte[]> GenerateProfitReportAsync(
        DateTime from, DateTime to,
        int? branchId = null,
        string format = "pdf",
        CancellationToken ct = default);

    // ── Inventory report ──────────────────────────────────────────
    Task<byte[]> GenerateInventoryReportAsync(
        int? warehouseId = null,
        string format = "pdf",
        CancellationToken ct = default);

    // ── Customer account statement ────────────────────────────────
    Task<byte[]> GenerateCustomerAccountAsync(
        int customerId,
        DateTime from, DateTime to,
        string format = "pdf",
        CancellationToken ct = default);

    // ── Supplier account ──────────────────────────────────────────
    Task<byte[]> GenerateSupplierAccountAsync(
        int supplierId,
        DateTime from, DateTime to,
        string format = "pdf",
        CancellationToken ct = default);

    // ── Shift report ──────────────────────────────────────────────
    Task<byte[]> GenerateShiftReportAsync(
        int shiftId,
        string format = "pdf",
        CancellationToken ct = default);
}

// ════════════════════════════════════════════════════════════════════
// PRINTER SERVICE INTERFACE
// ════════════════════════════════════════════════════════════════════
/// <summary>
/// Contract for thermal/receipt printing and cash drawer.
/// Implementation uses ESC/POS raw commands with Windows print API fallback.
/// </summary>
public interface IPrinterService
{
    /// <summary>Print a sales invoice receipt on the configured thermal printer.</summary>
    Task PrintInvoiceAsync(
        SalesInvoice invoice,
        CancellationToken ct = default);

    /// <summary>Send pulse to open the cash drawer.</summary>
    Task OpenCashDrawerAsync(
        string? printerName = null,
        CancellationToken ct = default);

    /// <summary>Print a test page to verify printer connectivity.</summary>
    Task PrintTestPageAsync(
        string printerName,
        CancellationToken ct = default);
}

// ════════════════════════════════════════════════════════════════════
// SETTINGS REPOSITORY INTERFACE
// ════════════════════════════════════════════════════════════════════
/// <summary>
/// Key-value settings store (used by ReportService and PrinterService).
/// Settings are stored in the Settings table.
/// </summary>
public interface ISettingsRepository
{
    Task<string>  GetStringAsync(string key, string defaultValue = "",  CancellationToken ct = default);
    Task<bool>    GetBoolAsync  (string key, bool   defaultValue = false, CancellationToken ct = default);
    Task<int>     GetIntAsync   (string key, int    defaultValue = 0,   CancellationToken ct = default);
    Task<decimal> GetDecimalAsync(string key, decimal defaultValue = 0, CancellationToken ct = default);

    Task SetStringAsync(string key, string value,  CancellationToken ct = default);
    Task SetBoolAsync  (string key, bool   value,  CancellationToken ct = default);
    Task SetIntAsync   (string key, int    value,  CancellationToken ct = default);
}
