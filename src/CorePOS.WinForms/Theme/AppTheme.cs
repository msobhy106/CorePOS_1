using System.Drawing;
using System.Windows.Forms;

namespace CorePOS.WinForms.Theme;

/// <summary>
/// Central theme definition for Core POS.
/// Modern dark sidebar + light content area. Full RTL Arabic support.
/// </summary>
public static class AppTheme
{
    // ══════════════════════════════════════════════════════════════
    // COLOR PALETTE
    // ══════════════════════════════════════════════════════════════

    // Background colors
    public static readonly Color BgSidebar      = Color.FromArgb(22,  27,  46);   // Dark navy
    public static readonly Color BgSidebarHover = Color.FromArgb(35,  43,  70);   // Slightly lighter
    public static readonly Color BgSidebarActive= Color.FromArgb(67, 106, 215);   // Blue accent
    public static readonly Color BgContent      = Color.FromArgb(245, 247, 252);  // Light grey
    public static readonly Color BgCard         = Color.White;
    public static readonly Color BgTopBar       = Color.White;
    public static readonly Color BgInput        = Color.White;
    public static readonly Color BgInputFocus   = Color.FromArgb(235, 242, 255);

    // Text colors
    public static readonly Color TextPrimary    = Color.FromArgb(30,  34,  50);
    public static readonly Color TextSecondary  = Color.FromArgb(107, 114, 142);
    public static readonly Color TextSidebar    = Color.FromArgb(180, 190, 220);
    public static readonly Color TextSidebarAct = Color.White;
    public static readonly Color TextWhite      = Color.White;
    public static readonly Color TextLabel      = Color.FromArgb(80,  88, 120);

    // Accent / action colors
    public static readonly Color AccentBlue     = Color.FromArgb(67,  106, 215);
    public static readonly Color AccentGreen    = Color.FromArgb(34,  197, 94);
    public static readonly Color AccentRed      = Color.FromArgb(239, 68,  68);
    public static readonly Color AccentOrange   = Color.FromArgb(249, 115, 22);
    public static readonly Color AccentYellow   = Color.FromArgb(234, 179, 8);
    public static readonly Color AccentPurple   = Color.FromArgb(139, 92,  246);

    // Border / separator
    public static readonly Color Border         = Color.FromArgb(226, 232, 240);
    public static readonly Color BorderFocus    = Color.FromArgb(67,  106, 215);

    // Status badge backgrounds
    public static readonly Color BadgeSuccess   = Color.FromArgb(220, 252, 231);
    public static readonly Color BadgeError     = Color.FromArgb(254, 226, 226);
    public static readonly Color BadgeWarning   = Color.FromArgb(254, 243, 199);
    public static readonly Color BadgeInfo      = Color.FromArgb(219, 234, 254);

    // ══════════════════════════════════════════════════════════════
    // FONTS (Arabic-capable)
    // ══════════════════════════════════════════════════════════════
    public static readonly Font FontTitle       = new("Segoe UI", 18f, FontStyle.Bold);
    public static readonly Font FontH1          = new("Segoe UI", 14f, FontStyle.Bold);
    public static readonly Font FontH2          = new("Segoe UI", 12f, FontStyle.Bold);
    public static readonly Font FontBody        = new("Segoe UI", 10f);
    public static readonly Font FontBodyBold    = new("Segoe UI", 10f, FontStyle.Bold);
    public static readonly Font FontSmall       = new("Segoe UI", 9f);
    public static readonly Font FontSmallBold   = new("Segoe UI", 9f, FontStyle.Bold);
    public static readonly Font FontMono        = new("Consolas",  10f);
    public static readonly Font FontArabic      = new("Tahoma",   11f);
    public static readonly Font FontArabicBold  = new("Tahoma",   11f, FontStyle.Bold);
    public static readonly Font FontPOS         = new("Segoe UI", 13f, FontStyle.Bold); // POS screen
    public static readonly Font FontPOSLarge    = new("Segoe UI", 20f, FontStyle.Bold); // totals
    public static readonly Font FontSidebar     = new("Segoe UI", 10f);
    public static readonly Font FontSidebarIcon = new("Segoe UI", 14f);

