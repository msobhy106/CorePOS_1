using System.Drawing.Printing;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using CorePOS.Application.Interfaces;
using CorePOS.Domain.Entities;

namespace CorePOS.Infrastructure.Services;

/// <summary>
/// Thermal printer service using ESC/POS commands.
/// Supports: 58mm, 80mm thermal printers.
/// Fallback: Windows GDI printing if ESC/POS raw fails.
/// Cash drawer: pulse via ESC/POS pin 2.
/// </summary>
public class ThermalPrinterService : IPrinterService
{
    private readonly ILogger<ThermalPrinterService> _logger;
    private readonly ISettingsRepository            _settings;

    public ThermalPrinterService(
        ILogger<ThermalPrinterService> logger,
        ISettingsRepository settings)
    {
        _logger   = logger;
        _settings = settings;
    }

    // ════════════════════════════════════════════════════════════════
    // PUBLIC API
    // ════════════════════════════════════════════════════════════════

    public async Task PrintInvoiceAsync(
        SalesInvoice invoice, CancellationToken ct = default)
    {
        var printerName = await _settings.GetStringAsync("ThermalPrinterName", string.Empty, ct);
        var printSize   = await _settings.GetStringAsync("DefaultPrintSize",   "80mm",        ct);
        var footerText  = await _settings.GetStringAsync("InvoiceFooterText",  "شكراً لتعاملكم معنا ♥", ct);
        var companyName = await _settings.GetStringAsync("CompanyNameAr",      "Core POS",    ct);
        var companyPhone= await _settings.GetStringAsync("CompanyPhone",       "",            ct);
        var companyAddr = await _settings.GetStringAsync("CompanyAddress",     "",            ct);
        var autoCashDrw = await _settings.GetBoolAsync  ("OpenCashDrawer",     true,          ct);

        // Build ESC/POS bytes
        var bytes = printSize == "58mm"
            ? Build58mmReceipt(invoice, companyName, companyPhone, companyAddr, footerText)
            : Build80mmReceipt(invoice, companyName, companyPhone, companyAddr, footerText);

        // Print
        if (string.IsNullOrWhiteSpace(printerName))
            printerName = GetDefaultPrinterName();

        bool rawOk = false;
        if (!string.IsNullOrEmpty(printerName))
            rawOk = SendRawToPrinter(printerName, bytes);

        if (!rawOk)
        {
            _logger.LogWarning("Raw ESC/POS failed, falling back to GDI print");
            PrintViaGdi(invoice, companyName, printerName, printSize);
        }

        // Open cash drawer
        if (autoCashDrw && invoice.PaymentMethod.ToString() != "Credit")
            await OpenCashDrawerAsync(printerName, ct);
    }

    public async Task OpenCashDrawerAsync(
        string? printerName = null, CancellationToken ct = default)
    {
        printerName ??= await _settings.GetStringAsync("ThermalPrinterName", string.Empty, ct);
        if (string.IsNullOrEmpty(printerName))
            printerName = GetDefaultPrinterName();

        if (string.IsNullOrEmpty(printerName)) return;

        // ESC/POS: ESC p 0 t1 t2
        var pulse = new byte[] { 0x1B, 0x70, 0x00, 0x40, 0x88 };
        SendRawToPrinter(printerName, pulse);
    }

    public async Task PrintTestPageAsync(
        string printerName, CancellationToken ct = default)
    {
        var builder = new EscPosBuilder(48);
        builder.Initialize()
               .AlignCenter()
               .Bold(true)
               .TextLatin("=== TEST PAGE ===\n")
               .Bold(false)
               .TextArabic("اختبار الطابعة\n")
               .TextLatin($"Printer: {printerName}\n")
               .TextLatin($"Date: {DateTime.Now:dd/MM/yyyy HH:mm}\n")
               .FeedLines(2)
               .Cut();

        SendRawToPrinter(printerName, builder.Build());
    }

