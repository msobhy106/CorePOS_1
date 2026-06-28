using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using CorePOS.Application.Interfaces;
using CorePOS.Application.Features.Reports.DTOs;
using CorePOS.Domain.Interfaces;

namespace CorePOS.Infrastructure.Printing;

public class ThermalPrinterService : IPrinterService
{
    private readonly ISettingsRepository _settings;
    private readonly IUnitOfWork         _uow;

    public ThermalPrinterService(ISettingsRepository settings, IUnitOfWork uow)
    {
        _settings = settings;
        _uow      = uow;
    }

    public async Task<bool> PrintReceiptAsync(
        int invoiceId, PrinterSize size, CancellationToken ct = default)
    {
        try
        {
            var invoice = await _uow.Sales.GetByIdWithItemsAsync(invoiceId, ct);
            if (invoice is null) return false;

            var printerName = await _settings.GetStringAsync("ReceiptPrinterName", "", ct);
            var companyName = await _settings.GetStringAsync("CompanyNameAr", "Core POS", ct);
            var footer      = await _settings.GetStringAsync("InvoiceFooterText", "شكراً لزيارتكم", ct);

            var lines = BuildReceiptLines(invoice, companyName, footer, size);

            return size switch
            {
                PrinterSize.Mm58 or PrinterSize.Mm80
                    => PrintRaw(printerName, lines, size),
                _ => PrintGraphic(printerName, lines, size)
            };
        }
        catch { return false; }
    }

    public async Task<bool> PrintBarcodeAsync(
        int productId, int copies = 1, CancellationToken ct = default)
    {
        try
        {
            var product = await _uow.Products.GetByIdWithDetailsAsync(productId, ct);
            if (product?.Barcode is null) return false;
            var printerName = await _settings.GetStringAsync("BarcodePrinterName", "", ct);
            if (string.IsNullOrEmpty(printerName)) return false;

            // Build ZPL label for barcode printer (Zebra compatible)
            var zpl = BuildZplLabel(product.Barcode, product.NameAr, product.SalePrice);
            return PrintRawZpl(printerName, zpl, copies);
        }
        catch { return false; }
    }

    public async Task<bool> OpenCashDrawerAsync(CancellationToken ct = default)
    {
        var printerName = await _settings.GetStringAsync("ReceiptPrinterName", "", ct);
        if (string.IsNullOrEmpty(printerName)) return false;
        // ESC/POS cash drawer open command: ESC p 0 25 250
        var cmd = new byte[] { 0x1B, 0x70, 0x00, 0x19, 0xFA };
        return SendRawBytes(printerName, cmd);
    }

    public IReadOnlyList<string> GetAvailablePrinters()
        => PrinterSettings.InstalledPrinters
            .Cast<string>()
            .ToList()
            .AsReadOnly();

    public bool TestPrinter(string printerName)
    {
        try
        {
            var doc = new PrintDocument { PrinterSettings = { PrinterName = printerName } };
            return doc.PrinterSettings.IsValid;
        }
        catch { return false; }
    }

    // ── Receipt Builder ───────────────────────────────────
    private static List<string> BuildReceiptLines(
        Domain.Entities.SalesInvoice invoice,
        string companyName, string footer, PrinterSize size)
    {
        int width = size == PrinterSize.Mm58 ? 32 : 48;
        var lines = new List<string>();

        lines.Add(CenterText(companyName, width));
        lines.Add(CenterText("─────────────────", width));
        lines.Add($"رقم الفاتورة: {invoice.InvoiceNo}");
        lines.Add($"التاريخ: {invoice.InvoiceDate:dd/MM/yyyy hh:mm tt}");
        if (invoice.Customer != null)
            lines.Add($"العميل: {invoice.Customer.Name}");
        lines.Add(new string('─', width));
        lines.Add($"{"الصنف",-20}{"الكمية",6}{"السعر",8}");
        lines.Add(new string('─', width));

        foreach (var item in invoice.Items)
        {
            var name = item.ProductNameAr.Length > 18
                ? item.ProductNameAr[..18] : item.ProductNameAr;
            lines.Add($"{name,-18}{item.Quantity,6:N2}{item.TotalPrice,8:N2}");
        }

        lines.Add(new string('─', width));
        lines.Add(RightAlign($"الإجمالي: {invoice.Subtotal:N2}", width));
        if (invoice.DiscountAmount > 0)
            lines.Add(RightAlign($"الخصم: {invoice.DiscountAmount:N2}", width));
        if (invoice.TaxAmount > 0)
            lines.Add(RightAlign($"الضريبة: {invoice.TaxAmount:N2}", width));
        lines.Add(RightAlign($"الصافي: {invoice.TotalAmount:N2}", width));
        lines.Add(RightAlign($"المدفوع: {invoice.PaidAmount:N2}", width));
        if (invoice.RemainingAmount > 0)
            lines.Add(RightAlign($"المتبقي: {invoice.RemainingAmount:N2}", width));
        lines.Add(new string('─', width));
        lines.Add(CenterText(footer, width));
        lines.Add("");
        lines.Add("");

        return lines;
    }

