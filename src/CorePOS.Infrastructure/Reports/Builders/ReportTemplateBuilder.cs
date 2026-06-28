using FastReport;
using FastReport.Table;
using System.Drawing;

namespace CorePOS.Infrastructure.Reports.Builders;

/// <summary>
/// Builds FastReport templates in code (no external .frx files needed).
/// Called by ReportService when template file is not found on disk.
/// Each method returns a fully configured Report ready for data binding.
/// 
/// Design: A4 portrait, Arabic RTL, company branding header, data table, footer.
/// </summary>
public static class ReportTemplateBuilder
{
    // ── Shared constants ──────────────────────────────────────────
    private const float PageWidth    = 793f;   // A4 width in points (96 dpi)
    private const float PageHeight   = 1122f;
    private const float Margin       = 40f;
    private const float ContentWidth = PageWidth - Margin * 2;

    private static readonly Font TitleFont    = new("Arial", 16f, FontStyle.Bold);
    private static readonly Font HeaderFont   = new("Arial", 10f, FontStyle.Bold);
    private static readonly Font NormalFont   = new("Arial",  9f);
    private static readonly Font SmallFont    = new("Arial",  8f);
    private static readonly Color HeaderBg    = Color.FromArgb(67, 106, 215);  // blue
    private static readonly Color AltRowBg    = Color.FromArgb(245, 247, 252);
    private static readonly Color BorderColor = Color.FromArgb(200, 210, 230);

