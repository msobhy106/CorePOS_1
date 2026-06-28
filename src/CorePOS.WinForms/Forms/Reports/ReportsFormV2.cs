using MediatR;
using CorePOS.WinForms.Theme;
using CorePOS.WinForms.Infrastructure;
using CorePOS.WinForms.Forms.Printing;
using CorePOS.Application.Interfaces;
using CorePOS.Application.Features.Reports.Queries;

namespace CorePOS.WinForms.Forms.Reports;

/// <summary>
/// Phase 10 — Reports screen with full print + export integration.
/// Left: report menu. Right: filters + live DataGrid preview + Print/PDF/Excel buttons.
/// Each report type → calls ReportService → shows PrintPreviewForm.
/// </summary>
public sealed class ReportsFormV2 : BaseForm
{
    private readonly IReportService  _reportService;
    private readonly IPrinterService _printerService;

    // ── Controls ──────────────────────────────────────────────────
    private Panel        _pnlLeft      = null!;
    private Panel        _pnlFilters   = null!;
    private DataGridView _dgvPreview   = null!;
    private Label        _lblTitle     = null!;
    private Label        _lblSummary   = null!;
    private DateTimePicker _dtFrom     = null!;
    private DateTimePicker _dtTo       = null!;
    private ComboBox     _cmbBranch    = null!;
    private Button       _btnRun       = null!;
    private Button       _btnPrint     = null!;
    private Button       _btnPdf       = null!;
    private Button       _btnExcel     = null!;
    private Panel        _pnlExtra     = null!;  // extra filters per report type

    private string       _activeReport = string.Empty;
    private byte[]?      _lastPdfBytes;
    private byte[]?      _lastXlsxBytes;

    // ── Report menu definitions ───────────────────────────────────
    private static readonly (string Key, string Label, string Icon)[] ReportMenu =
    [
        ("SalesPeriod",        "مبيعات الفترة",          "📈"),
        ("SalesToday",         "مبيعات اليوم",            "🗓"),
        ("Profit",             "تقرير الأرباح",           "💹"),
        ("CustomerDebts",      "مديونية العملاء",         "👥"),
        ("CustomerStatement",  "كشف حساب عميل",          "📋"),
        ("SupplierDues",       "مستحقات الموردين",        "🚚"),
        ("SupplierStatement",  "كشف حساب مورد",          "📋"),
        ("StockReport",        "تقرير المخزون",           "📦"),
        ("LowStock",           "الأصناف الناقصة",         "⚠"),
        ("SlowMoving",         "الأصناف الراكدة",         "🐌"),
        ("Expenses",           "تقرير المصروفات",         "💸"),
        ("CashboxMovement",    "حركة الخزنة",             "🏧"),
        ("ShiftReport",        "تقرير وردية",             "⏰"),
        ("CashierPerformance", "أداء الكاشيرين",          "👤"),
    ];

    public ReportsFormV2(IMediator mediator, IReportService reportService, IPrinterService printerService)
        : base(mediator)
    {
        _reportService  = reportService;
        _printerService = printerService;
        Text      = "التقارير";
        BackColor = AppTheme.BgContent;
        InitializeComponent();
    }

    // ══════════════════════════════════════════════════════════════
    // INITIALIZE
    // ══════════════════════════════════════════════════════════════
    private void InitializeComponent()
    {
        // ── Left sidebar menu ──────────────────────────────────────
        _pnlLeft = new Panel
        {
            Dock      = DockStyle.Left,
            Width     = 215,
            BackColor = AppTheme.BgCard
        };

        var lblMenuTitle = new Label
        {
            Text      = "📊 التقارير",
            Font      = AppTheme.FontH2,
            ForeColor = AppTheme.TextPrimary,
            Dock      = DockStyle.Top,
            Height    = 48,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = AppTheme.BgCard
        };

        var menuSep = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = AppTheme.Border };

        var menuScroll = new Panel
        {
            Dock      = DockStyle.Fill,
            AutoScroll= true,
            BackColor = AppTheme.BgCard
        };

