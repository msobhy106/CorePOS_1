using MediatR;
using CorePOS.WinForms.Theme;
using CorePOS.WinForms.Infrastructure;
using CorePOS.WinForms.Navigation;

namespace CorePOS.WinForms.Forms;

/// <summary>
/// Main shell form. Contains:
///   - Left sidebar with navigation menu
///   - Top bar with user info + shift status
///   - Content panel (swaps forms using panel embedding)
/// All child screens are embedded inside pnlContent (MDI-lite pattern).
/// </summary>
public sealed class MainForm : BaseForm
{
    // ── Layout panels ──────────────────────────────────────────────
    private Panel  _pnlSidebar  = null!;
    private Panel  _pnlTopBar   = null!;
    private Panel  _pnlContent  = null!;

    // ── Top bar controls ──────────────────────────────────────────
    private Label  _lblPageTitle   = null!;
    private Label  _lblUserName    = null!;
    private Label  _lblBranch      = null!;
    private Label  _lblShiftStatus = null!;
    private Label  _lblClock       = null!;
    private System.Windows.Forms.Timer _clockTimer = null!;

    // ── Sidebar menu items ────────────────────────────────────────
    private readonly List<SidebarItem> _menuItems = new();
    private SidebarItem? _activeItem;

    // ── Current embedded form ─────────────────────────────────────
    private Form? _currentChild;

    public MainForm(IMediator mediator) : base(mediator)
    {
        InitializeComponent();
        BuildSidebarMenu();
        StartClock();
        UpdateUserInfo();
        // Navigate to Dashboard by default
        NavigateTo(NavPages.Dashboard);
    }

    // ══════════════════════════════════════════════════════════════
    // INITIALIZE
    // ══════════════════════════════════════════════════════════════
    private void InitializeComponent()
    {
        Text            = "Core POS — نظام إدارة المبيعات";
        Size            = new Size(1280, 768);
        MinimumSize     = new Size(1024, 600);
        WindowState     = FormWindowState.Maximized;
        FormBorderStyle = FormBorderStyle.Sizable;
        BackColor       = AppTheme.BgContent;

        // ── Sidebar ────────────────────────────────────────────────
        _pnlSidebar = new Panel
        {
            Dock      = DockStyle.Left,
            Width     = AppTheme.SidebarWidth,
            BackColor = AppTheme.BgSidebar
        };

        // Logo area inside sidebar
        var pnlLogo = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 64,
            BackColor = AppTheme.BgSidebar,
            Padding   = new Padding(16, 0, 0, 0)
        };
        var lblLogo = new Label
        {
            Text      = "⚡ Core POS",
            Font      = new Font("Segoe UI", 14f, FontStyle.Bold),
            ForeColor = Color.White,
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
        pnlLogo.Controls.Add(lblLogo);

        var pnlMenuScroll = new Panel
        {
            Dock        = DockStyle.Fill,
            AutoScroll  = true,
            BackColor   = AppTheme.BgSidebar
        };
        // Items are added in BuildSidebarMenu()

        var pnlSidebarBottom = new Panel
        {
            Dock      = DockStyle.Bottom,
            Height    = 60,
            BackColor = AppTheme.BgSidebar,
            Padding   = new Padding(16, 0, 0, 0)
        };
        var btnLogout = new Button
        {
            Text      = "⇦  تسجيل الخروج",
            Dock      = DockStyle.Fill,
            FlatStyle = FlatStyle.Flat,
            BackColor = AppTheme.BgSidebar,
            ForeColor = AppTheme.TextSidebar,
            Font      = AppTheme.FontSidebar,
            TextAlign = ContentAlignment.MiddleLeft,
            Cursor    = Cursors.Hand
        };
        btnLogout.FlatAppearance.BorderSize = 0;
        btnLogout.Click += (_, _) => DoLogout();
        pnlSidebarBottom.Controls.Add(btnLogout);

        _pnlSidebar.Controls.Add(pnlMenuScroll);
        _pnlSidebar.Controls.Add(pnlSidebarBottom);
        _pnlSidebar.Controls.Add(pnlLogo);
        _pnlSidebar.Tag = pnlMenuScroll; // so BuildSidebarMenu can access it

        // ── Top bar ────────────────────────────────────────────────
        _pnlTopBar = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = AppTheme.TopBarHeight,
            BackColor = AppTheme.BgTopBar,
            Padding   = new Padding(20, 0, 20, 0)
        };

