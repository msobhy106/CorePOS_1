using FastReport;
using FastReport.Export.PdfSimple;
using FastReport.Export.OoXML;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using CorePOS.Application.Interfaces;
using CorePOS.Domain.Interfaces;
using CorePOS.Infrastructure.Reports.Builders;

namespace CorePOS.Infrastructure.Services;

/// <summary>
/// Complete ReportService — Phase 10.
/// Uses ReportTemplateBuilder to generate .frx templates in code,
/// then registers data, prepares, and exports to PDF / Excel / bytes.
/// Falls back to .frx files on disk if present (allows designer customization).
/// </summary>
public class ReportService : IReportService
{
    private readonly IUnitOfWork            _uow;
    private readonly ISettingsRepository    _settings;
    private readonly ILogger<ReportService> _logger;
    private readonly string                 _templatesPath;

    public ReportService(
        IUnitOfWork uow,
        ISettingsRepository settings,
        ILogger<ReportService> logger,
        IConfiguration config)
    {
        _uow           = uow;
        _settings      = settings;
        _logger        = logger;
        _templatesPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "Reports", "Templates");
        Directory.CreateDirectory(_templatesPath);
    }

    // ── Shared settings helpers ───────────────────────────────────
    private async Task<(string company, string phone, string addr, string header, string footer)>
        GetCompanySettingsAsync(CancellationToken ct)
    {
        var company = await _settings.GetStringAsync("CompanyNameAr",    "Core POS",           ct);
        var phone   = await _settings.GetStringAsync("CompanyPhone",     "",                   ct);
        var addr    = await _settings.GetStringAsync("CompanyAddress",   "",                   ct);
        var header  = await _settings.GetStringAsync("InvoiceHeaderText","",                   ct);
        var footer  = await _settings.GetStringAsync("InvoiceFooterText","شكراً لزيارتكم ♥",  ct);
        return (company, phone, addr, header, footer);
    }

    // ════════════════════════════════════════════════════════════════
    // SALES INVOICE PRINT
    // ════════════════════════════════════════════════════════════════
    public async Task<byte[]> GenerateInvoicePrintAsync(
        int invoiceId, string format = "pdf", CancellationToken ct = default)
    {
        try
        {
            var invoice = await _uow.Sales.GetByIdWithItemsAsync(invoiceId, ct)
                ?? throw new InvalidOperationException($"Invoice {invoiceId} not found.");

            var (company, phone, addr, header, footer) = await GetCompanySettingsAsync(ct);
            var printSize = await _settings.GetStringAsync("DefaultPrintSize", "A4", ct);

            using var report = LoadOrBuild($"SalesInvoice_{printSize}.frx", () =>
                printSize is "58mm" or "80mm"
                    ? ReportTemplateBuilder.BuildThermalReceiptTemplate(
                        printSize == "58mm" ? 58 : 80, company, footer)
                    : ReportTemplateBuilder.BuildInvoiceA4Template(company, phone, addr, header, footer));

            // Map domain entity → anonymous DTO (for parameter binding)
            var invoiceDto = new
            {
                invoice.InvoiceNo,
                InvoiceDate      = invoice.InvoiceDate.ToString("dd/MM/yyyy  hh:mm tt"),
                CustomerName     = invoice.Customer?.Name ?? "عميل نقدي",
                CustomerPhone    = invoice.Customer?.Phone ?? "",
                CashierName      = invoice.User?.FullName ?? "",
                PaymentMethodAr  = MapPaymentMethodAr(invoice.PaymentMethod.ToString()),
                invoice.Subtotal,
                invoice.DiscountAmount,
                invoice.TaxAmount,
                invoice.DeliveryCost,
                invoice.TotalAmount,
                invoice.PaidAmount,
                invoice.RemainingAmount,
                Notes            = invoice.Notes ?? ""
            };

            var itemDtos = invoice.Items.Select(i => new
            {
                i.ProductNameAr,
                Barcode    = i.Barcode ?? "",
                UnitName   = i.Unit?.Name ?? "",
                i.Quantity,
                i.UnitPrice,
                i.DiscountAmount,
                i.TaxAmount,
                i.TotalPrice
            }).ToList();

            report.RegisterData(new[] { invoiceDto }, "Invoice");
            report.RegisterData(itemDtos,             "Items");
            SetParams(report,
                ("InvoiceNo",   invoice.InvoiceNo),
                ("InvoiceDate", invoiceDto.InvoiceDate),
                ("CustomerName",invoiceDto.CustomerName),
                ("Total",       invoice.TotalAmount.ToString("N2")),
                ("Paid",        invoice.PaidAmount.ToString("N2")),
                ("Remaining",   invoice.RemainingAmount.ToString("N2")));

            report.Prepare();
            return Export(report, format);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Invoice print failed for InvoiceId={Id}", invoiceId);
            throw;
        }
    }

    // ════════════════════════════════════════════════════════════════
    // SALES REPORT
    // ════════════════════════════════════════════════════════════════
    public async Task<byte[]> GenerateSalesReportAsync(
        DateTime from, DateTime to, int? branchId,
        string format = "pdf", CancellationToken ct = default)
    {
        try
        {
            var invoices = await _uow.Sales.GetByDateRangeAsync(from, to, branchId, ct);
            var (company, _, _, _, _) = await GetCompanySettingsAsync(ct);

            using var report = LoadOrBuild("SalesReport.frx",
                () => ReportTemplateBuilder.BuildSalesReportTemplate(company));

            var rows = invoices.Select(inv => new
            {
                inv.InvoiceNo,
                InvoiceDate     = inv.InvoiceDate.ToString("dd/MM/yyyy"),
                CustomerName    = inv.Customer?.Name ?? "نقدي",
                ItemsCount      = inv.Items.Count,
                inv.TotalAmount,
                inv.PaidAmount,
                PaymentMethodAr = MapPaymentMethodAr(inv.PaymentMethod.ToString()),
                StatusAr        = MapStatusAr(inv.Status.ToString())
            }).ToList();

            report.RegisterData(rows, "Invoices");
            SetParams(report,
                ("DateFrom",     from.ToString("dd/MM/yyyy")),
                ("DateTo",       to.ToString("dd/MM/yyyy")),
                ("TotalCount",   rows.Count.ToString()),
                ("TotalSales",   rows.Sum(r => r.TotalAmount).ToString("N2")),
                ("TotalReturns", "0.00"));

            report.Prepare();
            return Export(report, format);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sales report failed: {From}-{To}", from, to);
            throw;
        }
    }

    // ════════════════════════════════════════════════════════════════
    // PROFIT REPORT
    // ════════════════════════════════════════════════════════════════
    public async Task<byte[]> GenerateProfitReportAsync(
        DateTime from, DateTime to, int? branchId,
        string format = "pdf", CancellationToken ct = default)
    {
        try
        {
            var invoices = await _uow.Sales.GetByDateRangeAsync(from, to, branchId, ct);
            var (company, _, _, _, _) = await GetCompanySettingsAsync(ct);

            // Aggregate by product
            var profitData = invoices
                .SelectMany(inv => inv.Items.Select(item => new { inv, item }))
                .GroupBy(x => x.item.ProductId)
                .Select(g => new
                {
                    ProductName = g.First().item.ProductNameAr,
                    QtySold     = g.Sum(x => x.item.Quantity),
                    Sales       = g.Sum(x => x.item.TotalPrice),
                    Cost        = g.Sum(x => x.item.Quantity * x.item.PurchasePrice),
                    Profit      = g.Sum(x => x.item.TotalPrice - x.item.Quantity * x.item.PurchasePrice),
                    Margin      = g.Sum(x => x.item.TotalPrice) > 0
                        ? (g.Sum(x => x.item.TotalPrice - x.item.Quantity * x.item.PurchasePrice)
                           / g.Sum(x => x.item.TotalPrice) * 100)
                        : 0m
                })
                .OrderByDescending(p => p.Profit)
                .ToList();

            using var report = LoadOrBuild("ProfitReport.frx",
                () => ReportTemplateBuilder.BuildProfitReportTemplate(company));

            report.RegisterData(profitData, "ProfitData");
            SetParams(report,
                ("DateFrom",    from.ToString("dd/MM/yyyy")),
                ("DateTo",      to.ToString("dd/MM/yyyy")),
                ("TotalSales",  profitData.Sum(p => p.Sales).ToString("N2")),
                ("TotalCost",   profitData.Sum(p => p.Cost).ToString("N2")),
                ("TotalProfit", profitData.Sum(p => p.Profit).ToString("N2")));

            report.Prepare();
            return Export(report, format);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Profit report failed");
            throw;
        }
    }

    // ════════════════════════════════════════════════════════════════
    // INVENTORY REPORT
    // ════════════════════════════════════════════════════════════════
    public async Task<byte[]> GenerateInventoryReportAsync(
        int? warehouseId, string format = "pdf", CancellationToken ct = default)
    {
        try
        {
            var stocks = await _uow.Inventory.GetAllStockByWarehouseAsync(warehouseId ?? 0, ct);
            var (company, _, _, _, _) = await GetCompanySettingsAsync(ct);

            var data = stocks.Select(s => new
            {
                Barcode      = s.Product?.Barcode ?? "",
                NameAr       = s.Product?.NameAr ?? "",
                CategoryName = s.Product?.Category?.NameAr ?? "",
                UnitName     = s.Product?.BaseUnit?.Name ?? "",
                s.Quantity,
                MinStock     = s.Product?.MinStock ?? 0,
                s.AverageCost,
                StockValue   = s.StockValue,
                IsLowStock   = s.Product != null && s.Quantity <= s.Product.MinStock
            }).ToList();

            using var report = LoadOrBuild("InventoryReport.frx",
                () => ReportTemplateBuilder.BuildInventoryReportTemplate(company));

            report.RegisterData(data, "StockData");
            SetParams(report,
                ("ReportDate", DateTime.Today.ToString("dd/MM/yyyy")),
                ("ItemCount",  data.Count.ToString()),
                ("TotalValue", data.Sum(d => d.StockValue).ToString("N2")));

            report.Prepare();
            return Export(report, format);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Inventory report failed");
            throw;
        }
    }

    // ════════════════════════════════════════════════════════════════
    // CUSTOMER ACCOUNT STATEMENT
    // ════════════════════════════════════════════════════════════════
    public async Task<byte[]> GenerateCustomerAccountAsync(
        int customerId, DateTime from, DateTime to,
        string format = "pdf", CancellationToken ct = default)
    {
        try
        {
            var customer = await _uow.Customers.GetByIdWithDetailsAsync(customerId, ct)
                ?? throw new InvalidOperationException("Customer not found.");

            var invoices = await _uow.Sales.GetByCustomerAsync(customerId, from, to, ct);
            var payments = await _uow.Customers.GetPaymentsByCustomerAsync(customerId, from, to, ct);
            var (company, _, _, _, _) = await GetCompanySettingsAsync(ct);

            // Build statement transactions
            var transactions = new List<object>();
            decimal runningBalance = 0;

            foreach (var inv in invoices.OrderBy(i => i.InvoiceDate))
            {
                runningBalance += inv.RemainingAmount;
                transactions.Add(new
                {
                    Date      = inv.InvoiceDate.ToString("dd/MM/yyyy"),
                    TypeAr    = "فاتورة بيع",
                    Reference = inv.InvoiceNo,
                    Debit     = inv.TotalAmount,
                    Credit    = inv.PaidAmount,
                    Balance   = runningBalance,
                    Notes     = ""
                });
            }
            foreach (var pay in payments.OrderBy(p => p.PaymentDate))
            {
                runningBalance -= pay.Amount;
                transactions.Add(new
                {
                    Date      = pay.PaymentDate.ToString("dd/MM/yyyy"),
                    TypeAr    = "تحصيل",
                    Reference = pay.ReferenceNo ?? "",
                    Debit     = 0m,
                    Credit    = pay.Amount,
                    Balance   = runningBalance,
                    Notes     = pay.Notes ?? ""
                });
            }

            using var report = LoadOrBuild("CustomerAccount.frx",
                () => ReportTemplateBuilder.BuildCustomerAccountTemplate(company));

            report.RegisterData(transactions, "Transactions");
            SetParams(report,
                ("CustomerName",   customer.Name),
                ("CustomerPhone",  customer.Phone ?? ""),
                ("CurrentBalance", customer.CurrentBalance.ToString("N2")),
                ("DateFrom",       from.ToString("dd/MM/yyyy")),
                ("DateTo",         to.ToString("dd/MM/yyyy")),
                ("TotalDebit",     transactions.Sum(t => (decimal)t.GetType().GetProperty("Debit")!.GetValue(t)!).ToString("N2")),
                ("TotalCredit",    transactions.Sum(t => (decimal)t.GetType().GetProperty("Credit")!.GetValue(t)!).ToString("N2")),
                ("FinalBalance",   customer.CurrentBalance.ToString("N2")));

            report.Prepare();
            return Export(report, format);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Customer account failed for CustomerId={Id}", customerId);
            throw;
        }
    }

    // ════════════════════════════════════════════════════════════════
    // SUPPLIER ACCOUNT
    // ════════════════════════════════════════════════════════════════
    public async Task<byte[]> GenerateSupplierAccountAsync(
        int supplierId, DateTime from, DateTime to,
        string format = "pdf", CancellationToken ct = default)
    {
        try
        {
            var supplier = await _uow.Suppliers.GetByIdAsync(supplierId, ct)
                ?? throw new InvalidOperationException("Supplier not found.");

            var invoices = await _uow.Purchases.GetBySupplierAsync(supplierId, from, to, ct);
            var (company, _, _, _, _) = await GetCompanySettingsAsync(ct);

            // Reuse customer account template (same structure)
            var transactions = invoices.OrderBy(i => i.InvoiceDate).Select(inv => new
            {
                Date      = inv.InvoiceDate.ToString("dd/MM/yyyy"),
                TypeAr    = "فاتورة شراء",
                Reference = inv.InvoiceNo,
                Debit     = inv.TotalAmount,
                Credit    = inv.PaidAmount,
                Balance   = inv.TotalAmount - inv.PaidAmount,
                Notes     = inv.SupplierRefNo ?? ""
            }).ToList<object>();

            using var report = LoadOrBuild("SupplierAccount.frx",
                () => ReportTemplateBuilder.BuildCustomerAccountTemplate(company));

            report.RegisterData(transactions, "Transactions");
            SetParams(report,
                ("CustomerName",   supplier.Name),
                ("CustomerPhone",  supplier.Phone ?? ""),
                ("CurrentBalance", supplier.CurrentBalance.ToString("N2")),
                ("DateFrom",       from.ToString("dd/MM/yyyy")),
                ("DateTo",         to.ToString("dd/MM/yyyy")),
                ("TotalDebit",     invoices.Sum(i => i.TotalAmount).ToString("N2")),
                ("TotalCredit",    invoices.Sum(i => i.PaidAmount).ToString("N2")),
                ("FinalBalance",   supplier.CurrentBalance.ToString("N2")));

            report.Prepare();
            return Export(report, format);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Supplier account failed for SupplierId={Id}", supplierId);
            throw;
        }
    }

    // ════════════════════════════════════════════════════════════════
    // SHIFT REPORT
    // ════════════════════════════════════════════════════════════════
    public async Task<byte[]> GenerateShiftReportAsync(
        int shiftId, string format = "pdf", CancellationToken ct = default)
    {
        try
        {
            var invoices = await _uow.Sales.GetByShiftAsync(shiftId, ct);
            var (company, _, _, _, _) = await GetCompanySettingsAsync(ct);

            var rows = invoices.Select(inv => new
            {
                inv.InvoiceNo,
                InvoiceTime    = inv.InvoiceDate.ToString("hh:mm tt"),
                CustomerName   = inv.Customer?.Name ?? "نقدي",
                PaymentMethodAr= MapPaymentMethodAr(inv.PaymentMethod.ToString()),
                inv.TotalAmount,
                StatusAr       = MapStatusAr(inv.Status.ToString())
            }).ToList();

            var firstInvoice = invoices.OrderBy(i => i.InvoiceDate).FirstOrDefault();
            var cashierName  = firstInvoice?.User?.FullName ?? "";
            var shiftDate    = firstInvoice?.InvoiceDate.ToString("dd/MM/yyyy") ?? "";
            var totalSales   = rows.Sum(r => r.TotalAmount);

            // Get expenses for this shift
            decimal totalExpenses = 0; // TODO: query shift expenses when shift entity available

            using var report = LoadOrBuild("ShiftReport.frx",
                () => ReportTemplateBuilder.BuildShiftReportTemplate(company));

            report.RegisterData(rows, "Invoices");
            SetParams(report,
                ("ShiftNo",       shiftId.ToString()),
                ("CashierName",   cashierName),
                ("ShiftDate",     shiftDate),
                ("OpenBalance",   "0.00"),
                ("TotalSales",    totalSales.ToString("N2")),
                ("TotalReturns",  "0.00"),
                ("TotalExpenses", totalExpenses.ToString("N2")),
                ("CloseBalance",  totalSales.ToString("N2")),
                ("InvoiceCount",  rows.Count.ToString()));

            report.Prepare();
            return Export(report, format);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Shift report failed for ShiftId={Id}", shiftId);
            throw;
        }
    }

    // ════════════════════════════════════════════════════════════════
    // PRIVATE HELPERS
    // ════════════════════════════════════════════════════════════════
    private Report LoadOrBuild(string fileName, Func<Report> builder)
    {
        var path = Path.Combine(_templatesPath, fileName);
        var report = builder();  // always build fresh (template files are optional overrides)
        if (File.Exists(path))
        {
            try { report.Load(path); }
            catch (Exception ex) { _logger.LogWarning(ex, "Failed to load template {File}, using built-in", fileName); }
        }
        return report;
    }

    private static void SetParams(Report report, params (string Key, string Value)[] parameters)
    {
        foreach (var (key, value) in parameters)
            report.SetParameterValue(key, value);
    }

    private static byte[] Export(Report report, string format)
    {
        using var ms = new MemoryStream();
        switch (format.ToLowerInvariant())
        {
            case "xlsx":
            case "excel":
                var xlsx = new XlsxExport();
                report.Export(xlsx, ms);
                break;
            default: // pdf
                var pdf = new PDFSimpleExport
                {
                    EmbedFonts        = true,
                    Background        = true,
                    PrintOptimized    = false,
                    PdfCompliance     = PdfStandard.None
                };
                report.Export(pdf, ms);
                break;
        }
        return ms.ToArray();
    }

    private static string MapPaymentMethodAr(string method) => method switch
    {
        "Cash"         => "نقدي",
        "Visa"         => "فيزا",
        "BankTransfer" => "تحويل بنكي",
        "EWallet"      => "محفظة إلكترونية",
        "Credit"       => "آجل",
        "Mixed"        => "مختلط",
        _              => method
    };

    private static string MapStatusAr(string status) => status switch
    {
        "Completed"    => "مكتملة",
        "PartialReturn"=> "مرتجع جزئي",
        "FullReturn"   => "مرتجع كلي",
        "Cancelled"    => "ملغية",
        "Draft"        => "مسودة",
        "Held"         => "معلقة",
        _              => status
    };
}
