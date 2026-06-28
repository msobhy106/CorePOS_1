namespace CorePOS.Application.Interfaces;

public enum PrinterSize { Mm58, Mm80, A5, A4 }

public interface IPrinterService
{
    Task<bool> PrintReceiptAsync(int invoiceId, PrinterSize size, CancellationToken ct = default);
    Task<bool> PrintBarcodeAsync(int productId, int copies = 1, CancellationToken ct = default);
    Task<bool> OpenCashDrawerAsync(CancellationToken ct = default);
    IReadOnlyList<string> GetAvailablePrinters();
    bool TestPrinter(string printerName);
}