    private static string CenterText(string text, int width)
    {
        if (text.Length >= width) return text;
        int padding = (width - text.Length) / 2;
        return new string(' ', padding) + text;
    }

    private static string RightAlign(string text, int width)
        => text.Length >= width ? text : text.PadLeft(width);

    // ── Raw ESC/POS printing ──────────────────────────────
    private static bool PrintRaw(string printerName, List<string> lines, PrinterSize size)
    {
        try
        {
            var sb = new StringBuilder();
            // ESC/POS init
            sb.Append('\x1B'); sb.Append('@');
            // Right-to-left mode for Arabic (printer-dependent)
            foreach (var line in lines)
                sb.AppendLine(line);
            // Cut paper
            sb.Append('\x1D'); sb.Append('V'); sb.Append('\x00');

            var bytes = Encoding.GetEncoding(1256).GetBytes(sb.ToString());
            return SendRawBytes(printerName, bytes);
        }
        catch { return false; }
    }

    private static bool PrintGraphic(string printerName, List<string> lines, PrinterSize size)
    {
        try
        {
            var pd = new PrintDocument();
            pd.PrinterSettings.PrinterName = printerName;
            if (!pd.PrinterSettings.IsValid) return false;

            pd.PrintPage += (sender, e) =>
            {
                if (e.Graphics is null) return;
                var font = new Font("Arial", size == PrinterSize.A4 ? 11f : 10f,
                    FontStyle.Regular, GraphicsUnit.Point);
                var brush = Brushes.Black;
                float y = 10f;
                foreach (var line in lines)
                {
                    e.Graphics.DrawString(line, font, brush, new PointF(10f, y));
                    y += font.GetHeight(e.Graphics);
                }
                font.Dispose();
            };
            pd.Print();
            return true;
        }
        catch { return false; }
    }

    private static bool SendRawBytes(string printerName, byte[] data)
    {
        try
        {
            RawPrinterHelper.SendBytesToPrinter(printerName, data);
            return true;
        }
        catch { return false; }
    }

    private static string BuildZplLabel(string barcode, string name, decimal price)
        => $"^XA^FO50,30^BC^FD{barcode}^FS^FO50,110^FD{name}^FS^FO50,140^FD{price:N2}^FS^XZ";

    private static bool PrintRawZpl(string printerName, string zpl, int copies)
    {
        for (int i = 0; i < copies; i++)
        {
            var bytes = Encoding.UTF8.GetBytes(zpl);
            if (!SendRawBytes(printerName, bytes)) return false;
        }
        return true;
    }
}

/// <summary>Sends raw bytes to a Windows printer via P/Invoke.</summary>
internal static class RawPrinterHelper
{
    [System.Runtime.InteropServices.DllImport("winspool.drv", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
    private static extern bool OpenPrinter(string pPrinterName,
        out nint phPrinter, nint pDefault);

    [System.Runtime.InteropServices.DllImport("winspool.drv", SetLastError = true, ExactSpelling = true)]
    private static extern bool StartDocPrinter(nint hPrinter, int level, ref DOCINFOA pDocInfo);

    [System.Runtime.InteropServices.DllImport("winspool.drv", SetLastError = true, ExactSpelling = true)]
    private static extern bool EndDocPrinter(nint hPrinter);

    [System.Runtime.InteropServices.DllImport("winspool.drv", SetLastError = true, ExactSpelling = true)]
    private static extern bool StartPagePrinter(nint hPrinter);

    [System.Runtime.InteropServices.DllImport("winspool.drv", SetLastError = true, ExactSpelling = true)]
    private static extern bool EndPagePrinter(nint hPrinter);

    [System.Runtime.InteropServices.DllImport("winspool.drv", SetLastError = true, ExactSpelling = true)]
    private static extern bool WritePrinter(nint hPrinter, nint pBytes,
        int dwCount, out int dwWritten);

    [System.Runtime.InteropServices.DllImport("winspool.drv", SetLastError = true, ExactSpelling = true)]
    private static extern bool ClosePrinter(nint hPrinter);

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Ansi)]
    private struct DOCINFOA
    {
        [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPStr)]
        public string pDocName;
        [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPStr)]
        public string? pOutputFile;
        [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPStr)]
        public string pDataType;
    }

    public static void SendBytesToPrinter(string printerName, byte[] bytes)
    {
        if (!OpenPrinter(printerName, out nint hPrinter, 0))
            throw new InvalidOperationException($"Cannot open printer: {printerName}");
        try
        {
            var di = new DOCINFOA { pDocName = "CorePOS Receipt", pDataType = "RAW" };
            StartDocPrinter(hPrinter, 1, ref di);
            StartPagePrinter(hPrinter);
            var ptr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(bytes.Length);
            System.Runtime.InteropServices.Marshal.Copy(bytes, 0, ptr, bytes.Length);
            WritePrinter(hPrinter, ptr, bytes.Length, out _);
            System.Runtime.InteropServices.Marshal.FreeCoTaskMem(ptr);
            EndPagePrinter(hPrinter);
            EndDocPrinter(hPrinter);
        }
        finally { ClosePrinter(hPrinter); }
    }
}
