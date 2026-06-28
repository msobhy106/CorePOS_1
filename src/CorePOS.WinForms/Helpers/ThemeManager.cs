using System.Drawing;
using System.Windows.Forms;

namespace CorePOS.WinForms.Helpers;

/// <summary>Centralized dark theme colors and styles for all forms.</summary>
public static class ThemeManager
{
    // ── Color Palette ─────────────────────────────────────
    public static readonly Color BgDark       = Color.FromArgb(13,  17,  28);
    public static readonly Color BgMedium     = Color.FromArgb(18,  24,  40);
    public static readonly Color BgLight      = Color.FromArgb(22,  30,  50);
    public static readonly Color BgInput      = Color.FromArgb(22,  30,  48);
    public static readonly Color AccentBlue   = Color.FromArgb(49,  130, 206);
    public static readonly Color AccentGreen  = Color.FromArgb(56,  161, 105);
    public static readonly Color AccentRed    = Color.FromArgb(229, 62,  62);
    public static readonly Color AccentOrange = Color.FromArgb(237, 137, 54);
    public static readonly Color AccentPurple = Color.FromArgb(159, 122, 234);
    public static readonly Color TextPrimary  = Color.FromArgb(237, 242, 247);
    public static readonly Color TextSecondary= Color.FromArgb(160, 174, 192);
    public static readonly Color TextMuted    = Color.FromArgb(100, 116, 139);
    public static readonly Color BorderColor  = Color.FromArgb(30,  40,  60);
    public static readonly Color GridHeader   = Color.FromArgb(22,  30,  50);
    public static readonly Color GridRow      = Color.FromArgb(18,  24,  40);
    public static readonly Color GridAltRow   = Color.FromArgb(22,  30,  46);
    public static readonly Color GridSelect   = Color.FromArgb(49,  130, 206);

    // ── Fonts ─────────────────────────────────────────────
    public static readonly Font FontDefault   = new("Segoe UI", 10f);
    public static readonly Font FontSmall     = new("Segoe UI", 9f);
    public static readonly Font FontLarge     = new("Segoe UI", 12f);
    public static readonly Font FontTitle     = new("Segoe UI", 14f, FontStyle.Bold);
    public static readonly Font FontBold      = new("Segoe UI", 10f, FontStyle.Bold);
    public static readonly Font FontMono      = new("Consolas",  10f);

    // ── Apply theme to form ───────────────────────────────
    public static void ApplyToForm(Form form)
    {
        form.BackColor         = BgDark;
        form.ForeColor         = TextPrimary;
        form.Font              = FontDefault;
        form.RightToLeft       = RightToLeft.Yes;
        form.RightToLeftLayout = true;
        ApplyToControls(form.Controls);
    }

    public static void ApplyToControls(Control.ControlCollection controls)
    {
        foreach (Control ctrl in controls)
        {
            switch (ctrl)
            {
                case DataGridView dgv:
                    StyleGrid(dgv);
                    break;
                case TextBox txt:
                    txt.BackColor   = BgInput;
                    txt.ForeColor   = TextPrimary;
                    txt.BorderStyle = BorderStyle.FixedSingle;
                    break;
                case ComboBox cbo:
                    cbo.BackColor   = BgInput;
                    cbo.ForeColor   = TextPrimary;
                    cbo.FlatStyle   = FlatStyle.Flat;
                    break;
                case Button btn:
                    if (btn.BackColor == SystemColors.Control)
                    {
                        btn.BackColor  = AccentBlue;
                        btn.ForeColor  = Color.White;
                        btn.FlatStyle  = FlatStyle.Flat;
                        btn.FlatAppearance.BorderSize = 0;
                        btn.Cursor     = Cursors.Hand;
                    }
                    break;
                case Panel pnl:
                    if (pnl.BackColor == SystemColors.Control)
                        pnl.BackColor = BgMedium;
                    ApplyToControls(pnl.Controls);
                    break;
                case Label lbl:
                    lbl.ForeColor = TextSecondary;
                    break;
                case GroupBox grp:
                    grp.ForeColor = TextSecondary;
                    ApplyToControls(grp.Controls);
                    break;
                case TabControl tab:
                    tab.BackColor = BgMedium;
                    foreach (TabPage page in tab.TabPages)
                    {
                        page.BackColor = BgMedium;
                        ApplyToControls(page.Controls);
                    }
                    break;
            }

            if (ctrl.Controls.Count > 0 && ctrl is not Panel and not GroupBox)
                ApplyToControls(ctrl.Controls);
        }
    }