    // ════════════════════════════════════════════════════════════════
    // SALES INVOICE (Receipt — A4 / A5)
    // ════════════════════════════════════════════════════════════════
    public static Report BuildInvoiceA4Template(
        string companyName, string companyPhone, string companyAddr,
        string headerLine, string footerLine)
    {
        var report = new Report();
        var page   = new ReportPage
        {
            PaperWidth  = 210,  // A4 mm
            PaperHeight = 297,
            LeftMargin  = 10,
            RightMargin = 10,
            TopMargin   = 10,
            BottomMargin= 10
        };
        report.Pages.Add(page);

        // ── Page Header Band ──────────────────────────────────────
        var header = new PageHeaderBand { Height = Units.Centimeters * 3.5f };
        page.PageHeader = header;

        // Company name
        AddText(header, companyName, 0, 0, ContentWidth, 30,
            font: TitleFont, align: HorzAlign.Center, bold: true);

        // Company info line
        AddText(header, $"{companyAddr}  |  ☎ {companyPhone}", 0, 32, ContentWidth, 18,
            font: SmallFont, align: HorzAlign.Center);

        // Header separator line
        AddText(header, new string('━', 90), 0, 52, ContentWidth, 14,
            font: SmallFont, align: HorzAlign.Center);

        // Invoice title + number + date row
        AddText(header, "فـاتـورة مـبـيـعـات", 0, 66, ContentWidth / 2, 22,
            font: HeaderFont, align: HorzAlign.Right, bold: true);
        AddText(header, "رقم: [Invoice.InvoiceNo]", ContentWidth / 2, 66, ContentWidth / 2, 22,
            font: HeaderFont, align: HorzAlign.Left);

        AddText(header, "التاريخ: [Invoice.InvoiceDate]",   0, 88, ContentWidth / 2, 18,
            font: NormalFont, align: HorzAlign.Right);
        AddText(header, "الكاشير: [Invoice.CashierName]", ContentWidth / 2, 88, ContentWidth / 2, 18,
            font: NormalFont, align: HorzAlign.Left);

        AddText(header, "العميل: [Invoice.CustomerName]", 0, 106, ContentWidth / 2, 18,
            font: NormalFont, align: HorzAlign.Right);
        AddText(header, "طريقة الدفع: [Invoice.PaymentMethodAr]", ContentWidth / 2, 106, ContentWidth / 2, 18,
            font: NormalFont, align: HorzAlign.Left);

        // ── Column Headers Band ───────────────────────────────────
        var colHeader = new GroupHeaderBand { Height = Units.Centimeters * 0.8f };
        page.Bands.Add(colHeader);

        float[] widths = [30f, 200f, 60f, 70f, 50f, 80f];
        string[] labels = ["#", "اسم الصنف", "الوحدة", "الكمية", "الخصم", "السعر"];
        float x = 0;
        for (int i = 0; i < labels.Length; i++)
        {
            var cell = AddText(colHeader, labels[i], x, 0, widths[i], 20,
                font: HeaderFont, align: HorzAlign.Center);
            cell.FillColor = HeaderBg;
            cell.TextColor = Color.White;
            cell.Border.Lines = BorderLines.All;
            cell.Border.Color = HeaderBg;
            x += widths[i];
        }

        // ── Data Band (items) ─────────────────────────────────────
        var dataBand = new DataBand
        {
            DataSource = report.GetDataSource("Items"),
            Height     = Units.Centimeters * 0.7f,
            EvenRows   = true,
            EvenColor  = AltRowBg
        };
        page.Bands.Add(dataBand);

        x = 0;
        string[] fields = ["[Row#]", "[Items.ProductNameAr]", "[Items.UnitName]",
                           "[Items.Quantity]", "[Items.DiscountAmount:N2]", "[Items.TotalPrice:N2]"];
        for (int i = 0; i < fields.Length; i++)
        {
            var cell = AddText(dataBand, fields[i], x, 0, widths[i], 18,
                font: NormalFont, align: i == 1 ? HorzAlign.Right : HorzAlign.Center);
            cell.Border.Lines  = BorderLines.Bottom | BorderLines.Left | BorderLines.Right;
            cell.Border.Color  = BorderColor;
            x += widths[i];
        }

        // ── Footer Band (totals) ──────────────────────────────────
        var footerBand = new ReportSummaryBand { Height = Units.Centimeters * 4f };
        page.ReportSummary = footerBand;

        AddSeparatorLine(footerBand, 0);

        float fy = 8f;
        AddTotalRow(footerBand, "المجموع الفرعي:",  "[Invoice.Subtotal:N2]",       ref fy);
        AddTotalRow(footerBand, "الخصم:",           "[Invoice.DiscountAmount:N2]",  ref fy, Color.Red);
        AddTotalRow(footerBand, "الضريبة:",         "[Invoice.TaxAmount:N2]",       ref fy, Color.DarkOrange);
        AddTotalRow(footerBand, "تكلفة التوصيل:",  "[Invoice.DeliveryCost:N2]",    ref fy);

        // Total box
        var totalBox = AddText(footerBand, "الإجمـالـي النهائي:", 0, fy + 4, ContentWidth / 2, 26,
            font: new Font("Arial", 12f, FontStyle.Bold), align: HorzAlign.Right);
        totalBox.FillColor = Color.FromArgb(220, 252, 231);

        var totalVal = AddText(footerBand, "[Invoice.TotalAmount:N2]", ContentWidth / 2, fy + 4, ContentWidth / 2, 26,
            font: new Font("Arial", 12f, FontStyle.Bold), align: HorzAlign.Left);
        totalVal.FillColor = Color.FromArgb(220, 252, 231);
        totalVal.TextColor = Color.FromArgb(21, 128, 61);
        fy += 30f;

        AddTotalRow(footerBand, "المدفوع:",   "[Invoice.PaidAmount:N2]",      ref fy, Color.DarkGreen);
        AddTotalRow(footerBand, "المتبقي:",   "[Invoice.RemainingAmount:N2]", ref fy, Color.Red);

        // Footer text
        AddText(footerBand, footerLine, 0, fy + 6, ContentWidth, 18,
            font: SmallFont, align: HorzAlign.Center);

        return report;
    }