        int menuY = 0;
        Button? activeBtn = null;
        foreach (var (key, label, icon) in ReportMenu)
        {
            var btn = new Button
            {
                Text      = $"{icon}  {label}",
                Location  = new Point(0, menuY),
                Width     = 214,
                Height    = 44,
                FlatStyle = FlatStyle.Flat,
                BackColor = AppTheme.BgCard,
                ForeColor = AppTheme.TextPrimary,
                Font      = AppTheme.FontBody,
                TextAlign = ContentAlignment.MiddleRight,
                Cursor    = Cursors.Hand,
                Tag       = key,
                Padding   = new Padding(0, 0, 12, 0)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, _) =>
            {
                if (s is not Button b) return;
                // Deactivate previous
                if (activeBtn != null)
                {
                    activeBtn.BackColor = AppTheme.BgCard;
                    activeBtn.ForeColor = AppTheme.TextPrimary;
                }
                // Activate this
                b.BackColor = AppTheme.BgSidebarActive;
                b.ForeColor = Color.White;
                activeBtn   = b;
                _activeReport = b.Tag?.ToString() ?? string.Empty;
                OnReportSelected(_activeReport, label);
            };
            menuScroll.Controls.Add(btn);
            menuY += 44;
        }

        _pnlLeft.Controls.AddRange([menuScroll, menuSep, lblMenuTitle]);

        // ── Right content area ─────────────────────────────────────
        var pnlRight = new Panel
        {
            Dock      = DockStyle.Fill,
            BackColor = AppTheme.BgContent,
            Padding   = new Padding(12, 8, 12, 8)
        };

        // Title bar
        _lblTitle = new Label
        {
            Text      = "اختر تقريراً من القائمة",
            Dock      = DockStyle.Top,
            Height    = 44,
            Font      = AppTheme.FontH1,
            ForeColor = AppTheme.TextPrimary,
            TextAlign = ContentAlignment.MiddleRight
        };