        _lblPageTitle = new Label
        {
            Text      = "الرئيسية",
            Font      = AppTheme.FontH2,
            ForeColor = AppTheme.TextPrimary,
            Dock      = DockStyle.Left,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoSize  = false,
            Width     = 300
        };

        var pnlTopRight = new Panel
        {
            Dock      = DockStyle.Right,
            Width     = 500,
            BackColor = AppTheme.BgTopBar
        };

        _lblClock = new Label
        {
            Font      = AppTheme.FontSmall,
            ForeColor = AppTheme.TextSecondary,
            AutoSize  = true,
            Location  = new Point(10, 8)
        };

        _lblBranch = new Label
        {
            Font      = AppTheme.FontSmall,
            ForeColor = AppTheme.TextSecondary,
            AutoSize  = true,
            Location  = new Point(10, 26)
        };

        _lblShiftStatus = new Label
        {
            Font      = AppTheme.FontSmallBold,
            ForeColor = AppTheme.AccentGreen,
            AutoSize  = true,
            Location  = new Point(150, 17)
        };

        _lblUserName = new Label
        {
            Font      = AppTheme.FontBodyBold,
            ForeColor = AppTheme.TextPrimary,
            AutoSize  = true,
            Location  = new Point(320, 17)
        };

        pnlTopRight.Controls.AddRange([_lblClock, _lblBranch, _lblShiftStatus, _lblUserName]);

        // Separator line at bottom of top bar
        var separator = new Panel
        {
            Dock      = DockStyle.Bottom,
            Height    = 1,
            BackColor = AppTheme.Border
        };
        _pnlTopBar.Controls.AddRange([_lblPageTitle, pnlTopRight, separator]);

        // ── Content area ───────────────────────────────────────────
        _pnlContent = new Panel
        {
            Dock      = DockStyle.Fill,
            BackColor = AppTheme.BgContent,
            Padding   = new Padding(0)
        };