    // ════════════════════════════════════════════════════════════════
    // THERMAL RECEIPT (58mm / 80mm) — plain text template
    // ════════════════════════════════════════════════════════════════
    public static Report BuildThermalReceiptTemplate(
        int widthMm, string companyName, string footerText)
    {
        var report = new Report();
        float paperW = widthMm == 58 ? 58f : 80f;
        int   charW  = widthMm == 58 ? 32  : 48;

        var page = new ReportPage
        {
            PaperWidth   = paperW,
            PaperHeight  = 200,   // expandable
            LeftMargin   = 2,
            RightMargin  = 2,
            TopMargin    = 2,
            BottomMargin = 4
        };
        report.Pages.Add(page);

        float usable = (paperW - 4) * 2.835f; // mm to points approx

        var hdr = new PageHeaderBand { Height = Units.Centimeters * 2.4f };
        page.PageHeader = hdr;
        AddText(hdr, companyName, 0, 0, usable, 16, font: new Font("Arial", 8f, FontStyle.Bold), align: HorzAlign.Center);
        AddText(hdr, "───────────────────────", 0, 18, usable, 10, font: SmallFont, align: HorzAlign.Center);
        AddText(hdr, "رقم: [Invoice.InvoiceNo]", 0, 30, usable, 10, font: SmallFont, align: HorzAlign.Right);
        AddText(hdr, "[Invoice.InvoiceDate]",    0, 42, usable, 10, font: SmallFont, align: HorzAlign.Right);
        AddText(hdr, "[Invoice.CustomerName]",   0, 54, usable, 10, font: SmallFont, align: HorzAlign.Right);

        var colH = new GroupHeaderBand { Height = Units.Centimeters * 0.5f };
        page.Bands.Add(colH);
        float tw = usable; float cw = tw * 0.4f;
        AddText(colH, "الصنف",   0,    0, tw - cw * 2, 10, font: SmallFont, align: HorzAlign.Right);
        AddText(colH, "ك",       tw - cw * 2, 0, cw, 10, font: SmallFont, align: HorzAlign.Center);
        AddText(colH, "السعر",   tw - cw, 0, cw, 10, font: SmallFont, align: HorzAlign.Left);

        var data = new DataBand { DataSource = report.GetDataSource("Items"), Height = Units.Centimeters * 0.45f };
        page.Bands.Add(data);
        AddText(data, "[Items.ProductNameAr]", 0, 0, tw - cw * 2, 9, font: SmallFont, align: HorzAlign.Right);
        AddText(data, "[Items.Quantity]",      tw - cw * 2, 0, cw, 9, font: SmallFont, align: HorzAlign.Center);
        AddText(data, "[Items.TotalPrice:N2]", tw - cw, 0, cw, 9, font: SmallFont, align: HorzAlign.Left);

        var sum = new ReportSummaryBand { Height = Units.Centimeters * 2.5f };
        page.ReportSummary = sum;
        AddText(sum, "───────────────────────", 0, 0, usable, 10, font: SmallFont, align: HorzAlign.Center);
        float sy = 12f;
        float labelW = usable * 0.55f; float valW = usable * 0.45f;
        void SumRow(string lbl, string val, ref float yy) {
            AddText(sum, lbl, 0, yy, labelW, 10, font: SmallFont, align: HorzAlign.Right);
            AddText(sum, val, labelW, yy, valW, 10, font: SmallFont, align: HorzAlign.Left);
            yy += 11f;
        }
        SumRow("الإجمالي:", "[Invoice.TotalAmount:N2]", ref sy);
        SumRow("المدفوع:", "[Invoice.PaidAmount:N2]", ref sy);
        SumRow("المتبقي:", "[Invoice.RemainingAmount:N2]", ref sy);
        sy += 4f;
        AddText(sum, footerText, 0, sy, usable, 10, font: SmallFont, align: HorzAlign.Center);

        return report;
    }