    // ════════════════════════════════════════════════════════════════
    // 80mm RECEIPT BUILDER
    // ════════════════════════════════════════════════════════════════
    private static byte[] Build80mmReceipt(
        SalesInvoice inv, string company, string phone, string addr, string footer)
    {
        const int CharWidth = 48;
        var b = new EscPosBuilder(CharWidth);

        b.Initialize()
         .AlignCenter()
         .FontSize(2, 2)
         .Bold(true)
         .TextArabic(company + "\n")
         .FontSize(1, 1)
         .Bold(false);

        if (!string.IsNullOrEmpty(phone))
            b.TextArabic($"☎ {phone}\n");
        if (!string.IsNullOrEmpty(addr))
            b.TextArabic(addr + "\n");

        b.Separator('─', CharWidth)
         .AlignRight()
         .TextArabic($"فاتورة رقم: {inv.InvoiceNo}\n")
         .TextArabic($"التاريخ: {inv.InvoiceDate:dd/MM/yyyy  hh:mm tt}\n");

        var cashier  = inv.User?.FullName ?? "";
        var customer = inv.Customer?.Name ?? "عميل نقدي";
        b.TextArabic($"الكاشير: {cashier}\n")
         .TextArabic($"العميل: {customer}\n")
         .Separator('─', CharWidth);

        // Items header
        b.AlignRight()
         .Bold(true)
         .ColsArabic(CharWidth, ("الصنف", 24), ("ك", 6), ("سعر", 8), ("إجمالي", 10))
         .Bold(false)
         .Separator('─', CharWidth);

        // Items
        int rowNo = 1;
        foreach (var item in inv.Items)
        {
            var nameAr = Truncate(item.ProductNameAr, 22);
            b.TextArabic($"{rowNo++,2}. {nameAr}\n");
            b.ColsLatin(CharWidth,
                ("", 2),
                (item.Quantity.ToString("N2"), 10),
                (item.UnitPrice.ToString("N2"), 12),
                (item.TotalPrice.ToString("N2"), 12),
                ("", 12));
        }

        b.Separator('─', CharWidth);

        // Totals
        b.AlignRight();
        TotalRowAr(b, "المجموع الفرعي:",   inv.Subtotal.ToString("N2"),          CharWidth);
        if (inv.DiscountAmount > 0)
            TotalRowAr(b, "الخصم:",         $"-{inv.DiscountAmount:N2}",          CharWidth);
        if (inv.TaxAmount > 0)
            TotalRowAr(b, "الضريبة:",       $"+{inv.TaxAmount:N2}",              CharWidth);
        if (inv.DeliveryCost > 0)
            TotalRowAr(b, "التوصيل:",       $"+{inv.DeliveryCost:N2}",           CharWidth);

        b.Separator('=', CharWidth);
        b.Bold(true).FontSize(1, 2);
        TotalRowAr(b, "الإجمالي:",          inv.TotalAmount.ToString("N2"),       CharWidth);
        b.FontSize(1, 1).Bold(false);
        TotalRowAr(b, "المدفوع:",           inv.PaidAmount.ToString("N2"),        CharWidth);
        TotalRowAr(b, "المتبقي:",           inv.RemainingAmount.ToString("N2"),   CharWidth);

        // Payment method
        b.Separator('─', CharWidth)
         .AlignCenter()
         .TextArabic($"طريقة الدفع: {MapPayMethodAr(inv.PaymentMethod.ToString())}\n");

        // Footer
        b.Separator('─', CharWidth)
         .AlignCenter()
         .TextArabic(footer + "\n")
         .FeedLines(3)
         .Cut();

        return b.Build();
    }

