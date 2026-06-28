using MediatR;
using CorePOS.WinForms.Theme;
using CorePOS.WinForms.Infrastructure;
using CorePOS.WinForms.Controls;
using CorePOS.Application.Features.Dashboard.Queries;

namespace CorePOS.WinForms.Forms.Dashboard;

/// <summary>
/// Main dashboard screen.
/// Shows: KPI cards (sales, profit, customers, invoices) + today's summary + recent invoices.
/// Auto-refreshes every 60 seconds.
/// </summary>
public sealed class DashboardForm : BaseForm
{
    private Panel _pnlCards      = null!;
    private Panel _pnlRecent     = null!;
    private DataGridView _dgvRecent = null!;
    private Label _lblLastRefresh   = null!;
    private System.Windows.Forms.Timer _refreshTimer = null!;

    // KPI card references for update
    private KpiCard _cardSales    = null!;
    private KpiCard _cardProfit   = null!;
    private KpiCard _cardInvoices = null!;
    private KpiCard _cardCustomers= null!;

    public DashboardForm(IMediator mediator) : base(mediator)
    {
        InitializeComponent();
        LoadDashboardAsync();
        StartAutoRefresh();
    }

    private void InitializeComponent()
    {
        Text      = "الرئيسية";
        BackColor = AppTheme.BgContent;
        Padding   = new Padding(24, 20, 24, 20);

        // ── Greeting bar ──────────────────────────────────────────
        var pnlGreet = new Panel
        {
            Dock   = DockStyle.Top,
            Height = 56,
            BackColor = AppTheme.BgContent
        };
        var lblGreet = new Label
        {
            Text      = $"مرحباً، {(UserSession.IsLoggedIn ? UserSession.Current.DisplayName : "المستخدم")} 👋",
            Font      = AppTheme.FontH1,
            ForeColor = AppTheme.TextPrimary,
            AutoSize  = true,
            Location  = new Point(0, 10)
        };
        var lblSubtitle = new Label
        {
            Text      = "إليك ملخص اليوم",
            Font      = AppTheme.FontBody,
            ForeColor = AppTheme.TextSecondary,
            AutoSize  = true,
            Location  = new Point(0, 36)
        };
        _lblLastRefresh = new Label
        {
            Font      = AppTheme.FontSmall,
            ForeColor = AppTheme.TextSecondary,
            AutoSize  = true,
            Anchor    = AnchorStyles.Top | AnchorStyles.Left
        };
        pnlGreet.Controls.AddRange([lblGreet, lblSubtitle, _lblLastRefresh]);

        // ── KPI Cards row ─────────────────────────────────────────
        _pnlCards = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 130,
            BackColor = AppTheme.BgContent,
            Padding   = new Padding(0, 12, 0, 12)
        };
        BuildKpiCards();