    // ════════════════════════════════════════════════════════════════
    // SALES REPORT (date range, tabular)
    // ════════════════════════════════════════════════════════════════
    public static Report BuildSalesReportTemplate(string companyName)
    {
        var report = new Report();
        var page   = BuildA4Page();
        report.Pages.Add(page);

        // Title
        var titleBand = new PageHeaderBand { Height = Units.Centimeters * 2.2f };
        page.PageHeader = titleBand;
        BuildReportHeader(titleBand, companyName, "تقرير المبيعات",
            "الفترة من: [DateFrom]  إلى  [DateTo]");

        // Column headers
        var colH = new GroupHeaderBand { Height = Units.Centimeters * 0.8f };
        page.Bands.Add(colH);
        BuildColumnHeader(colH, new[]
        {
            ("#", 25f),       ("رقم الفاتورة", 90f), ("التاريخ", 85f),
            ("العميل", 120f), ("الأصناف", 50f),       ("الإجمالي", 80f),
            ("المدفوع", 75f), ("طريقة الدفع", 80f)
        });

        // Data band
        var data = new DataBand
        {
            DataSource = report.GetDataSource("Invoices"),
            Height     = Units.Centimeters * 0.65f,
            EvenRows   = true, EvenColor = AltRowBg
        };
        page.Bands.Add(data);
        BuildDataRow(data, new[]
        {
            ("[Row#]", 25f, HorzAlign.Center),
            ("[Invoices.InvoiceNo]", 90f, HorzAlign.Center),
            ("[Invoices.InvoiceDate]", 85f, HorzAlign.Center),
            ("[Invoices.CustomerName]", 120f, HorzAlign.Right),
            ("[Invoices.ItemsCount]", 50f, HorzAlign.Center),
            ("[Invoices.TotalAmount:N2]", 80f, HorzAlign.Center),
            ("[Invoices.PaidAmount:N2]", 75f, HorzAlign.Center),
            ("[Invoices.PaymentMethodAr]", 80f, HorzAlign.Center)
        });

        // Summary
        var sumBand = new ReportSummaryBand { Height = Units.Centimeters * 1.8f };
        page.ReportSummary = sumBand;
        BuildSummaryRow(sumBand, new[]
        {
            ("إجمالي الفواتير:", "[TotalCount]", Color.Black),
            ("إجمالي المبيعات:", "[TotalSales]", Color.FromArgb(21, 128, 61)),
            ("إجمالي المرتجعات:", "[TotalReturns]", Color.Red)
        });

        return report;
    }

    // ════════════════════════════════════════════════════════════════
    // PROFIT REPORT
    // ════════════════════════════════════════════════════════════════
    public static Report BuildProfitReportTemplate(string companyName)
    {
        var report = new Report();
        var page   = BuildA4Page();
        report.Pages.Add(page);

        var titleBand = new PageHeaderBand { Height = Units.Centimeters * 2.2f };
        page.PageHeader = titleBand;
        BuildReportHeader(titleBand, companyName, "تقرير الأرباح والخسائر",
            "الفترة من: [DateFrom]  إلى  [DateTo]");

        var colH = new GroupHeaderBand { Height = Units.Centimeters * 0.8f };
        page.Bands.Add(colH);
        BuildColumnHeader(colH, new[]
        {
            ("#", 25f),           ("الصنف", 160f),
            ("الكمية المباعة", 80f), ("إجمالي المبيعات", 90f),
            ("التكلفة", 85f),     ("الربح الإجمالي", 90f), ("هامش%", 60f)
        });

        var data = new DataBand
        {
            DataSource = report.GetDataSource("ProfitData"),
            Height     = Units.Centimeters * 0.65f,
            EvenRows   = true, EvenColor = AltRowBg
        };
        page.Bands.Add(data);
        BuildDataRow(data, new[]
        {
            ("[Row#]", 25f, HorzAlign.Center),
            ("[ProfitData.ProductName]", 160f, HorzAlign.Right),
            ("[ProfitData.QtySold:N2]", 80f, HorzAlign.Center),
            ("[ProfitData.Sales:N2]", 90f, HorzAlign.Center),
            ("[ProfitData.Cost:N2]", 85f, HorzAlign.Center),
            ("[ProfitData.Profit:N2]", 90f, HorzAlign.Center),
            ("[ProfitData.Margin:N1]%", 60f, HorzAlign.Center)
        });

        var sumBand = new ReportSummaryBand { Height = Units.Centimeters * 1.8f };
        page.ReportSummary = sumBand;
        BuildSummaryRow(sumBand, new[]
        {
            ("إجمالي المبيعات:", "[TotalSales:N2]", Color.Black),
            ("إجمالي التكلفة:", "[TotalCost:N2]",  Color.DarkRed),
            ("صافي الربح:",      "[TotalProfit:N2]", Color.DarkGreen)
        });

        return report;
    }