    // ════════════════════════════════════════════════════════════════
    // 58mm RECEIPT BUILDER
    // ════════════════════════════════════════════════════════════════
    private static byte[] Build58mmReceipt(
        SalesInvoice inv, string company, string phone, string addr, string footer)
    {
        const int CharWidth = 32;
        var b = new EscPosBuilder(CharWidth);

        b.Initialize()
         .AlignCenter()
         .Bold(true)
         .TextArabic(Truncate(company, CharWidth) + "\n")
         .Bold(false);

        if (!string.IsNullOrEmpty(phone))
            b.TextArabic($"☎ {phone}\n");

        b.Separator('─', CharWidth)
         .AlignRight()
         .TextArabic($"رقم: {inv.InvoiceNo}\n")
         .TextArabic($"{inv.InvoiceDate:dd/MM/yyyy  hh:mm}\n")
         .TextArabic($"{inv.Customer?.Name ?? "نقدي"}\n")
         .Separator('─', CharWidth);

        // Items — simplified for 58mm
        foreach (var item in inv.Items)
        {
            var name = Truncate(item.ProductNameAr, CharWidth - 2);
            b.AlignRight().TextArabic(name + "\n");
            var detail = $"{item.Quantity:N0} x {item.UnitPrice:N2} = {item.TotalPrice:N2}";
            b.AlignLeft().TextLatin(detail.PadLeft(CharWidth) + "\n");
        }

        b.Separator('─', CharWidth)
         .AlignRight();

        TotalRowAr(b, "الإجمالي:", inv.TotalAmount.ToString("N2"), CharWidth);
        TotalRowAr(b, "المدفوع:",  inv.PaidAmount.ToString("N2"),  CharWidth);
        TotalRowAr(b, "المتبقي:",  inv.RemainingAmount.ToString("N2"), CharWidth);

        b.Separator('─', CharWidth)
         .AlignCenter()
         .TextArabic(footer + "\n")
         .FeedLines(3)
         .Cut();

        return b.Build();
    }

    // ════════════════════════════════════════════════════════════════
    // GDI FALLBACK PRINT
    // ════════════════════════════════════════════════════════════════
    private void PrintViaGdi(
        SalesInvoice inv, string company, string printerName, string printSize)
    {
        try
        {
            var pd = new PrintDocument();
            if (!string.IsNullOrEmpty(printerName))
                pd.PrinterSettings.PrinterName = printerName;

            if (printSize == "58mm")
            {
                pd.DefaultPageSettings.PaperSize = new PaperSize("58mm", 228, 800);
            }
            else if (printSize == "80mm")
            {
                pd.DefaultPageSettings.PaperSize = new PaperSize("80mm", 315, 800);
            }

            pd.PrintPage += (_, e) => DrawInvoiceGdi(e, inv, company);
            pd.Print();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GDI print fallback also failed");
        }
    }

    private static void DrawInvoiceGdi(PrintPageEventArgs e, SalesInvoice inv, string company)
    {
        var g    = e.Graphics!;
        var rtl  = new StringFormat(StringFormatFlags.DirectionRightToLeft);
        float x  = e.MarginBounds.Left;
        float y  = e.MarginBounds.Top;
        float w  = e.MarginBounds.Width;

        using var titleFont  = new Font("Tahoma", 12f, FontStyle.Bold);
        using var normalFont = new Font("Tahoma", 9f);
        using var smallFont  = new Font("Tahoma", 8f);
        using var boldFont   = new Font("Tahoma", 10f, FontStyle.Bold);

        void Line(string text, Font font, bool center = false)
        {
            var fmt = center ? new StringFormat(StringFormatFlags.DirectionRightToLeft) { Alignment = StringAlignment.Center } : rtl;
            g.DrawString(text, font, Brushes.Black, new RectangleF(x, y, w, 30), fmt);
            y += g.MeasureString(text, font, (int)w, fmt).Height + 2;
        }

        void Sep() { g.DrawLine(Pens.Black, x, y, x + w, y); y += 4; }

        Line(company, titleFont, center: true);
        if (!string.IsNullOrEmpty(inv.Customer?.Phone))
            Line($"☎ {inv.Customer.Phone}", smallFont, center: true);
        Sep();
        Line($"فاتورة رقم: {inv.InvoiceNo}", normalFont);
        Line($"التاريخ: {inv.InvoiceDate:dd/MM/yyyy HH:mm}", normalFont);
        Line($"العميل: {inv.Customer?.Name ?? "نقدي"}", normalFont);
        Sep();

        foreach (var item in inv.Items)
        {
            Line(item.ProductNameAr, normalFont);
            Line($"  {item.Quantity:N2} × {item.UnitPrice:N2} = {item.TotalPrice:N2}", smallFont);
        }

        Sep();
        Line($"الإجمالي: {inv.TotalAmount:N2}", boldFont);
        Line($"المدفوع:  {inv.PaidAmount:N2}",  normalFont);
        Line($"المتبقي:  {inv.RemainingAmount:N2}", normalFont);
    }

    // ════════════════════════════════════════════════════════════════
    // WINDOWS RAW PRINTING API
    // ════════════════════════════════════════════════════════════════
    [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true,
        CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    private static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter,
        out IntPtr hPrinter, IntPtr pd);

