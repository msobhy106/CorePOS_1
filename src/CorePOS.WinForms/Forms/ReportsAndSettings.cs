using MediatR;
using CorePOS.WinForms.Theme;
using CorePOS.WinForms.Infrastructure;
using CorePOS.Application.Features.Reports.Queries;
using CorePOS.Application.Features.Settings.Queries;
using CorePOS.Application.Features.Settings.Commands;

namespace CorePOS.WinForms.Forms.Reports;

/// <summary>
/// Reports hub — left panel: report categories; right panel: filters + grid.
/// </summary>
public sealed class ReportsForm : BaseForm
{
    private Panel        _pnlFilters = null!;
    private DataGridView _dgvReport  = null!;
    private Label        _lblTitle   = null!;
    private Label        _lblSummary = null!;
    private DateTimePicker _dtFrom   = null!;
    private DateTimePicker _dtTo     = null!;
    private string       _currentReport = string.Empty;

    public ReportsForm(IMediator mediator) : base(mediator)
    {
        Text      = "التقارير";
        BackColor = AppTheme.BgContent;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        // ── Left: report category list ────────────────────────────
        var pnlLeft = new Panel { Dock = DockStyle.Left, Width = 210, BackColor = AppTheme.BgCard };

        var lblMenuTitle = new Label
        {
            Text = "📊 التقارير", Font = AppTheme.FontH2, ForeColor = AppTheme.TextPrimary,
            Dock = DockStyle.Top, Height = 44, TextAlign = ContentAlignment.MiddleCenter
        };
        var sep = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = AppTheme.Border };

        var reportButtons = new[]
        {
            ("📈 مبيعات اليوم",      "SalesToday"),
            ("📅 مبيعات الفترة",     "SalesPeriod"),
            ("💹 تقرير الأرباح",     "Profit"),
            ("👥 مديونية العملاء",   "CustomerDebts"),
            ("📋 كشف حساب عميل",    "CustomerStatement"),
            ("🚚 مستحقات الموردين",  "SupplierDues"),
            ("📦 المخزون الحالي",    "StockReport"),
            ("⚠ الأصناف الناقصة",   "LowStock"),
            ("🐌 الأصناف الراكدة",   "SlowMoving"),
            ("💰 تقرير المصروفات",   "Expenses"),
            ("🏧 حركة الخزنة",       "CashboxMovement"),
            ("👤 أداء الكاشيرين",    "CashierPerf"),
        };

