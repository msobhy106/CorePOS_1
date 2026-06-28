using CorePOS.WinForms.Theme;

namespace CorePOS.WinForms.Navigation;

// ── Page name constants ────────────────────────────────────────────
public static class NavPages
{
    public const string Dashboard  = "Dashboard";
    public const string POS        = "POS";
    public const string SalesList  = "SalesList";
    public const string Purchases  = "Purchases";
    public const string Inventory  = "Inventory";
    public const string Finance    = "Finance";
    public const string Customers  = "Customers";
    public const string Suppliers  = "Suppliers";
    public const string Products   = "Products";
    public const string Employees  = "Employees";
    public const string Reports    = "Reports";
    public const string Settings   = "Settings";
}

// ── Sidebar menu item definition ──────────────────────────────────
public record SidebarItemDef(string Icon, string Label, string Page, string? Module);

// ── Sidebar item control (custom-drawn button) ────────────────────
public sealed class SidebarItem : Control
{
    public string Icon  { get; }
    public string Label { get; }
    public string Page  { get; }

    private bool _isActive;
    private bool _isHovered;

    public SidebarItem(string icon, string label, string page)
    {
        Icon   = icon;
        Label  = label;
        Page   = page;
        Height = AppTheme.SidebarItemHeight;
        Cursor = Cursors.Hand;
        SetStyle(ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.AllPaintingInWmPaint  |
                 ControlStyles.UserPaint, true);
    }

    public void SetActive(bool active)
    {
        _isActive = active;
        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e) { _isHovered = true;  Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _isHovered = false; Invalidate(); base.OnMouseLeave(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g  = e.Graphics;
        var rc = ClientRectangle;

        // Background
        var bg = _isActive  ? AppTheme.BgSidebarActive
               : _isHovered ? AppTheme.BgSidebarHover
               :               AppTheme.BgSidebar;
        g.Clear(bg);

        // Active indicator strip (left side → right side for RTL)
        if (_isActive)
        {
            using var accentBrush = new SolidBrush(Color.White);
            g.FillRectangle(accentBrush, rc.Right - 3, 8, 3, rc.Height - 16);
        }

        // Icon
        var iconColor = _isActive ? AppTheme.TextSidebarAct : AppTheme.TextSidebar;
        using var iconBrush = new SolidBrush(iconColor);
        using var iconFont  = new Font("Segoe UI Emoji", 13f);
        var iconSize = g.MeasureString(Icon, iconFont);
        g.DrawString(Icon, iconFont, iconBrush,
            rc.Width - 36 - (int)iconSize.Width, (rc.Height - (int)iconSize.Height) / 2);

        // Label (RTL: text to the right)
        var textColor = _isActive ? AppTheme.TextSidebarAct : AppTheme.TextSidebar;
        using var textBrush = new SolidBrush(textColor);
        using var textFont  = _isActive ? AppTheme.FontBodyBold : AppTheme.FontSidebar;
        var sf = new StringFormat(StringFormatFlags.DirectionRightToLeft)
        {
            Alignment     = StringAlignment.Far,
            LineAlignment = StringAlignment.Center
        };
        g.DrawString(Label, textFont, textBrush,
            new RectangleF(50, 0, rc.Width - 60, rc.Height), sf);
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        OnClick(e);
    }
}