    // ════════════════════════════════════════════════════════════════
    // INVENTORY REPORT
    // ════════════════════════════════════════════════════════════════
    public static Report BuildInventoryReportTemplate(string companyName)
    {
        var report = new Report();
        var page   = BuildA4Page();
        report.Pages.Add(page);

        var titleBand = new PageHeaderBand { Height = Units.Centimeters * 2.2f };
        page.PageHeader = titleBand;
        BuildReportHeader(titleBand, companyName, "تقرير المخزون الحالي",
            "تاريخ التقرير: [ReportDate]");

        var colH = new GroupHeaderBand { Height = Units.Centimeters * 0.8f };
        page.Bands.Add(colH);
        BuildColumnHeader(colH, new[]
        {
            ("#", 25f),         ("باركود", 90f),
            ("الصنف", 150f),   ("القسم", 90f),
            ("الوحدة", 60f),   ("الكمية", 65f),
            ("الحد الأدنى", 70f), ("متوسط التكلفة", 80f), ("القيمة", 80f)
        });

        var data = new DataBand
        {
            DataSource = report.GetDataSource("StockData"),
            Height     = Units.Centimeters * 0.65f,
            EvenRows   = true, EvenColor = AltRowBg
        };
        page.Bands.Add(data);
        BuildDataRow(data, new[]
        {
            ("[Row#]", 25f, HorzAlign.Center),
            ("[StockData.Barcode]", 90f, HorzAlign.Center),
            ("[StockData.NameAr]", 150f, HorzAlign.Right),
            ("[StockData.CategoryName]", 90f, HorzAlign.Center),
            ("[StockData.UnitName]", 60f, HorzAlign.Center),
            ("[StockData.Quantity:N2]", 65f, HorzAlign.Center),
            ("[StockData.MinStock]", 70f, HorzAlign.Center),
            ("[StockData.AverageCost:N2]", 80f, HorzAlign.Center),
            ("[StockData.StockValue:N2]", 80f, HorzAlign.Center)
        });

        var sumBand = new ReportSummaryBand { Height = Units.Centimeters * 1.4f };
        page.ReportSummary = sumBand;
        BuildSummaryRow(sumBand, new[]
        {
            ("عدد الأصناف:", "[ItemCount]", Color.Black),
            ("إجمالي قيمة المخزون:", "[TotalValue:N2]", Color.DarkBlue)
        });

        return report;
    }

    // ════════════════════════════════════════════════════════════════
    // CUSTOMER ACCOUNT STATEMENT
    // ════════════════════════════════════════════════════════════════
    public static Report BuildCustomerAccountTemplate(string companyName)
    {
        var report = new Report();
        var page   = BuildA4Page();
        report.Pages.Add(page);

        var titleBand = new PageHeaderBand { Height = Units.Centimeters * 3f };
        page.PageHeader = titleBand;
        BuildReportHeader(titleBand, companyName, "كشف حساب عميل",
            "من: [DateFrom]  إلى  [DateTo]");

        // Customer info box
        AddText(titleBand, "العميل: [CustomerName]   |   الرصيد الحالي: [CurrentBalance]   |   رقم الهاتف: [CustomerPhone]",
            0, 50, ContentWidth, 16, font: NormalFont, align: HorzAlign.Center);

        var colH = new GroupHeaderBand { Height = Units.Centimeters * 0.8f };
        page.Bands.Add(colH);
        BuildColumnHeader(colH, new[]
        {
            ("التاريخ", 85f), ("النوع", 80f), ("المرجع", 90f),
            ("مدين", 90f),   ("دائن", 90f),  ("الرصيد", 100f), ("ملاحظات", 100f)
        });

        var data = new DataBand
        {
            DataSource = report.GetDataSource("Transactions"),
            Height     = Units.Centimeters * 0.65f,
            EvenRows   = true, EvenColor = AltRowBg
        };
        page.Bands.Add(data);
        BuildDataRow(data, new[]
        {
            ("[Transactions.Date]", 85f, HorzAlign.Center),
            ("[Transactions.TypeAr]", 80f, HorzAlign.Center),
            ("[Transactions.Reference]", 90f, HorzAlign.Center),
            ("[Transactions.Debit:N2]", 90f, HorzAlign.Center),
            ("[Transactions.Credit:N2]", 90f, HorzAlign.Center),
            ("[Transactions.Balance:N2]", 100f, HorzAlign.Center),
            ("[Transactions.Notes]", 100f, HorzAlign.Right)
        });

        var sumBand = new ReportSummaryBand { Height = Units.Centimeters * 1.4f };
        page.ReportSummary = sumBand;
        BuildSummaryRow(sumBand, new[]
        {
            ("إجمالي المدين:", "[TotalDebit:N2]", Color.DarkRed),
            ("إجمالي الدائن:", "[TotalCredit:N2]", Color.DarkGreen),
            ("الرصيد النهائي:", "[FinalBalance:N2]", Color.DarkBlue)
        });

        return report;
    }