        var menuPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = AppTheme.BgCard };
        int menuY = 0;
        foreach (var (label, key) in reportButtons)
        {
            var btn = new Button
            {
                Text      = label,
                Location  = new Point(0, menuY),
                Width     = 210,
                Height    = 42,
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
                if (s is Button b && b.Tag is string k)
                {
                    _currentReport = k;
                    // Highlight
                    foreach (Control c in menuPanel.Controls)
                        if (c is Button mb) mb.BackColor = AppTheme.BgCard;
                    b.BackColor = AppTheme.BgSidebarActive;
                    b.ForeColor = Color.White;
                    LoadReport(k);
                }
            };
            menuPanel.Controls.Add(btn);
            menuY += 42;
        }

        pnlLeft.Controls.AddRange([menuPanel, sep, lblMenuTitle]);

        // ── Right: filters + grid ─────────────────────────────────
        var pnlRight = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.BgContent, Padding = new Padding(12, 8, 12, 8) };

        // Title
        _lblTitle = new Label
        {
            Text = "اختر تقريراً من القائمة", Dock = DockStyle.Top, Height = 44,
            Font = AppTheme.FontH1, ForeColor = AppTheme.TextPrimary, TextAlign = ContentAlignment.MiddleRight
        };

        // Filters bar
        _pnlFilters = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = AppTheme.BgCard, Padding = new Padding(8) };

        var lblFrom = new Label { Text = "من:", Dock = DockStyle.Right, AutoSize = false, Width = 30, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel, TextAlign = ContentAlignment.MiddleRight };
        _dtFrom = new DateTimePicker { Dock = DockStyle.Right, Width = 140, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(-30), Font = AppTheme.FontBody };
        var lblTo = new Label { Text = "إلى:", Dock = DockStyle.Right, AutoSize = false, Width = 36, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel, TextAlign = ContentAlignment.MiddleRight };
        _dtTo = new DateTimePicker { Dock = DockStyle.Right, Width = 140, Format = DateTimePickerFormat.Short, Value = DateTime.Today, Font = AppTheme.FontBody };

        var btnRun = new Button { Text = "▶ تشغيل", Dock = DockStyle.Right, Width = 100, Height = 36, FlatStyle = FlatStyle.Flat, BackColor = AppTheme.AccentBlue, ForeColor = Color.White, Font = AppTheme.FontBodyBold, Cursor = Cursors.Hand };
        btnRun.FlatAppearance.BorderSize = 0;
        btnRun.Click += (_, _) => LoadReport(_currentReport);

        var btnExport = new Button { Text = "📤 تصدير", Dock = DockStyle.Right, Width = 100, Height = 36, FlatStyle = FlatStyle.Flat, BackColor = AppTheme.AccentPurple, ForeColor = Color.White, Font = AppTheme.FontBodyBold, Cursor = Cursors.Hand, Margin = new Padding(0, 0, 6, 0) };
        btnExport.FlatAppearance.BorderSize = 0;
        btnExport.Click += (_, _) => ExportReport();

        var btnPrint = new Button { Text = "🖨 طباعة", Dock = DockStyle.Left, Width = 100, Height = 36, FlatStyle = FlatStyle.Flat, BackColor = AppTheme.AccentOrange, ForeColor = Color.White, Font = AppTheme.FontBodyBold, Cursor = Cursors.Hand };
        btnPrint.FlatAppearance.BorderSize = 0;
        btnPrint.Click += (_, _) => PrintReport();

        _pnlFilters.Controls.AddRange([lblFrom, _dtFrom, lblTo, _dtTo, btnRun, btnExport, btnPrint]);

        // Summary bar
        var pnlSummary = new Panel { Dock = DockStyle.Bottom, Height = 34, BackColor = AppTheme.BgCard, Padding = new Padding(8, 0, 8, 0) };
        _lblSummary = new Label { Dock = DockStyle.Fill, Font = AppTheme.FontSmallBold, ForeColor = AppTheme.AccentBlue, TextAlign = ContentAlignment.MiddleRight };
        pnlSummary.Controls.Add(_lblSummary);

        // Grid
        var pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.BgCard };
        _dgvReport = new DataGridView { Dock = DockStyle.Fill };
        AppTheme.StyleDataGrid(_dgvReport);
        pnlGrid.Controls.Add(_dgvReport);

        pnlRight.Controls.AddRange([pnlSummary, pnlGrid, _pnlFilters, _lblTitle]);
        Controls.AddRange([pnlRight, pnlLeft]);
    }

    private void LoadReport(string reportKey)
    {
        if (string.IsNullOrEmpty(reportKey)) return;
        _currentReport = reportKey;

        RunAsync(async () =>
        {
            var from = _dtFrom.Value.Date;
            var to   = _dtTo.Value.Date.AddDays(1).AddSeconds(-1);
            var branchId = UserSession.Current.BranchId;

            // Each report type builds different columns and loads different data
            switch (reportKey)
            {
                case "SalesToday":
                case "SalesPeriod":
                    await LoadSalesReport(branchId, from, to);
                    break;
                case "Profit":
                    await LoadProfitReport(branchId, from, to);
                    break;
                case "CustomerDebts":
                    await LoadCustomerDebtsReport(branchId);
                    break;
                case "SupplierDues":
                    await LoadSupplierDuesReport(branchId);
                    break;
                case "StockReport":
                    await LoadStockReport(branchId);
                    break;
                case "LowStock":
                    await LoadLowStockReport(branchId);
                    break;
                case "SlowMoving":
                    await LoadSlowMovingReport(branchId, from, to);
                    break;
                case "Expenses":
                    await LoadExpensesReport(branchId, from, to);
                    break;
                case "CashboxMovement":
                    await LoadCashboxReport(branchId, from, to);
                    break;
                case "CashierPerf":
                    await LoadCashierPerfReport(branchId, from, to);
                    break;
            }
        }, "جاري إنشاء التقرير...");
    }

    private async Task LoadSalesReport(int branchId, DateTime from, DateTime to)
    {
        var result = await _mediator.Send(new GetSalesReportQuery(branchId, from, to));
        InvokeOnUI(() =>
        {
            SetColumns("رقم الفاتورة", "التاريخ", "العميل", "عدد الأصناف", "الإجمالي", "المدفوع", "طريقة الدفع");
            _dgvReport.Rows.Clear();
            if (!result.IsSuccess || result.Value == null) return;
            decimal total = 0;
            foreach (var r in result.Value)
            {
                _dgvReport.Rows.Add(r.InvoiceNo, r.Date.ToString("dd/MM/yyyy HH:mm"), r.CustomerName, r.ItemsCount, $"{r.Total:N2}", $"{r.Paid:N2}", r.PayMethodAr);
                total += r.Total;
            }
            _lblTitle.Text   = "تقرير المبيعات";
            _lblSummary.Text = $"عدد الفواتير: {result.Value.Count}  |  الإجمالي: {total:N2} ج.م";
        });
    }

    private async Task LoadProfitReport(int branchId, DateTime from, DateTime to)
    {
        var result = await _mediator.Send(new GetProfitReportQuery(branchId, from, to));
        InvokeOnUI(() =>
        {
            SetColumns("الصنف", "الكمية المباعة", "إجمالي المبيعات", "تكلفة البضاعة", "الربح الإجمالي", "هامش الربح%");
            _dgvReport.Rows.Clear();
            if (!result.IsSuccess || result.Value == null) return;
            decimal totalProfit = 0;
            foreach (var r in result.Value)
            {
                var margin = r.Sales > 0 ? r.Profit / r.Sales * 100 : 0;
                _dgvReport.Rows.Add(r.ProductName, r.QtySold, $"{r.Sales:N2}", $"{r.Cost:N2}", $"{r.Profit:N2}", $"{margin:N1}%");
                var rowIdx = _dgvReport.Rows.Count - 1;
                _dgvReport.Rows[rowIdx].Cells[4].Style.ForeColor = r.Profit >= 0 ? AppTheme.AccentGreen : AppTheme.AccentRed;
                totalProfit += r.Profit;
            }
            _lblTitle.Text   = "تقرير الأرباح";
            _lblSummary.Text = $"إجمالي الربح: {totalProfit:N2} ج.م";
        });
    }

    private async Task LoadCustomerDebtsReport(int branchId)
    {
        var result = await _mediator.Send(new GetCustomerDebtsReportQuery(branchId));
        InvokeOnUI(() =>
        {
            SetColumns("العميل", "الهاتف", "إجمالي المشتريات", "إجمالي المدفوع", "الرصيد المتبقي", "آخر فاتورة");
            _dgvReport.Rows.Clear();
            if (!result.IsSuccess || result.Value == null) return;
            decimal totalDebt = 0;
            foreach (var r in result.Value.Where(x => x.Balance > 0))
            {
                _dgvReport.Rows.Add(r.Name, r.Phone, $"{r.TotalPurchases:N2}", $"{r.TotalPaid:N2}", $"{r.Balance:N2}", r.LastInvoiceDate.ToString("dd/MM/yyyy"));
                _dgvReport.Rows[_dgvReport.Rows.Count - 1].Cells[4].Style.ForeColor = AppTheme.AccentRed;
                totalDebt += r.Balance;
            }
            _lblTitle.Text   = "مديونية العملاء";
            _lblSummary.Text = $"عدد العملاء: {_dgvReport.Rows.Count}  |  إجمالي الديون: {totalDebt:N2} ج.م";
        });
    }

    private async Task LoadSupplierDuesReport(int branchId)
    {
        var result = await _mediator.Send(new GetSupplierDuesReportQuery(branchId));
        InvokeOnUI(() =>
        {
            SetColumns("المورد", "الهاتف", "إجمالي المشتريات", "إجمالي المدفوع", "المستحق", "آخر فاتورة");
            _dgvReport.Rows.Clear();
            if (!result.IsSuccess || result.Value == null) return;
            decimal total = 0;
            foreach (var r in result.Value.Where(x => x.Balance > 0))
            {
                _dgvReport.Rows.Add(r.Name, r.Phone, $"{r.TotalPurchases:N2}", $"{r.TotalPaid:N2}", $"{r.Balance:N2}", r.LastInvoiceDate.ToString("dd/MM/yyyy"));
                total += r.Balance;
            }
            _lblTitle.Text   = "مستحقات الموردين";
            _lblSummary.Text = $"إجمالي المستحقات: {total:N2} ج.م";
        });
    }

    private async Task LoadStockReport(int branchId)
    {
        var result = await _mediator.Send(new GetStockReportQuery(branchId));
        InvokeOnUI(() =>
        {
            SetColumns("باركود", "الصنف", "القسم", "الكمية", "متوسط التكلفة", "قيمة المخزون");
            _dgvReport.Rows.Clear();
            if (!result.IsSuccess || result.Value == null) return;
            decimal totalValue = 0;
            foreach (var r in result.Value)
            {
                var value = r.Quantity * r.AverageCost;
                _dgvReport.Rows.Add(r.Barcode, r.NameAr, r.CategoryName, r.Quantity, $"{r.AverageCost:N2}", $"{value:N2}");
                totalValue += value;
            }
            _lblTitle.Text   = "تقرير المخزون";
            _lblSummary.Text = $"عدد الأصناف: {result.Value.Count:N0}  |  إجمالي القيمة: {totalValue:N2} ج.م";
        });
    }

    private async Task LoadLowStockReport(int branchId)
    {
        var result = await _mediator.Send(new GetLowStockReportQuery(branchId));
        InvokeOnUI(() =>
        {
            SetColumns("باركود", "الصنف", "الكمية الحالية", "الحد الأدنى", "الفارق", "المورد الافتراضي");
            _dgvReport.Rows.Clear();
            if (!result.IsSuccess || result.Value == null) return;
            foreach (var r in result.Value)
            {
                _dgvReport.Rows.Add(r.Barcode, r.NameAr, r.CurrentQty, r.MinStock, r.CurrentQty - r.MinStock, r.DefaultSupplier);
                _dgvReport.Rows[_dgvReport.Rows.Count - 1].DefaultCellStyle.ForeColor = AppTheme.AccentRed;
            }
            _lblTitle.Text   = "الأصناف الناقصة";
            _lblSummary.Text = $"عدد الأصناف: {result.Value.Count:N0}";
        });
    }

    private async Task LoadSlowMovingReport(int branchId, DateTime from, DateTime to)
    {
        var result = await _mediator.Send(new GetSlowMovingReportQuery(branchId, from, to));
        InvokeOnUI(() =>
        {
            SetColumns("باركود", "الصنف", "الكمية المباعة", "المخزون الحالي", "آخر حركة");
            _dgvReport.Rows.Clear();
            if (!result.IsSuccess || result.Value == null) return;
            foreach (var r in result.Value)
                _dgvReport.Rows.Add(r.Barcode, r.NameAr, r.QtySold, r.CurrentStock, r.LastMovement?.ToString("dd/MM/yyyy") ?? "لا توجد");
            _lblTitle.Text   = "الأصناف الراكدة";
            _lblSummary.Text = $"عدد الأصناف: {result.Value.Count:N0}";
        });
    }

    private async Task LoadExpensesReport(int branchId, DateTime from, DateTime to)
    {
        var result = await _mediator.Send(new GetExpensesReportQuery(branchId, from, to));
        InvokeOnUI(() =>
        {
            SetColumns("التاريخ", "نوع المصروف", "المبلغ", "الوصف", "بواسطة");
            _dgvReport.Rows.Clear();
            if (!result.IsSuccess || result.Value == null) return;
            decimal total = 0;
            foreach (var r in result.Value)
            {
                _dgvReport.Rows.Add(r.Date.ToString("dd/MM/yyyy"), r.TypeAr, $"{r.Amount:N2}", r.Description, r.CreatedByName);
                total += r.Amount;
            }
            _lblTitle.Text   = "تقرير المصروفات";
            _lblSummary.Text = $"إجمالي المصروفات: {total:N2} ج.م";
        });
    }

    private async Task LoadCashboxReport(int branchId, DateTime from, DateTime to)
    {
        var result = await _mediator.Send(new GetCashboxMovementReportQuery(branchId, from, to));
        InvokeOnUI(() =>
        {
            SetColumns("التاريخ", "الخزنة", "النوع", "مدين", "دائن", "الرصيد", "ملاحظات");
            _dgvReport.Rows.Clear();
            if (!result.IsSuccess || result.Value == null) return;
            foreach (var r in result.Value)
            {
                _dgvReport.Rows.Add(
                    r.Date.ToString("dd/MM/yyyy HH:mm"), r.CashboxName, r.TypeAr,
                    r.Debit > 0 ? $"{r.Debit:N2}" : string.Empty,
                    r.Credit > 0 ? $"{r.Credit:N2}" : string.Empty,
                    $"{r.Balance:N2}", r.Notes);
            }
            _lblTitle.Text   = "حركة الخزنة";
            _lblSummary.Text = $"عدد العمليات: {result.Value.Count:N0}";
        });
    }

    private async Task LoadCashierPerfReport(int branchId, DateTime from, DateTime to)
    {
        var result = await _mediator.Send(new GetCashierPerformanceReportQuery(branchId, from, to));
        InvokeOnUI(() =>
        {
            SetColumns("الكاشير", "عدد الفواتير", "إجمالي المبيعات", "متوسط الفاتورة", "إجمالي المرتجع");
            _dgvReport.Rows.Clear();
            if (!result.IsSuccess || result.Value == null) return;
            foreach (var r in result.Value)
                _dgvReport.Rows.Add(r.CashierName, r.InvoiceCount, $"{r.TotalSales:N2}", $"{r.AvgInvoice:N2}", $"{r.TotalReturns:N2}");
            _lblTitle.Text   = "أداء الكاشيرين";
            _lblSummary.Text = $"عدد الكاشيرين: {result.Value.Count:N0}";
        });
    }

    private void SetColumns(params string[] headers)
    {
        _dgvReport.Columns.Clear();
        foreach (var h in headers)
            _dgvReport.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = h, FillWeight = 100f / headers.Length });
    }

    private void ExportReport()
    {
        // Phase 10: export to Excel
        ShowSuccess("التصدير إلى Excel سيتم دعمه في Phase 10");
    }

    private void PrintReport()
    {
        // Phase 10: FastReport print
        ShowSuccess("الطباعة ستكون متاحة في Phase 10");
    }
}