        // ── Quick Actions ─────────────────────────────────────────
        var pnlQuick = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 70,
            BackColor = AppTheme.BgContent,
            Padding   = new Padding(0, 8, 0, 0)
        };

        var lblQuickTitle = new Label
        {
            Text      = "إجراءات سريعة",
            Font      = AppTheme.FontH2,
            ForeColor = AppTheme.TextPrimary,
            AutoSize  = true,
            Dock      = DockStyle.Top
        };

        var flowQuick = new FlowLayoutPanel
        {
            Dock      = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            BackColor = AppTheme.BgContent
        };

        var quickButtons = new[]
        {
            ("🛒 فاتورة بيع جديدة", AppTheme.AccentBlue,   NavPages.POS),
            ("📦 استلام مشتريات",  AppTheme.AccentGreen,  NavPages.Purchases),
            ("💳 تحصيل عميل",      AppTheme.AccentOrange, NavPages.Customers),
            ("📊 تقرير اليوم",      AppTheme.AccentPurple, NavPages.Reports),
        };

        foreach (var (label, color, page) in quickButtons)
        {
            var btn = new Button
            {
                Text      = label,
                Width     = 160,
                Height    = 38,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = AppTheme.FontSmallBold,
                Cursor    = Cursors.Hand,
                Margin    = new Padding(0, 0, 8, 0),
                Tag       = page
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, _) =>
            {
                if (s is Button b && b.Tag is string p)
                    NavigateToPage(p);
            };
            flowQuick.Controls.Add(btn);
        }

        pnlQuick.Controls.AddRange([lblQuickTitle, flowQuick]);

        // ── Recent Invoices grid ───────────────────────────────────
        _pnlRecent = new Panel
        {
            Dock      = DockStyle.Fill,
            BackColor = AppTheme.BgCard,
            Padding   = new Padding(16)
        };

        var lblRecentTitle = new Label
        {
            Text      = "آخر الفواتير",
            Font      = AppTheme.FontH2,
            ForeColor = AppTheme.TextPrimary,
            Dock      = DockStyle.Top,
            Height    = 36
        };

        _dgvRecent = new DataGridView { Dock = DockStyle.Fill };
        AppTheme.StyleDataGrid(_dgvRecent);

        _dgvRecent.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "InvoiceNo",    HeaderText = "رقم الفاتورة",  FillWeight = 15 },
            new DataGridViewTextBoxColumn { Name = "CustomerName", HeaderText = "العميل",        FillWeight = 25 },
            new DataGridViewTextBoxColumn { Name = "InvoiceDate",  HeaderText = "التاريخ",       FillWeight = 15 },
            new DataGridViewTextBoxColumn { Name = "Total",        HeaderText = "الإجمالي",      FillWeight = 15 },
            new DataGridViewTextBoxColumn { Name = "Paid",         HeaderText = "المدفوع",       FillWeight = 15 },
            new DataGridViewTextBoxColumn { Name = "PayMethod",    HeaderText = "طريقة الدفع",  FillWeight = 15 }
        );

        _pnlRecent.Controls.AddRange([lblRecentTitle, _dgvRecent]);

        // Assemble — add in reverse order for DockStyle.Top stacking
        Controls.Add(_pnlRecent);
        Controls.Add(pnlQuick);
        Controls.Add(_pnlCards);
        Controls.Add(pnlGreet);
    }

    // ── KPI Cards ─────────────────────────────────────────────────
    private void BuildKpiCards()
    {
        _cardSales     = new KpiCard("إجمالي مبيعات اليوم", "0.00 ج.م", "📈", AppTheme.AccentBlue);
        _cardProfit    = new KpiCard("إجمالي الأرباح",       "0.00 ج.م", "💹", AppTheme.AccentGreen);
        _cardInvoices  = new KpiCard("عدد الفواتير",          "0",         "📄", AppTheme.AccentOrange);
        _cardCustomers = new KpiCard("عملاء جدد اليوم",       "0",         "👥", AppTheme.AccentPurple);

        var cards = new[] { _cardSales, _cardProfit, _cardInvoices, _cardCustomers };

        // Arrange cards in a table layout
        var tbl = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            ColumnCount = 4,
            RowCount    = 1,
            BackColor   = AppTheme.BgContent
        };
        for (int i = 0; i < 4; i++)
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));

        int col = 0;
        foreach (var card in cards)
            tbl.Controls.Add(card, col++, 0);

        _pnlCards.Controls.Add(tbl);
    }

    // ── Data Loading ──────────────────────────────────────────────
    private void LoadDashboardAsync()
    {
        RunAsync(async () =>
        {
            var query  = new GetDashboardSummaryQuery(
                UserSession.Current.BranchId,
                DateTime.Today);
            var result = await _mediator.Send(query);

            InvokeOnUI(() =>
            {
                if (result.IsSuccess && result.Value != null)
                {
                    var d = result.Value;
                    _cardSales.UpdateValue($"{d.TodaySales:N2} ج.م");
                    _cardProfit.UpdateValue($"{d.TodayProfit:N2} ج.م");
                    _cardInvoices.UpdateValue(d.TodayInvoiceCount.ToString());
                    _cardCustomers.UpdateValue(d.NewCustomersToday.ToString());

                    // Recent invoices
                    _dgvRecent.Rows.Clear();
                    foreach (var inv in d.RecentInvoices)
                    {
                        _dgvRecent.Rows.Add(
                            inv.InvoiceNo,
                            inv.CustomerName,
                            inv.InvoiceDate.ToString("dd/MM/yyyy HH:mm"),
                            $"{inv.Total:N2}",
                            $"{inv.Paid:N2}",
                            inv.PaymentMethodAr);
                    }
                }

                _lblLastRefresh.Text = $"آخر تحديث: {DateTime.Now:HH:mm:ss}";
            });
        }, "جاري تحميل البيانات...");
    }

    private void StartAutoRefresh()
    {
        _refreshTimer          = new System.Windows.Forms.Timer { Interval = 60_000 };
        _refreshTimer.Tick    += (_, _) => LoadDashboardAsync();
        _refreshTimer.Start();
    }

    private void NavigateToPage(string page)
    {
        // Find MainForm parent
        var main = FindMainForm();
        main?.NavigateTo(page);
    }

    private MainForm? FindMainForm()
    {
        var parent = Parent;
        while (parent != null)
        {
            if (parent is MainForm mf) return mf;
            parent = parent.Parent;
        }
        return null;
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);
        if (Visible) LoadDashboardAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _refreshTimer?.Stop();
        base.Dispose(disposing);
    }
}