    // ════════════════════════════════════════════════════════════════
    // SHIFT CLOSING REPORT
    // ════════════════════════════════════════════════════════════════
    public static Report BuildShiftReportTemplate(string companyName)
    {
        var report = new Report();
        var page   = BuildA4Page();
        report.Pages.Add(page);

        var titleBand = new PageHeaderBand { Height = Units.Centimeters * 2.2f };
        page.PageHeader = titleBand;
        BuildReportHeader(titleBand, companyName, "تقرير إقفال الوردية",
            "الوردية رقم: [ShiftNo]   |   الكاشير: [CashierName]   |   التاريخ: [ShiftDate]");

        var colH = new GroupHeaderBand { Height = Units.Centimeters * 0.8f };
        page.Bands.Add(colH);
        BuildColumnHeader(colH, new[]
        {
            ("رقم الفاتورة", 100f), ("الوقت", 80f), ("العميل", 120f),
            ("طريقة الدفع", 90f),  ("الإجمالي", 85f), ("الحالة", 70f)
        });

        var data = new DataBand
        {
            DataSource = report.GetDataSource("Invoices"),
            Height     = Units.Centimeters * 0.65f,
            EvenRows   = true, EvenColor = AltRowBg
        };
        page.Bands.Add(data);
        BuildDataRow(data, new[]
        {
            ("[Invoices.InvoiceNo]", 100f, HorzAlign.Center),
            ("[Invoices.InvoiceTime]", 80f, HorzAlign.Center),
            ("[Invoices.CustomerName]", 120f, HorzAlign.Right),
            ("[Invoices.PaymentMethodAr]", 90f, HorzAlign.Center),
            ("[Invoices.TotalAmount:N2]", 85f, HorzAlign.Center),
            ("[Invoices.StatusAr]", 70f, HorzAlign.Center)
        });

        var sumBand = new ReportSummaryBand { Height = Units.Centimeters * 3f };
        page.ReportSummary = sumBand;
        float sy = 8f;
        AddText(sumBand, "ملخص الوردية", 0, sy, ContentWidth, 20,
            font: HeaderFont, align: HorzAlign.Center); sy += 24f;

        void ShiftRow(string lbl, string val, ref float y, Color? vc = null)
        {
            AddText(sumBand, lbl, ContentWidth * 0.45f, y, ContentWidth * 0.3f, 18,
                font: NormalFont, align: HorzAlign.Right);
            var v = AddText(sumBand, val, ContentWidth * 0.75f, y, ContentWidth * 0.25f, 18,
                font: new Font("Arial", 9f, FontStyle.Bold), align: HorzAlign.Left);
            if (vc.HasValue) v.TextColor = vc.Value;
            y += 20f;
        }

        ShiftRow("رصيد البداية:", "[OpenBalance:N2]", ref sy);
        ShiftRow("إجمالي المبيعات:", "[TotalSales:N2]", ref sy, Color.DarkGreen);
        ShiftRow("إجمالي المرتجعات:", "[TotalReturns:N2]", ref sy, Color.DarkRed);
        ShiftRow("إجمالي المصروفات:", "[TotalExpenses:N2]", ref sy, Color.DarkOrange);
        ShiftRow("رصيد النهاية:", "[CloseBalance:N2]", ref sy, Color.DarkBlue);
        ShiftRow("عدد الفواتير:", "[InvoiceCount]", ref sy);

        return report;
    }