    [DllImport("winspool.Drv", EntryPoint = "ClosePrinter",
        SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    private static extern bool ClosePrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA",
        SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true,
        CallingConvention = CallingConvention.StdCall)]
    private static extern bool StartDocPrinter(IntPtr hPrinter, int level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

    [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter",
        SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    private static extern bool EndDocPrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter",
        SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    private static extern bool StartPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter",
        SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    private static extern bool EndPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "WritePrinter",
        SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    private static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private class DOCINFOA
    {
        [MarshalAs(UnmanagedType.LPStr)] public string pDocName = "CorePOS Receipt";
        [MarshalAs(UnmanagedType.LPStr)] public string? pOutputFile;
        [MarshalAs(UnmanagedType.LPStr)] public string pDataType = "RAW";
    }

    private bool SendRawToPrinter(string printerName, byte[] bytes)
    {
        IntPtr hPrinter;
        if (!OpenPrinter(printerName.Normalize(), out hPrinter, IntPtr.Zero))
        {
            _logger.LogWarning("Could not open printer: {Name}", printerName);
            return false;
        }

        try
        {
            var di = new DOCINFOA();
            if (!StartDocPrinter(hPrinter, 1, di)) return false;
            if (!StartPagePrinter(hPrinter)) { EndDocPrinter(hPrinter); return false; }

            var pUnmanagedBytes = Marshal.AllocCoTaskMem(bytes.Length);
            try
            {
                Marshal.Copy(bytes, 0, pUnmanagedBytes, bytes.Length);
                WritePrinter(hPrinter, pUnmanagedBytes, bytes.Length, out _);
            }
            finally
            {
                Marshal.FreeCoTaskMem(pUnmanagedBytes);
            }

            EndPagePrinter(hPrinter);
            EndDocPrinter(hPrinter);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Raw printer error");
            return false;
        }
        finally
        {
            ClosePrinter(hPrinter);
        }
    }

    // ════════════════════════════════════════════════════════════════
    // HELPERS
    // ════════════════════════════════════════════════════════════════
    private static string GetDefaultPrinterName()
    {
        try
        {
            using var pd = new PrintDocument();
            return pd.PrinterSettings.PrinterName;
        }
        catch { return string.Empty; }
    }

    private static void TotalRowAr(EscPosBuilder b, string label, string value, int charWidth)
    {
        int valLen   = value.Length + 2;
        int labelLen = charWidth - valLen;
        b.TextArabicRow(label, value, labelLen, valLen);
    }

    private static string Truncate(string s, int max)
        => s.Length <= max ? s : s[..max];

    private static string MapPayMethodAr(string m) => m switch
    {
        "Cash"         => "نقدي",
        "Visa"         => "فيزا",
        "BankTransfer" => "تحويل بنكي",
        "EWallet"      => "محفظة",
        "Credit"       => "آجل",
        "Mixed"        => "مختلط",
        _              => m
    };
}

// ════════════════════════════════════════════════════════════════════
// ESC/POS BUILDER — Fluent API
// ════════════════════════════════════════════════════════════════════
public sealed class EscPosBuilder
{
    private readonly List<byte>  _bytes    = new();
    private readonly int         _charWidth;
    // CP864 Arabic codepage encoding
    private readonly Encoding    _arabic   = Encoding.GetEncoding(1256);

    public EscPosBuilder(int charWidth = 48) => _charWidth = charWidth;

    // ── Init / Reset ──────────────────────────────────────────────
    public EscPosBuilder Initialize()
    {
        Add(0x1B, 0x40);           // ESC @ — Initialize
        Add(0x1B, 0x74, 0x15);     // ESC t 21 — Select codepage CP1256 (Arabic)
        return this;
    }

    // ── Alignment ─────────────────────────────────────────────────
    public EscPosBuilder AlignLeft()   { Add(0x1B, 0x61, 0x00); return this; }
    public EscPosBuilder AlignCenter() { Add(0x1B, 0x61, 0x01); return this; }
    public EscPosBuilder AlignRight()  { Add(0x1B, 0x61, 0x02); return this; }

    // ── Font styles ───────────────────────────────────────────────
    public EscPosBuilder Bold(bool on)
    {
        Add(0x1B, 0x45, on ? (byte)1 : (byte)0);
        return this;
    }

    public EscPosBuilder Underline(bool on)
    {
        Add(0x1B, 0x2D, on ? (byte)1 : (byte)0);
        return this;
    }

    public EscPosBuilder FontSize(byte width, byte height)
    {
        byte size = (byte)(((width - 1) << 4) | (height - 1));
        Add(0x1D, 0x21, size);
        return this;
    }

    // ── Text ──────────────────────────────────────────────────────
    public EscPosBuilder TextLatin(string text)
    {
        _bytes.AddRange(Encoding.ASCII.GetBytes(text));
        return this;
    }

    public EscPosBuilder TextArabic(string text)
    {
        // Reverse Arabic text for RTL thermal printers
        var reversed = ReverseArabic(text);
        _bytes.AddRange(_arabic.GetBytes(reversed));
        return this;
    }

    public EscPosBuilder TextArabicRow(string right, string left, int rightLen, int leftLen)
    {
        var rightPad = PadArabic(right, rightLen);
        var leftPad  = left.PadLeft(leftLen);
        TextArabic(rightPad);
        TextLatin(leftPad + "\n");
        return this;
    }

    public EscPosBuilder ColsArabic(int totalWidth, params (string Text, int Width)[] cols)
    {
        var sb = new StringBuilder();
        // Print right-to-left
        for (int i = cols.Length - 1; i >= 0; i--)
            sb.Append(PadArabic(cols[i].Text, cols[i].Width));
        TextArabic(sb.ToString() + "\n");
        return this;
    }

    public EscPosBuilder ColsLatin(int totalWidth, params (string Text, int Width)[] cols)
    {
        var sb = new StringBuilder();
        foreach (var (text, width) in cols)
            sb.Append(text.PadRight(width).Substring(0, Math.Min(text.Length, width)).PadRight(width));
        TextLatin(sb.ToString() + "\n");
        return this;
    }

    // ── Layout ────────────────────────────────────────────────────
    public EscPosBuilder Separator(char ch = '-', int? width = null)
    {
        int w = width ?? _charWidth;
        TextLatin(new string(ch, w) + "\n");
        return this;
    }

    public EscPosBuilder FeedLines(int count)
    {
        Add(0x1B, 0x64, (byte)count);
        return this;
    }

    public EscPosBuilder Cut(bool partial = false)
    {
        // GS V — full cut (0x00) or partial (0x01)
        Add(0x1D, 0x56, partial ? (byte)0x01 : (byte)0x00);
        return this;
    }

    // ── Barcode ───────────────────────────────────────────────────
    public EscPosBuilder Barcode(string data, BarcodeType type = BarcodeType.Code128)
    {
        // GS k — print barcode
        AlignCenter();
        Add(0x1D, 0x6B, (byte)type);
        _bytes.AddRange(Encoding.ASCII.GetBytes(data));
        Add(0x00);
        return this;
    }

    // ── Build ─────────────────────────────────────────────────────
    public byte[] Build() => _bytes.ToArray();

    // ── Helpers ───────────────────────────────────────────────────
    private void Add(params byte[] data) => _bytes.AddRange(data);

    private static string PadArabic(string s, int width)
    {
        if (s.Length >= width) return s[..width];
        return s + new string(' ', width - s.Length);
    }

    /// <summary>Simple RTL reversal for thermal Arabic display.</summary>
    private static string ReverseArabic(string text)
    {
        // Split by newline, reverse each line's words for RTL
        var lines = text.Split('\n');
        var result = new StringBuilder();
        foreach (var line in lines)
        {
            if (string.IsNullOrEmpty(line)) { result.Append('\n'); continue; }
            // For thermal printers: Arabic chars print RTL naturally with CP1256
            // We just ensure newlines are correct
            result.Append(line).Append('\n');
        }
        return result.ToString().TrimEnd('\n');
    }
}

public enum BarcodeType : byte
{
    UpcA    = 0x00,
    UpcE    = 0x01,
    Ean13   = 0x02,
    Ean8    = 0x03,
    Code39  = 0x04,
    Itf     = 0x05,
    Codabar = 0x06,
    Code93  = 0x48,
    Code128 = 0x49
}