        // Add to main form (order matters for Dock layout)
        Controls.Add(_pnlContent);
        Controls.Add(_pnlTopBar);
        Controls.Add(_pnlSidebar);
    }

    // ══════════════════════════════════════════════════════════════
    // SIDEBAR MENU BUILDER
    // ══════════════════════════════════════════════════════════════
    private void BuildSidebarMenu()
    {
        var menuScroll = (Panel)_pnlSidebar.Tag!;

        var items = new[]
        {
            new SidebarItemDef("🏠",  "الرئيسية",        NavPages.Dashboard,   null),
            new SidebarItemDef("🛒",  "نقطة البيع",      NavPages.POS,         Modules.Sales),
            new SidebarItemDef("📄",  "فواتير المبيعات", NavPages.SalesList,   Modules.Sales),
            new SidebarItemDef("📦",  "المشتريات",       NavPages.Purchases,   Modules.Purchases),
            new SidebarItemDef("🏪",  "المخزن",          NavPages.Inventory,   Modules.Inventory),
            new SidebarItemDef("💰",  "الخزنة",          NavPages.Finance,     Modules.Finance),
            new SidebarItemDef("👥",  "العملاء",         NavPages.Customers,   Modules.Customers),
            new SidebarItemDef("🚚",  "الموردين",        NavPages.Suppliers,   Modules.Suppliers),
            new SidebarItemDef("📊",  "التقارير",        NavPages.Reports,     Modules.Reports),
            new SidebarItemDef("📁",  "الأصناف",         NavPages.Products,    Modules.Products),
            new SidebarItemDef("👤",  "الموظفين",        NavPages.Employees,   Modules.Employees),
            new SidebarItemDef("⚙",  "الإعدادات",       NavPages.Settings,    Modules.Settings),
        };

        int y = 0;
        foreach (var def in items)
        {
            // Permission check — always show Dashboard
            if (def.Module != null && !CanView(def.Module))
                continue;

            var item = new SidebarItem(def.Icon, def.Label, def.Page)
            {
                Location = new Point(0, y),
                Width    = AppTheme.SidebarWidth
            };
            item.Click += SidebarItem_Click;
            menuScroll.Controls.Add(item);
            _menuItems.Add(item);
            y += AppTheme.SidebarItemHeight;
        }
    }

    private void SidebarItem_Click(object? sender, EventArgs e)
    {
        if (sender is SidebarItem item)
            NavigateTo(item.Page);
    }

    // ══════════════════════════════════════════════════════════════
    // NAVIGATION
    // ══════════════════════════════════════════════════════════════
    public void NavigateTo(string page, string? pageTitle = null)
    {
        // Highlight active menu item
        foreach (var item in _menuItems)
            item.SetActive(item.Page == page);

        var activeItem = _menuItems.FirstOrDefault(i => i.Page == page);
        _lblPageTitle.Text = pageTitle ?? activeItem?.Label ?? page;

        // Dispose previous child
        if (_currentChild != null)
        {
            _pnlContent.Controls.Remove(_currentChild);
            _currentChild.Dispose();
            _currentChild = null;
        }

        // Create new child form
        Form? child = page switch
        {
            NavPages.Dashboard  => Program.ServiceProvider.GetRequiredService<Dashboard.DashboardForm>(),
            NavPages.POS        => Program.ServiceProvider.GetRequiredService<POS.POSForm>(),
            NavPages.SalesList  => Program.ServiceProvider.GetRequiredService<Sales.SalesListForm>(),
            NavPages.Purchases  => Program.ServiceProvider.GetRequiredService<Purchases.PurchasesListForm>(),
            NavPages.Inventory  => Program.ServiceProvider.GetRequiredService<Inventory.InventoryForm>(),
            NavPages.Finance    => Program.ServiceProvider.GetRequiredService<Finance.FinanceForm>(),
            NavPages.Customers  => Program.ServiceProvider.GetRequiredService<MasterData.CustomersForm>(),
            NavPages.Suppliers  => Program.ServiceProvider.GetRequiredService<MasterData.SuppliersForm>(),
            NavPages.Products   => Program.ServiceProvider.GetRequiredService<MasterData.ProductsForm>(),
            NavPages.Employees  => Program.ServiceProvider.GetRequiredService<MasterData.EmployeesForm>(),
            NavPages.Reports    => Program.ServiceProvider.GetRequiredService<Reports.ReportsForm>(),
            NavPages.Settings   => Program.ServiceProvider.GetRequiredService<Settings.SettingsForm>(),
            _                   => null
        };

        if (child == null) return;

        // Embed child into content panel
        child.TopLevel        = false;
        child.FormBorderStyle = FormBorderStyle.None;
        child.Dock            = DockStyle.Fill;
        _pnlContent.Controls.Add(child);
        child.Show();
        _currentChild = child;
    }

    // ══════════════════════════════════════════════════════════════
    // CLOCK & USER INFO
    // ══════════════════════════════════════════════════════════════
    private void StartClock()
    {
        _clockTimer          = new System.Windows.Forms.Timer { Interval = 1000 };
        _clockTimer.Tick    += (_, _) => UpdateClock();
        _clockTimer.Start();
        UpdateClock();
    }

    private void UpdateClock()
    {
        var now = DateTime.Now;
        _lblClock.Text = now.ToString("HH:mm:ss");
        _lblBranch.Text = now.ToString("dddd، d MMMM yyyy", new System.Globalization.CultureInfo("ar-EG"));
    }

    private void UpdateUserInfo()
    {
        if (!UserSession.IsLoggedIn) return;
        var s = UserSession.Current;
        _lblUserName.Text    = $"👤 {s.DisplayName}  |  {s.RoleNameAr}";
        _lblShiftStatus.Text = s.HasOpenShift ? "🟢 وردية مفتوحة" : "🔴 لا توجد وردية";
    }

    public void RefreshShiftStatus() => UpdateUserInfo();

    // ══════════════════════════════════════════════════════════════
    // LOGOUT
    // ══════════════════════════════════════════════════════════════
    private void DoLogout()
    {
        if (!Confirm("هل تريد تسجيل الخروج؟")) return;

        _clockTimer.Stop();
        UserSession.ClearSession();

        var loginForm = Program.ServiceProvider.GetRequiredService<Auth.LoginForm>();
        loginForm.Show();
        Close();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _clockTimer.Stop();
        base.OnFormClosed(e);
    }
}