    // ════════════════════════════════════════════════════════════════
    // PRIVATE HELPERS
    // ════════════════════════════════════════════════════════════════
    private static ReportPage BuildA4Page() => new()
    {
        PaperWidth   = 210,
        PaperHeight  = 297,
        LeftMargin   = 12,
        RightMargin  = 12,
        TopMargin    = 10,
        BottomMargin = 10
    };

    private static void BuildReportHeader(BandBase band, string company, string title, string subTitle)
    {
        AddText(band, company, 0, 0, ContentWidth, 22,
            font: TitleFont, align: HorzAlign.Center, bold: true);
        AddText(band, title, 0, 24, ContentWidth, 20,
            font: new Font("Arial", 13f, FontStyle.Bold), align: HorzAlign.Center);
        AddText(band, subTitle, 0, 46, ContentWidth, 16,
            font: SmallFont, align: HorzAlign.Center);

        // Separator
        var sep = new ShapeObject
        {
            Bounds = new RectangleF(0, 64, ContentWidth, 2),
            Shape  = ShapeKind.Rectangle,
            FillColor = HeaderBg
        };
        band.Objects.Add(sep);
    }

    private static void BuildColumnHeader(BandBase band,
        (string Label, float Width)[] columns)
    {
        float x = 0;
        foreach (var (label, width) in columns)
        {
            var cell = AddText(band, label, x, 0, width, 20,
                font: HeaderFont, align: HorzAlign.Center);
            cell.FillColor = HeaderBg;
            cell.TextColor = Color.White;
            cell.Border.Lines = BorderLines.All;
            cell.Border.Color = HeaderBg;
            x += width;
        }
    }

    private static void BuildDataRow(BandBase band,
        (string Field, float Width, HorzAlign Align)[] columns)
    {
        float x = 0;
        foreach (var (field, width, align) in columns)
        {
            var cell = AddText(band, field, x, 0, width, 16,
                font: NormalFont, align: align);
            cell.Border.Lines = BorderLines.Bottom | BorderLines.Left | BorderLines.Right;
            cell.Border.Color = BorderColor;
            x += width;
        }
    }

    private static void BuildSummaryRow(BandBase band,
        (string Label, string Value, Color ValueColor)[] rows)
    {
        AddSeparatorLine(band, 0);
        float y = 8f;
        foreach (var (label, value, color) in rows)
        {
            AddText(band, label, ContentWidth * 0.5f, y, ContentWidth * 0.25f, 16,
                font: HeaderFont, align: HorzAlign.Right);
            var v = AddText(band, value, ContentWidth * 0.75f, y, ContentWidth * 0.25f, 16,
                font: new Font("Arial", 9f, FontStyle.Bold), align: HorzAlign.Left);
            v.TextColor = color;
            y += 18f;
        }
    }

    private static void AddSeparatorLine(BandBase band, float y)
    {
        var line = new LineObject
        {
            Bounds   = new RectangleF(0, y, ContentWidth, 1),
            Border   = { Color = HeaderBg, Width = 1.5f }
        };
        band.Objects.Add(line);
    }

    private static void AddTotalRow(BandBase band, string label, string value,
        ref float y, Color? valueColor = null)
    {
        float halfW = ContentWidth / 2;
        AddText(band, label, 0, y, halfW, 16, font: NormalFont, align: HorzAlign.Right);
        var v = AddText(band, value, halfW, y, halfW, 16,
            font: new Font("Arial", 9f, FontStyle.Bold), align: HorzAlign.Left);
        if (valueColor.HasValue) v.TextColor = valueColor.Value;
        y += 18f;
    }

    private static TextObject AddText(BandBase band, string text,
        float x, float y, float w, float h,
        Font? font = null, HorzAlign align = HorzAlign.Right,
        bool bold = false)
    {
        var tb = new TextObject
        {
            Text   = text,
            Bounds = new RectangleF(x, y, w, h),
            Font   = font ?? (bold ? HeaderFont : NormalFont),
            HorzAlign     = align,
            VertAlign     = VertAlign.Center,
            WordWrap      = false,
            RightToLeft   = true,
            AutoWidth     = false,
            AutoHeight    = false
        };
        band.Objects.Add(tb);
        return tb;
    }
}