    // ── DataGridView styling ──────────────────────────────
    public static void StyleGrid(DataGridView dgv)
    {
        dgv.BackgroundColor        = GridRow;
        dgv.GridColor              = BorderColor;
        dgv.BorderStyle            = BorderStyle.None;
        dgv.RowHeadersVisible      = false;
        dgv.AllowUserToAddRows     = false;
        dgv.AllowUserToDeleteRows  = false;
        dgv.ReadOnly               = true;
        dgv.SelectionMode          = DataGridViewSelectionMode.FullRowSelect;
        dgv.MultiSelect            = false;
        dgv.ColumnHeadersHeight    = 38;
        dgv.RowTemplate.Height     = 34;
        dgv.AutoSizeColumnsMode    = DataGridViewAutoSizeColumnsMode.Fill;
        dgv.RightToLeft            = RightToLeft.Yes;
        dgv.EnableHeadersVisualStyles = false;

        dgv.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor          = GridRow,
            ForeColor          = TextPrimary,
            SelectionBackColor = GridSelect,
            SelectionForeColor = Color.White,
            Alignment          = DataGridViewContentAlignment.MiddleRight,
            Padding            = new Padding(4, 0, 4, 0),
            Font               = FontSmall
        };

        dgv.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = GridHeader,
            ForeColor = AccentBlue,
            Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Alignment = DataGridViewContentAlignment.MiddleRight,
            Padding   = new Padding(4, 0, 4, 0)
        };

        dgv.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor          = GridAltRow,
            ForeColor          = TextPrimary,
            SelectionBackColor = GridSelect,
            SelectionForeColor = Color.White
        };
    }

    // ── Button styles ─────────────────────────────────────
    public static Button MakePrimaryBtn(string text, int width = 120, int height = 36)
        => MakeBtn(text, AccentBlue, width, height);

    public static Button MakeSuccessBtn(string text, int width = 120, int height = 36)
        => MakeBtn(text, AccentGreen, width, height);

    public static Button MakeDangerBtn(string text, int width = 120, int height = 36)
        => MakeBtn(text, AccentRed, width, height);

    public static Button MakeWarningBtn(string text, int width = 120, int height = 36)
        => MakeBtn(text, AccentOrange, width, height);

    private static Button MakeBtn(string text, Color bg, int w, int h) => new()
    {
        Text      = text,
        Size      = new Size(w, h),
        BackColor = bg,
        ForeColor = Color.White,
        FlatStyle = FlatStyle.Flat,
        Font      = FontBold,
        Cursor    = Cursors.Hand,
        FlatAppearance = { BorderSize = 0 }
    };

    // ── Label helpers ─────────────────────────────────────
    public static Label MakeLabel(string text, Color? color = null) => new()
    {
        Text      = text,
        AutoSize  = true,
        ForeColor = color ?? TextSecondary,
        Font      = FontDefault
    };

    public static Label MakeTitleLabel(string text) => new()
    {
        Text      = text,
        AutoSize  = true,
        ForeColor = TextPrimary,
        Font      = FontTitle
    };

    // ── TextBox helper ────────────────────────────────────
    public static TextBox MakeTextBox(int width = 220, bool readOnly = false) => new()
    {
        Width       = width,
        BackColor   = readOnly ? BgLight : BgInput,
        ForeColor   = TextPrimary,
        BorderStyle = BorderStyle.FixedSingle,
        Font        = FontDefault,
        ReadOnly    = readOnly
    };

    // ── ComboBox helper ───────────────────────────────────
    public static ComboBox MakeComboBox(int width = 220) => new()
    {
        Width         = width,
        BackColor     = BgInput,
        ForeColor     = TextPrimary,
        FlatStyle     = FlatStyle.Flat,
        DropDownStyle = ComboBoxStyle.DropDownList,
        Font          = FontDefault
    };

    // ── Panel helpers ─────────────────────────────────────
    public static Panel MakeToolbar(int height = 50) => new()
    {
        Height    = height,
        Dock      = DockStyle.Top,
        BackColor = BgMedium,
        Padding   = new Padding(8, 6, 8, 6)
    };

    public static Panel MakeStatusBar() => new()
    {
        Height    = 28,
        Dock      = DockStyle.Bottom,
        BackColor = BgLight,
        Padding   = new Padding(8, 4, 8, 4)
    };
}