// ════════════════════════════════════════════════════════════════════
// SETTINGS FORM
// ════════════════════════════════════════════════════════════════════

namespace CorePOS.WinForms.Forms.Settings;

/// <summary>
/// System settings screen.
/// Tabs: General, Printing, Company, Users/Permissions
/// </summary>
public sealed class SettingsForm : BaseForm
{
    private TabControl _tabs = null!;

    public SettingsForm(IMediator mediator) : base(mediator)
    {
        Text      = "الإعدادات";
        BackColor = AppTheme.BgContent;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        _tabs = new TabControl { Dock = DockStyle.Fill, Font = AppTheme.FontBody };
        _tabs.TabPages.Add(BuildGeneralTab());
        _tabs.TabPages.Add(BuildPrintingTab());
        _tabs.TabPages.Add(BuildCompanyTab());
        _tabs.TabPages.Add(BuildUsersTab());
        Controls.Add(_tabs);
    }

    // ── General Settings ──────────────────────────────────────────
    private Dictionary<string, TextBox>   _settingTxt  = new();
    private Dictionary<string, ComboBox>  _settingCmb  = new();
    private Dictionary<string, CheckBox>  _settingChk  = new();

    private TabPage BuildGeneralTab()
    {
        var tab = new TabPage("عام") { BackColor = AppTheme.BgCard };
        var pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), AutoScroll = true };

        int y = 12;
        void AddSetting(string key, string label, string defaultVal)
        {
            pnl.Controls.Add(new Label { Text = label, Location = new Point(16, y), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel });
            y += 22;
            var tb = new TextBox { Location = new Point(16, y), Width = 360, Height = AppTheme.InputHeight, Font = AppTheme.FontBody, BorderStyle = BorderStyle.FixedSingle, Text = defaultVal };
            pnl.Controls.Add(tb);
            _settingTxt[key] = tb;
            y += 46;
        }

        void AddSettingCmb(string key, string label, string[] options, int defIdx = 0)
        {
            pnl.Controls.Add(new Label { Text = label, Location = new Point(16, y), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel });
            y += 22;
            var cb = new ComboBox { Location = new Point(16, y), Width = 260, DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody };
            cb.Items.AddRange(options); cb.SelectedIndex = defIdx;
            pnl.Controls.Add(cb);
            _settingCmb[key] = cb;
            y += 46;
        }

        AddSetting("Currency", "عملة النظام:", "ج.م");
        AddSetting("TaxRate",  "نسبة الضريبة الافتراضية (%):", "0");
        AddSettingCmb("PrintSize", "حجم الطباعة الافتراضي:", ["58mm", "80mm", "A5", "A4"], 1);
        AddSettingCmb("PrintAsk",  "طلب حجم الطباعة عند كل فاتورة:", ["نعم", "لا"], 1);
        AddSetting("DecimalPlaces", "خانات عشرية للكميات:", "2");
        AddSettingCmb("PriceType",  "نوع السعر الافتراضي:", ["سعر بيع", "سعر جملة", "سعر نصف جملة"], 0);

        var btnSave = new Button
        {
            Text = "💾 حفظ الإعدادات العامة", Location = new Point(16, y), Width = 220, Height = AppTheme.ButtonHeight + 4,
            FlatStyle = FlatStyle.Flat, BackColor = AppTheme.AccentGreen, ForeColor = Color.White,
            Font = AppTheme.FontBodyBold, Cursor = Cursors.Hand
        };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += (_, _) => SaveGeneralSettings();
        pnl.Controls.Add(btnSave);

        tab.Controls.Add(pnl);
        LoadSettings();
        return tab;
    }

    // ── Printing Settings ─────────────────────────────────────────
    private TextBox _txtPrinterName  = null!;
    private TextBox _txtHeaderLine1  = null!;
    private TextBox _txtHeaderLine2  = null!;
    private TextBox _txtFooterLine   = null!;
    private CheckBox _chkShowLogo    = null!;
    private CheckBox _chkAutoPrint   = null!;

    private TabPage BuildPrintingTab()
    {
        var tab = new TabPage("الطباعة") { BackColor = AppTheme.BgCard };
        var pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

        int y = 12;
        void Row(string lbl, out TextBox tb, string ph = "")
        {
            pnl.Controls.Add(new Label { Text = lbl, Location = new Point(16, y), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel });
            y += 22;
            tb = new TextBox { Location = new Point(16, y), Width = 440, Height = AppTheme.InputHeight, Font = AppTheme.FontBody, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = ph };
            pnl.Controls.Add(tb);
            y += 46;
        }

        Row("اسم الطابعة:", out _txtPrinterName, "اتركه فارغاً للطابعة الافتراضية");
        Row("سطر رأس الفاتورة 1:", out _txtHeaderLine1, "اسم المحل أو الشركة");
        Row("سطر رأس الفاتورة 2:", out _txtHeaderLine2, "العنوان / رقم الهاتف");
        Row("نص ذيل الفاتورة:", out _txtFooterLine, "شكراً لتعاملكم معنا");

        _chkShowLogo = new CheckBox { Text = "إظهار الشعار في الفاتورة", Location = new Point(16, y), AutoSize = true, Font = AppTheme.FontBody }; y += 36;
        _chkAutoPrint = new CheckBox { Text = "طباعة تلقائية بعد الحفظ", Location = new Point(16, y), AutoSize = true, Font = AppTheme.FontBody, Checked = true }; y += 52;
        pnl.Controls.AddRange([_chkShowLogo, _chkAutoPrint]);

        var btnSave = new Button
        {
            Text = "💾 حفظ إعدادات الطباعة", Location = new Point(16, y), Width = 220, Height = AppTheme.ButtonHeight + 4,
            FlatStyle = FlatStyle.Flat, BackColor = AppTheme.AccentGreen, ForeColor = Color.White,
            Font = AppTheme.FontBodyBold, Cursor = Cursors.Hand
        };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += (_, _) => SavePrintingSettings();
        pnl.Controls.Add(btnSave);

        tab.Controls.Add(pnl);
        return tab;
    }

    // ── Company Settings ──────────────────────────────────────────
    private TextBox _txtCompanyName = null!, _txtCompanyPhone = null!, _txtCompanyAddr = null!, _txtTaxNo = null!;

    private TabPage BuildCompanyTab()
    {
        var tab = new TabPage("بيانات الشركة") { BackColor = AppTheme.BgCard };
        var pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
        int y = 12;
        void Row(string lbl, out TextBox tb)
        {
            pnl.Controls.Add(new Label { Text = lbl, Location = new Point(16, y), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel });
            y += 22;
            tb = new TextBox { Location = new Point(16, y), Width = 440, Height = AppTheme.InputHeight, Font = AppTheme.FontBody, BorderStyle = BorderStyle.FixedSingle };
            pnl.Controls.Add(tb); y += 46;
        }
        Row("اسم الشركة/المحل:", out _txtCompanyName);
        Row("رقم الهاتف:", out _txtCompanyPhone);
        Row("العنوان:", out _txtCompanyAddr);
        Row("الرقم الضريبي:", out _txtTaxNo);

        var btnSave = new Button
        {
            Text = "💾 حفظ بيانات الشركة", Location = new Point(16, y), Width = 200, Height = AppTheme.ButtonHeight + 4,
            FlatStyle = FlatStyle.Flat, BackColor = AppTheme.AccentGreen, ForeColor = Color.White,
            Font = AppTheme.FontBodyBold, Cursor = Cursors.Hand
        };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += (_, _) => SaveCompanySettings();
        pnl.Controls.Add(btnSave);
        tab.Controls.Add(pnl);
        return tab;
    }

    // ── Users & Permissions Tab ───────────────────────────────────
    private DataGridView _dgvUsers  = null!;

    private TabPage BuildUsersTab()
    {
        var tab = new TabPage("المستخدمين والصلاحيات") { BackColor = AppTheme.BgContent };

        var pnlBtns = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = AppTheme.BgCard, Padding = new Padding(8) };
        var flow    = new FlowLayoutPanel { Dock = DockStyle.Left, Width = 360, FlowDirection = FlowDirection.LeftToRight, BackColor = AppTheme.BgCard, WrapContents = false };

        Button Btn(string t, Color c, EventHandler h) { var b = new Button { Text = t, Width = 120, Height = 34, FlatStyle = FlatStyle.Flat, BackColor = c, ForeColor = Color.White, Font = AppTheme.FontSmall, Cursor = Cursors.Hand, Margin = new Padding(0, 0, 6, 0) }; b.FlatAppearance.BorderSize = 0; b.Click += h; return b; }
        flow.Controls.AddRange([
            Btn("➕ مستخدم جديد", AppTheme.AccentGreen, (_, _) => AddUser()),
            Btn("✏ تعديل",         AppTheme.AccentBlue,  (_, _) => EditUser()),
            Btn("🔒 صلاحيات",      AppTheme.AccentOrange,(_, _) => EditPermissions())
        ]);
        pnlBtns.Controls.Add(flow);

        _dgvUsers = new DataGridView { Dock = DockStyle.Fill };
        AppTheme.StyleDataGrid(_dgvUsers);
        _dgvUsers.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "UserId",    Visible = false },
            new DataGridViewTextBoxColumn { Name = "Username",  HeaderText = "اسم المستخدم",  FillWeight = 20 },
            new DataGridViewTextBoxColumn { Name = "FullName",  HeaderText = "الاسم الكامل",   FillWeight = 25 },
            new DataGridViewTextBoxColumn { Name = "Role",      HeaderText = "الدور",            FillWeight = 20 },
            new DataGridViewTextBoxColumn { Name = "Branch",    HeaderText = "الفرع",            FillWeight = 20 },
            new DataGridViewTextBoxColumn { Name = "Status",    HeaderText = "الحالة",           FillWeight = 10 },
            new DataGridViewTextBoxColumn { Name = "LastLogin", HeaderText = "آخر دخول",         FillWeight = 15 }
        );

        LoadUsers();
        tab.Controls.AddRange([_dgvUsers, pnlBtns]);
        return tab;
    }

    // ── Settings Actions ──────────────────────────────────────────
    private void LoadSettings()
    {
        Task.Run(async () =>
        {
            var result = await _mediator.Send(new GetSettingsQuery());
            InvokeOnUI(() =>
            {
                if (!result.IsSuccess || result.Value == null) return;
                foreach (var kv in result.Value)
                {
                    if (_settingTxt.TryGetValue(kv.Key, out var tb)) tb.Text = kv.Value;
                    if (_settingCmb.TryGetValue(kv.Key, out var cb))
                    {
                        var idx = cb.Items.Cast<string>().ToList().IndexOf(kv.Value);
                        if (idx >= 0) cb.SelectedIndex = idx;
                    }
                }
            });
        });
    }

    private void SaveGeneralSettings()
    {
        var settings = new Dictionary<string, string>();
        foreach (var kv in _settingTxt) settings[kv.Key] = kv.Value.Text;
        foreach (var kv in _settingCmb) settings[kv.Key] = kv.Value.SelectedItem?.ToString() ?? string.Empty;
        SaveSettings(settings);
    }

    private void SavePrintingSettings()
    {
        var settings = new Dictionary<string, string>
        {
            ["PrinterName"]   = _txtPrinterName.Text.Trim(),
            ["HeaderLine1"]   = _txtHeaderLine1.Text.Trim(),
            ["HeaderLine2"]   = _txtHeaderLine2.Text.Trim(),
            ["FooterLine"]    = _txtFooterLine.Text.Trim(),
            ["ShowLogo"]      = _chkShowLogo.Checked.ToString(),
            ["AutoPrint"]     = _chkAutoPrint.Checked.ToString()
        };
        SaveSettings(settings);
    }

    private void SaveCompanySettings()
    {
        var settings = new Dictionary<string, string>
        {
            ["CompanyName"]  = _txtCompanyName.Text.Trim(),
            ["CompanyPhone"] = _txtCompanyPhone.Text.Trim(),
            ["CompanyAddr"]  = _txtCompanyAddr.Text.Trim(),
            ["TaxNo"]        = _txtTaxNo.Text.Trim()
        };
        SaveSettings(settings);
    }

    private void SaveSettings(Dictionary<string, string> settings)
    {
        RunAsync(async () =>
        {
            var result = await _mediator.Send(new SaveSettingsCommand(settings, UserSession.Current.UserId));
            InvokeOnUI(() => { if (result.IsSuccess) ShowSuccess("تم حفظ الإعدادات"); else ShowError(result.Error); });
        }, "جاري الحفظ...");
    }

    private void LoadUsers()
    {
        RunAsync(async () =>
        {
            var result = await _mediator.Send(new GetUsersListQuery());
            InvokeOnUI(() =>
            {
                _dgvUsers.Rows.Clear();
                if (!result.IsSuccess || result.Value == null) return;
                foreach (var u in result.Value)
                    _dgvUsers.Rows.Add(u.UserId, u.Username, u.FullName, u.RoleName, u.BranchName, u.IsActive ? "نشط" : "موقوف", u.LastLoginAt?.ToString("dd/MM/yyyy HH:mm") ?? "-");
            });
        });
    }

    private void AddUser()
    {
        using var dlg = new UserEditDialog(_mediator, null);
        if (dlg.ShowDialog() == DialogResult.OK) LoadUsers();
    }

    private void EditUser()
    {
        if (_dgvUsers.CurrentRow == null) return;
        var id = (int)_dgvUsers.CurrentRow.Cells["UserId"].Value;
        using var dlg = new UserEditDialog(_mediator, id);
        if (dlg.ShowDialog() == DialogResult.OK) LoadUsers();
    }

    private void EditPermissions()
    {
        if (_dgvUsers.CurrentRow == null) return;
        var id   = (int)_dgvUsers.CurrentRow.Cells["UserId"].Value;
        var name = _dgvUsers.CurrentRow.Cells["FullName"].Value?.ToString() ?? string.Empty;
        using var dlg = new UserPermissionsDialog(_mediator, id, name);
        dlg.ShowDialog();
    }
}