        // Filters panel
        _pnlFilters = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 56,
            BackColor = AppTheme.BgCard,
            Padding   = new Padding(8, 8, 8, 8)
        };

        var lblFrom = new Label
        {
            Text = "من:", Dock = DockStyle.Right, Width = 30,
            Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel,
            TextAlign = ContentAlignment.MiddleRight, AutoSize = false
        };
        _dtFrom = new DateTimePicker
        {
            Dock = DockStyle.Right, Width = 140,
            Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(-30),
            Font = AppTheme.FontBody
        };

        var lblTo = new Label
        {
            Text = "إلى:", Dock = DockStyle.Right, Width = 36,
            Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel,
            TextAlign = ContentAlignment.MiddleRight, AutoSize = false
        };
        _dtTo = new DateTimePicker
        {
            Dock = DockStyle.Right, Width = 140,
            Format = DateTimePickerFormat.Short, Value = DateTime.Today,
            Font = AppTheme.FontBody
        };

        // Action buttons
        var flowBtns = new FlowLayoutPanel
        {
            Dock = DockStyle.Left, Width = 460,
            FlowDirection = FlowDirection.LeftToRight,
            BackColor = AppTheme.BgCard, WrapContents = false
        };

        Button AB(string t, Color c, EventHandler h, int w = 110)
        {
            var b = new Button
            {
                Text = t, Width = w, Height = 36,
                FlatStyle = FlatStyle.Flat, BackColor = c,
                ForeColor = Color.White, Font = AppTheme.FontSmallBold,
                Cursor = Cursors.Hand, Margin = new Padding(0, 0, 6, 0)
            };
            b.FlatAppearance.BorderSize = 0; b.Click += h; return b;
        }

        _btnRun   = AB("▶ تشغيل",       AppTheme.AccentBlue,   (_, _) => RunReport());
        _btnPrint = AB("🖨 طباعة",       AppTheme.AccentOrange, (_, _) => PrintReport());
        _btnPdf   = AB("📄 PDF",         AppTheme.AccentRed,    (_, _) => ExportPdf(), 80);
        _btnExcel = AB("📊 Excel",       AppTheme.AccentGreen,  (_, _) => ExportExcel(), 90);

        _btnPrint.Enabled = false;
        _btnPdf.Enabled   = false;
        _btnExcel.Enabled = false;

        flowBtns.Controls.AddRange([_btnRun, _btnPrint, _btnPdf, _btnExcel]);
        _pnlFilters.Controls.AddRange([lblFrom, _dtFrom, lblTo, _dtTo, flowBtns]);

        // Extra filters (dynamic per report)
        _pnlExtra = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 0,
            BackColor = AppTheme.BgCard
        };

        // Summary bar
        var pnlSummary = new Panel
        {
            Dock = DockStyle.Bottom, Height = 36,
            BackColor = AppTheme.BgCard, Padding = new Padding(8, 0, 8, 0)
        };
        _lblSummary = new Label
        {
            Dock = DockStyle.Fill, Font = AppTheme.FontSmallBold,
            ForeColor = AppTheme.AccentBlue, TextAlign = ContentAlignment.MiddleRight
        };
        var sepBottom = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = AppTheme.Border };
        pnlSummary.Controls.AddRange([_lblSummary, sepBottom]);

        // Preview grid
        var pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.BgCard };
        _dgvPreview = new DataGridView { Dock = DockStyle.Fill };
        AppTheme.StyleDataGrid(_dgvPreview);
        _dgvPreview.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        pnlGrid.Controls.Add(_dgvPreview);

        pnlRight.Controls.AddRange([pnlSummary, pnlGrid, _pnlExtra, _pnlFilters, _lblTitle]);
        Controls.AddRange([pnlRight, _pnlLeft]);
    }

    // ══════════════════════════════════════════════════════════════
    // REPORT SELECTION
    // ══════════════════════════════════════════════════════════════
    private void OnReportSelected(string key, string label)
    {
        _lblTitle.Text = label;
        _lblSummary.Text = string.Empty;
        _dgvPreview.Rows.Clear();
        _dgvPreview.Columns.Clear();
        _btnPrint.Enabled = false;
        _btnPdf.Enabled   = false;
        _btnExcel.Enabled = false;
        _lastPdfBytes     = null;
        _lastXlsxBytes    = null;

        // Build extra filters for specific reports
        BuildExtraFilters(key);

        // Auto-run for some reports
        if (key is "CustomerDebts" or "SupplierDues" or "StockReport" or "LowStock")
            RunReport();
    }

    private ComboBox? _cmbCustomer, _cmbSupplier, _cmbShift, _cmbWarehouse;

    private void BuildExtraFilters(string key)
    {
        _pnlExtra.Controls.Clear();
        _cmbCustomer = null; _cmbSupplier = null; _cmbShift = null; _cmbWarehouse = null;

        switch (key)
        {
            case "CustomerStatement":
                _pnlExtra.Height = 48;
                _pnlExtra.Controls.Add(new Label
                {
                    Text = "اختر عميل:", Location = new Point(300, 14),
                    AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel
                });
                _cmbCustomer = new ComboBox
                {
                    Location = new Point(16, 10), Width = 280,
                    DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody
                };
                _pnlExtra.Controls.Add(_cmbCustomer);
                LoadCustomersCombo(_cmbCustomer);
                break;

            case "SupplierStatement":
                _pnlExtra.Height = 48;
                _pnlExtra.Controls.Add(new Label
                {
                    Text = "اختر مورد:", Location = new Point(300, 14),
                    AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel
                });
                _cmbSupplier = new ComboBox
                {
                    Location = new Point(16, 10), Width = 280,
                    DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody
                };
                _pnlExtra.Controls.Add(_cmbSupplier);
                LoadSuppliersCombo(_cmbSupplier);
                break;

            case "ShiftReport":
                _pnlExtra.Height = 48;
                _pnlExtra.Controls.Add(new Label
                {
                    Text = "رقم الوردية:", Location = new Point(300, 14),
                    AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel
                });
                _cmbShift = new ComboBox
                {
                    Location = new Point(16, 10), Width = 200,
                    DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody
                };
                _pnlExtra.Controls.Add(_cmbShift);
                LoadShiftsCombo(_cmbShift);
                break;

            case "StockReport":
            case "LowStock":
                _pnlExtra.Height = 48;
                _pnlExtra.Controls.Add(new Label
                {
                    Text = "المستودع:", Location = new Point(300, 14),
                    AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel
                });
                _cmbWarehouse = new ComboBox
                {
                    Location = new Point(16, 10), Width = 240,
                    DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody
                };
                _pnlExtra.Controls.Add(_cmbWarehouse);
                LoadWarehousesCombo(_cmbWarehouse);
                break;

            default:
                _pnlExtra.Height = 0;
                break;
        }
    }

    // ══════════════════════════════════════════════════════════════
    // RUN REPORT — loads data into grid
    // ══════════════════════════════════════════════════════════════
    private void RunReport()
    {
        if (string.IsNullOrEmpty(_activeReport)) return;

        _btnRun.Enabled   = false;
        _btnPrint.Enabled = false;
        _btnPdf.Enabled   = false;
        _btnExcel.Enabled = false;

        ShowLoading("جاري إنشاء التقرير...");

        Task.Run(async () =>
        {
            try
            {
                await LoadReportDataAsync(_activeReport);
                InvokeOnUI(() =>
                {
                    HideLoading();
                    _btnRun.Enabled   = true;
                    _btnPrint.Enabled = true;
                    _btnPdf.Enabled   = true;
                    _btnExcel.Enabled = true;
                });
            }
            catch (Exception ex)
            {
                InvokeOnUI(() =>
                {
                    HideLoading();
                    _btnRun.Enabled = true;
                    ShowError("فشل التقرير: " + ex.Message);
                });
            }
        });
    }

    private async Task LoadReportDataAsync(string key)
    {
        var branchId = UserSession.Current.BranchId;
        var from     = _dtFrom.Value.Date;
        var to       = _dtTo.Value.Date.AddDays(1).AddSeconds(-1);

        switch (key)
        {
            case "SalesPeriod":
            case "SalesToday":
            {
                if (key == "SalesToday") { from = DateTime.Today; to = DateTime.Today.AddDays(1).AddSeconds(-1); }
                var result = await _mediator.Send(new GetSalesReportQuery(branchId, from, to));
                if (!result.IsSuccess || result.Value == null) return;
                _lastPdfBytes  = await _reportService.GenerateSalesReportAsync(from, to, branchId, "pdf");
                _lastXlsxBytes = await _reportService.GenerateSalesReportAsync(from, to, branchId, "xlsx");
                InvokeOnUI(() =>
                {
                    SetColumns("رقم الفاتورة", "التاريخ", "العميل", "الأصناف", "الإجمالي", "المدفوع", "الدفع");
                    FillGrid(result.Value, r => new object[]
                    {
                        r.InvoiceNo, r.Date.ToString("dd/MM/yyyy HH:mm"),
                        r.CustomerName, r.ItemsCount,
                        $"{r.Total:N2}", $"{r.Paid:N2}", r.PayMethodAr
                    });
                    _lblSummary.Text = $"عدد الفواتير: {result.Value.Count}  |  الإجمالي: {result.Value.Sum(r => r.Total):N2} ج.م";
                });
                break;
            }

            case "Profit":
            {
                var result = await _mediator.Send(new GetProfitReportQuery(branchId, from, to));
                if (!result.IsSuccess || result.Value == null) return;
                _lastPdfBytes  = await _reportService.GenerateProfitReportAsync(from, to, branchId, "pdf");
                _lastXlsxBytes = await _reportService.GenerateProfitReportAsync(from, to, branchId, "xlsx");
                InvokeOnUI(() =>
                {
                    SetColumns("الصنف", "الكمية", "المبيعات", "التكلفة", "الربح", "هامش%");
                    FillGrid(result.Value, r => new object[]
                    {
                        r.ProductName, $"{r.QtySold:N2}",
                        $"{r.Sales:N2}", $"{r.Cost:N2}", $"{r.Profit:N2}",
                        $"{(r.Sales > 0 ? r.Profit / r.Sales * 100 : 0):N1}%"
                    });
                    _lblSummary.Text = $"إجمالي الربح: {result.Value.Sum(r => r.Profit):N2} ج.م";
                    // Color profit cells
                    ColorColumn("الربح", r => r.Cells["الربح"].Value?.ToString()?.StartsWith("-") == true
                        ? AppTheme.AccentRed : AppTheme.AccentGreen);
                });
                break;
            }

            case "CustomerDebts":
            {
                var result = await _mediator.Send(new GetCustomerDebtsReportQuery(branchId));
                if (!result.IsSuccess || result.Value == null) return;
                var debts = result.Value.Where(r => r.Balance > 0).ToList();
                InvokeOnUI(() =>
                {
                    SetColumns("العميل", "الهاتف", "المشتريات", "المدفوع", "المتبقي", "آخر فاتورة");
                    FillGrid(debts, r => new object[]
                    {
                        r.Name, r.Phone, $"{r.TotalPurchases:N2}",
                        $"{r.TotalPaid:N2}", $"{r.Balance:N2}",
                        r.LastInvoiceDate.ToString("dd/MM/yyyy")
                    });
                    _lblSummary.Text = $"عدد العملاء: {debts.Count}  |  إجمالي الديون: {debts.Sum(r => r.Balance):N2} ج.م";
                });
                _lastPdfBytes = await _reportService.GenerateSalesReportAsync(from, to, branchId, "pdf");
                break;
            }

            case "CustomerStatement":
            {
                var custItem = _cmbCustomer?.SelectedItem as MasterData.ComboItem;
                if (custItem == null || custItem.Id == 0) { ShowError("اختر عميلاً أولاً"); return; }
                _lastPdfBytes  = await _reportService.GenerateCustomerAccountAsync(custItem.Id, from, to, "pdf");
                _lastXlsxBytes = await _reportService.GenerateCustomerAccountAsync(custItem.Id, from, to, "xlsx");
                InvokeOnUI(() => _lblSummary.Text = $"كشف حساب: {custItem.Name}");
                break;
            }

            case "SupplierDues":
            {
                var result = await _mediator.Send(new GetSupplierDuesReportQuery(branchId));
                if (!result.IsSuccess || result.Value == null) return;
                var dues = result.Value.Where(r => r.Balance > 0).ToList();
                InvokeOnUI(() =>
                {
                    SetColumns("المورد", "الهاتف", "إجمالي مشتريات", "المدفوع", "المستحق", "آخر فاتورة");
                    FillGrid(dues, r => new object[]
                    {
                        r.Name, r.Phone, $"{r.TotalPurchases:N2}",
                        $"{r.TotalPaid:N2}", $"{r.Balance:N2}",
                        r.LastInvoiceDate.ToString("dd/MM/yyyy")
                    });
                    _lblSummary.Text = $"إجمالي المستحقات: {dues.Sum(r => r.Balance):N2} ج.م";
                });
                break;
            }

            case "SupplierStatement":
            {
                var supItem = _cmbSupplier?.SelectedItem as MasterData.ComboItem;
                if (supItem == null || supItem.Id == 0) { ShowError("اختر مورداً أولاً"); return; }
                _lastPdfBytes  = await _reportService.GenerateSupplierAccountAsync(supItem.Id, from, to, "pdf");
                _lastXlsxBytes = await _reportService.GenerateSupplierAccountAsync(supItem.Id, from, to, "xlsx");
                InvokeOnUI(() => _lblSummary.Text = $"كشف حساب مورد: {supItem.Name}");
                break;
            }

            case "StockReport":
            {
                var whId   = (_cmbWarehouse?.SelectedItem as MasterData.ComboItem)?.Id;
                _lastPdfBytes  = await _reportService.GenerateInventoryReportAsync(whId == 0 ? null : whId, "pdf");
                _lastXlsxBytes = await _reportService.GenerateInventoryReportAsync(whId == 0 ? null : whId, "xlsx");
                var result = await _mediator.Send(new Inventory.Queries.GetStockListQuery(branchId, whId == 0 ? null : whId, string.Empty));
                if (!result.IsSuccess || result.Value == null) return;
                InvokeOnUI(() =>
                {
                    SetColumns("باركود", "الصنف", "القسم", "الكمية", "الحد الأدنى", "متوسط التكلفة", "القيمة");
                    FillGrid(result.Value, s => new object[]
                    {
                        s.Barcode, s.NameAr, s.CategoryName, $"{s.Quantity:N2}",
                        $"{s.MinStock}", $"{s.AverageCost:N2}",
                        $"{s.Quantity * s.AverageCost:N2}"
                    });
                    // Red rows for low stock
                    for (int i = 0; i < _dgvPreview.Rows.Count; i++)
                    {
                        var s = result.Value[i];
                        if (s.Quantity <= s.MinStock)
                            _dgvPreview.Rows[i].DefaultCellStyle.ForeColor = AppTheme.AccentRed;
                    }
                    _lblSummary.Text = $"الأصناف: {result.Value.Count}  |  إجمالي القيمة: {result.Value.Sum(s => s.Quantity * s.AverageCost):N2} ج.م";
                });
                break;
            }

            case "LowStock":
            {
                var result = await _mediator.Send(new GetLowStockReportQuery(branchId));
                if (!result.IsSuccess || result.Value == null) return;
                InvokeOnUI(() =>
                {
                    SetColumns("باركود", "الصنف", "الكمية الحالية", "الحد الأدنى", "الفارق", "المورد");
                    FillGrid(result.Value, r => new object[]
                    {
                        r.Barcode, r.NameAr, $"{r.CurrentQty:N2}", $"{r.MinStock}",
                        $"{r.CurrentQty - r.MinStock:N2}", r.DefaultSupplier
                    });
                    _lblSummary.Text = $"أصناف ناقصة: {result.Value.Count}";
                    for (int i = 0; i < _dgvPreview.Rows.Count; i++)
                        _dgvPreview.Rows[i].DefaultCellStyle.ForeColor = AppTheme.AccentRed;
                });
                break;
            }

            case "SlowMoving":
            {
                var result = await _mediator.Send(new GetSlowMovingReportQuery(branchId, from, to));
                if (!result.IsSuccess || result.Value == null) return;
                InvokeOnUI(() =>
                {
                    SetColumns("باركود", "الصنف", "الكمية المباعة", "المخزون الحالي", "آخر حركة");
                    FillGrid(result.Value, r => new object[]
                    {
                        r.Barcode, r.NameAr, $"{r.QtySold:N2}", $"{r.CurrentStock:N2}",
                        r.LastMovement?.ToString("dd/MM/yyyy") ?? "لا توجد حركة"
                    });
                    _lblSummary.Text = $"أصناف راكدة: {result.Value.Count}";
                });
                break;
            }

            case "Expenses":
            {
                var result = await _mediator.Send(new GetExpensesReportQuery(branchId, from, to));
                if (!result.IsSuccess || result.Value == null) return;
                InvokeOnUI(() =>
                {
                    SetColumns("التاريخ", "النوع", "المبلغ", "الوصف", "بواسطة");
                    FillGrid(result.Value, r => new object[]
                    {
                        r.Date.ToString("dd/MM/yyyy HH:mm"),
                        r.TypeAr, $"{r.Amount:N2}", r.Description, r.CreatedByName
                    });
                    _lblSummary.Text = $"إجمالي المصروفات: {result.Value.Sum(r => r.Amount):N2} ج.م";
                });
                break;
            }

            case "CashboxMovement":
            {
                var result = await _mediator.Send(new GetCashboxMovementReportQuery(branchId, from, to));
                if (!result.IsSuccess || result.Value == null) return;
                InvokeOnUI(() =>
                {
                    SetColumns("التاريخ", "الخزنة", "النوع", "مدين", "دائن", "الرصيد", "ملاحظات");
                    FillGrid(result.Value, r => new object[]
                    {
                        r.Date.ToString("dd/MM/yyyy HH:mm"), r.CashboxName, r.TypeAr,
                        r.Debit > 0 ? $"{r.Debit:N2}" : "",
                        r.Credit > 0 ? $"{r.Credit:N2}" : "",
                        $"{r.Balance:N2}", r.Notes
                    });
                    _lblSummary.Text = $"عمليات: {result.Value.Count}";
                });
                break;
            }

            case "ShiftReport":
            {
                var shiftItem = _cmbShift?.SelectedItem as MasterData.ComboItem;
                if (shiftItem == null || shiftItem.Id == 0) { ShowError("اختر وردية أولاً"); return; }
                _lastPdfBytes = await _reportService.GenerateShiftReportAsync(shiftItem.Id, "pdf");
                InvokeOnUI(() => _lblSummary.Text = $"تقرير الوردية: {shiftItem.Name}");
                break;
            }

            case "CashierPerformance":
            {
                var result = await _mediator.Send(new GetCashierPerformanceReportQuery(branchId, from, to));
                if (!result.IsSuccess || result.Value == null) return;
                InvokeOnUI(() =>
                {
                    SetColumns("الكاشير", "عدد الفواتير", "إجمالي المبيعات", "متوسط فاتورة", "المرتجعات");
                    FillGrid(result.Value, r => new object[]
                    {
                        r.CashierName, r.InvoiceCount,
                        $"{r.TotalSales:N2}", $"{r.AvgInvoice:N2}", $"{r.TotalReturns:N2}"
                    });
                    _lblSummary.Text = $"كاشيرين: {result.Value.Count}";
                });
                break;
            }
        }
    }

    // ══════════════════════════════════════════════════════════════
    // GRID HELPERS
    // ══════════════════════════════════════════════════════════════
    private void SetColumns(params string[] headers)
    {
        _dgvPreview.Columns.Clear();
        foreach (var h in headers)
            _dgvPreview.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = h, Name = h,
                FillWeight = 100f / headers.Length,
                MinimumWidth = 70
            });
    }

    private void FillGrid<T>(IEnumerable<T> data, Func<T, object[]> mapper)
    {
        _dgvPreview.Rows.Clear();
        foreach (var item in data)
            _dgvPreview.Rows.Add(mapper(item));
    }

    private void ColorColumn(string colName, Func<DataGridViewRow, Color> colorFunc)
    {
        if (!_dgvPreview.Columns.Contains(colName)) return;
        foreach (DataGridViewRow row in _dgvPreview.Rows)
            row.Cells[colName].Style.ForeColor = colorFunc(row);
    }

    // ══════════════════════════════════════════════════════════════
    // PRINT / EXPORT
    // ══════════════════════════════════════════════════════════════
    private void PrintReport()
    {
        if (_lastPdfBytes == null && !HasGridData())
        { ShowError("شغّل التقرير أولاً"); return; }

        if (_lastPdfBytes != null)
        {
            var preview = PrintPreviewForm.ForBytes(
                _reportService, _printerService, _lastPdfBytes, _lblTitle.Text);
            preview.ShowDialog(this);
        }
        else
        {
            ShowError("الطباعة المباشرة ستكون متاحة بعد توليد PDF");
        }
    }

    private void ExportPdf()
    {
        if (_lastPdfBytes == null) { RunReportAndThen(ExportPdf); return; }

        using var dlg = new SaveFileDialog
        {
            Title    = "حفظ PDF",
            Filter   = "PDF Files|*.pdf",
            FileName = $"{_lblTitle.Text.Replace(" ", "_")}_{DateTime.Today:yyyyMMdd}.pdf"
        };
        if (dlg.ShowDialog() != DialogResult.OK) return;
        File.WriteAllBytes(dlg.FileName, _lastPdfBytes);

        if (Confirm("تم الحفظ. هل تريد فتح الملف؟"))
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(dlg.FileName) { UseShellExecute = true });
    }

    private void ExportExcel()
    {
        if (_lastXlsxBytes == null) { ShowError("تصدير Excel غير متاح لهذا التقرير. جرب تشغيل التقرير أولاً."); return; }

        using var dlg = new SaveFileDialog
        {
            Title    = "حفظ Excel",
            Filter   = "Excel Files|*.xlsx",
            FileName = $"{_lblTitle.Text.Replace(" ", "_")}_{DateTime.Today:yyyyMMdd}.xlsx"
        };
        if (dlg.ShowDialog() != DialogResult.OK) return;
        File.WriteAllBytes(dlg.FileName, _lastXlsxBytes);

        if (Confirm("تم الحفظ. هل تريد فتح الملف؟"))
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(dlg.FileName) { UseShellExecute = true });
    }

    private void RunReportAndThen(Action after)
    {
        ShowLoading("جاري توليد التقرير...");
        Task.Run(async () =>
        {
            await LoadReportDataAsync(_activeReport);
            InvokeOnUI(() => { HideLoading(); after(); });
        });
    }

    private bool HasGridData() => _dgvPreview.Rows.Count > 0;

    // ══════════════════════════════════════════════════════════════
    // COMBO LOADERS
    // ══════════════════════════════════════════════════════════════
    private void LoadCustomersCombo(ComboBox cmb)
    {
        Task.Run(async () =>
        {
            var result = await _mediator.Send(new Customers.Queries.GetCustomersListQuery("", UserSession.Current.BranchId, 500));
            InvokeOnUI(() =>
            {
                if (!result.IsSuccess || result.Value == null) return;
                cmb.Items.Add(new MasterData.ComboItem(0, "-- اختر عميل --"));
                foreach (var c in result.Value.Items)
                    cmb.Items.Add(new MasterData.ComboItem(c.CustomerId, c.Name));
                cmb.SelectedIndex = 0;
            });
        });
    }

    private void LoadSuppliersCombo(ComboBox cmb)
    {
        Task.Run(async () =>
        {
            var result = await _mediator.Send(new Suppliers.Queries.GetSuppliersListQuery("", 200));
            InvokeOnUI(() =>
            {
                if (!result.IsSuccess || result.Value == null) return;
                cmb.Items.Add(new MasterData.ComboItem(0, "-- اختر مورد --"));
                foreach (var s in result.Value.Items)
                    cmb.Items.Add(new MasterData.ComboItem(s.SupplierId, s.Name));
                cmb.SelectedIndex = 0;
            });
        });
    }

    private void LoadShiftsCombo(ComboBox cmb)
    {
        Task.Run(async () =>
        {
            var result = await _mediator.Send(new Shifts.Queries.GetRecentShiftsQuery(UserSession.Current.BranchId, 30));
            InvokeOnUI(() =>
            {
                if (!result.IsSuccess || result.Value == null) return;
                cmb.Items.Add(new MasterData.ComboItem(0, "-- اختر وردية --"));
                foreach (var s in result.Value)
                    cmb.Items.Add(new MasterData.ComboItem(s.ShiftId, $"{s.ShiftNo} — {s.OpenedAt:dd/MM/yyyy HH:mm}"));
                cmb.SelectedIndex = 0;
            });
        });
    }

    private void LoadWarehousesCombo(ComboBox cmb)
    {
        Task.Run(async () =>
        {
            var result = await _mediator.Send(new Inventory.Queries.GetWarehousesQuery(UserSession.Current.BranchId));
            InvokeOnUI(() =>
            {
                if (!result.IsSuccess || result.Value == null) return;
                cmb.Items.Add(new MasterData.ComboItem(0, "كل المستودعات"));
                foreach (var w in result.Value)
                    cmb.Items.Add(new MasterData.ComboItem(w.WarehouseId, w.Name));
                cmb.SelectedIndex = 0;
            });
        });
    }
}