    // ══════════════════════════════════════════════════════════════
    // SIZES
    // ══════════════════════════════════════════════════════════════
    public const int SidebarWidth       = 220;
    public const int TopBarHeight       = 56;
    public const int SidebarItemHeight  = 44;
    public const int CardPadding        = 20;
    public const int CornerRadius       = 8;
    public const int ButtonHeight       = 36;
    public const int InputHeight        = 36;

    // ══════════════════════════════════════════════════════════════
    // APPLY RTL TO FORM
    // ══════════════════════════════════════════════════════════════
    public static void ApplyRtl(Form form)
    {
        form.RightToLeft        = RightToLeft.Yes;
        form.RightToLeftLayout  = true;
    }

    // ══════════════════════════════════════════════════════════════
    // STYLE HELPERS
    // ══════════════════════════════════════════════════════════════
    public static void StylePrimaryButton(Button btn)
    {
        btn.BackColor   = AccentBlue;
        btn.ForeColor   = TextWhite;
        btn.FlatStyle   = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.Font        = FontBodyBold;
        btn.Height      = ButtonHeight;
        btn.Cursor      = Cursors.Hand;
    }

    public static void StyleDangerButton(Button btn)
    {
        btn.BackColor   = AccentRed;
        btn.ForeColor   = TextWhite;
        btn.FlatStyle   = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.Font        = FontBodyBold;
        btn.Height      = ButtonHeight;
        btn.Cursor      = Cursors.Hand;
    }

    public static void StyleSuccessButton(Button btn)
    {
        btn.BackColor   = AccentGreen;
        btn.ForeColor   = TextWhite;
        btn.FlatStyle   = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.Font        = FontBodyBold;
        btn.Height      = ButtonHeight;
        btn.Cursor      = Cursors.Hand;
    }

    public static void StyleSecondaryButton(Button btn)
    {
        btn.BackColor   = Color.White;
        btn.ForeColor   = AccentBlue;
        btn.FlatStyle   = FlatStyle.Flat;
        btn.FlatAppearance.BorderColor = AccentBlue;
        btn.FlatAppearance.BorderSize  = 1;
        btn.Font        = FontBodyBold;
        btn.Height      = ButtonHeight;
        btn.Cursor      = Cursors.Hand;
    }

    public static void StyleTextBox(TextBox tb)
    {
        tb.BackColor    = BgInput;
        tb.ForeColor    = TextPrimary;
        tb.BorderStyle  = BorderStyle.FixedSingle;
        tb.Font         = FontBody;
        tb.Height       = InputHeight;
    }

    public static void StyleDataGrid(DataGridView dgv)
    {
        dgv.BackgroundColor             = BgCard;
        dgv.BorderStyle                 = BorderStyle.None;
        dgv.CellBorderStyle             = DataGridViewCellBorderStyle.SingleHorizontal;
        dgv.GridColor                   = Border;
        dgv.RowHeadersVisible           = false;
        dgv.SelectionMode               = DataGridViewSelectionMode.FullRowSelect;
        dgv.MultiSelect                 = false;
        dgv.ReadOnly                    = true;
        dgv.AllowUserToAddRows          = false;
        dgv.AllowUserToDeleteRows       = false;
        dgv.AllowUserToResizeRows       = false;
        dgv.AutoSizeColumnsMode         = DataGridViewAutoSizeColumnsMode.Fill;
        dgv.Font                        = FontBody;
        dgv.RowTemplate.Height          = 40;
        dgv.EnableHeadersVisualStyles   = false;

        // Header style
        dgv.ColumnHeadersDefaultCellStyle.BackColor   = BgContent;
        dgv.ColumnHeadersDefaultCellStyle.ForeColor   = TextSecondary;
        dgv.ColumnHeadersDefaultCellStyle.Font        = FontSmallBold;
        dgv.ColumnHeadersDefaultCellStyle.Alignment   = DataGridViewContentAlignment.MiddleCenter;
        dgv.ColumnHeadersHeight                       = 44;
        dgv.ColumnHeadersBorderStyle                  = DataGridViewHeaderBorderStyle.None;

        // Row style
        dgv.DefaultCellStyle.BackColor      = Color.White;
        dgv.DefaultCellStyle.ForeColor      = TextPrimary;
        dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
        dgv.DefaultCellStyle.SelectionForeColor = TextPrimary;
        dgv.DefaultCellStyle.Alignment      = DataGridViewContentAlignment.MiddleCenter;

        // Alternate row
        dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 252, 255);
    }
}
