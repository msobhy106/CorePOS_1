using CorePOS.WinForms.Theme;

namespace CorePOS.WinForms.Controls;

/// <summary>
/// Reusable KPI card control for dashboard.
/// Shows: icon, title, value. Updates value dynamically.
/// </summary>
public sealed class KpiCard : Panel
{
    private readonly Label _lblTitle;
    private readonly Label _lblValue;
    private readonly Label _lblIcon;
    private readonly Color _accentColor;

    public KpiCard(string title, string value, string icon, Color accentColor)
    {
        _accentColor = accentColor;

        // Panel setup
        Margin    = new Padding(0, 0, 12, 0);
        Dock      = DockStyle.Fill;
        BackColor = AppTheme.BgCard;
        Padding   = new Padding(16);
        Cursor    = Cursors.Default;

        SetStyle(ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.AllPaintingInWmPaint  |
                 ControlStyles.UserPaint, true);

        // Icon circle label
        _lblIcon = new Label
        {
            Text      = icon,
            Font      = new Font("Segoe UI Emoji", 18f),
            AutoSize  = false,
            Size      = new Size(48, 48),
            TextAlign = ContentAlignment.MiddleCenter,
            Location  = new Point(Width - 64, 16),
            Anchor    = AnchorStyles.Top | AnchorStyles.Left
        };

        // Value label (large)
        _lblValue = new Label
        {
            Text      = value,
            Font      = new Font("Segoe UI", 18f, FontStyle.Bold),
            ForeColor = AppTheme.TextPrimary,
            AutoSize  = true,
            Location  = new Point(16, 16)
        };

        // Title label (small, below value)
        _lblTitle = new Label
        {
            Text      = title,
            Font      = AppTheme.FontSmall,
            ForeColor = AppTheme.TextSecondary,
            AutoSize  = true,
            Location  = new Point(16, 52)
        };

        Controls.AddRange([_lblIcon, _lblValue, _lblTitle]);

        Resize += (_, _) => RepositionIcon();
    }

    public void UpdateValue(string newValue)
    {
        _lblValue.Text = newValue;
    }

    private void RepositionIcon()
    {
        _lblIcon.Location = new Point(Width - 64, 16);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g  = e.Graphics;
        var rc = ClientRectangle;

        // Top accent line
        using var accentPen = new Pen(_accentColor, 3f);
        g.DrawLine(accentPen, rc.Left, rc.Top, rc.Right, rc.Top);

        // Icon background circle
        var iconRect = new Rectangle(Width - 64, 12, 44, 44);
        var lightColor = Color.FromArgb(30, _accentColor.R, _accentColor.G, _accentColor.B);
        using var circleBrush = new SolidBrush(lightColor);
        g.FillEllipse(circleBrush, iconRect);
    }
}

/// <summary>
/// Badge label — shows a colored pill badge (e.g. "مفتوح", "مغلق").
/// </summary>
public sealed class BadgeLabel : Label
{
    private readonly Color _bgColor;

    public BadgeLabel(string text, Color bgColor, Color textColor)
    {
        _bgColor  = bgColor;
        Text      = text;
        ForeColor = textColor;
        Font      = AppTheme.FontSmallBold;
        AutoSize  = false;
        Height    = 22;
        TextAlign = ContentAlignment.MiddleCenter;
        Padding   = new Padding(8, 0, 8, 0);
        SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g  = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using var brush = new SolidBrush(_bgColor);
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        int radius = Height / 2;
        DrawRoundedRect(g, brush, rect, radius);
        base.OnPaint(e);
    }

    private static void DrawRoundedRect(Graphics g, Brush brush, Rectangle rect, int radius)
    {
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
        path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
        path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
        path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
        path.CloseAllFigures();
        g.FillPath(brush, path);
    }
}

/// <summary>
/// Search TextBox with placeholder + magnifier icon.
/// </summary>
public sealed class SearchBox : Panel
{
    public TextBox TextBox { get; }

    public event EventHandler<string>? SearchChanged;

    public SearchBox(string placeholder = "بحث...")
    {
        Height    = AppTheme.InputHeight + 4;
        BackColor = AppTheme.BgInput;
        BorderStyle = BorderStyle.FixedSingle;

        var iconLbl = new Label
        {
            Text      = "🔍",
            Font      = new Font("Segoe UI Emoji", 11f),
            AutoSize  = false,
            Width     = 32,
            Dock      = DockStyle.Right,
            TextAlign = ContentAlignment.MiddleCenter
        };

        TextBox = new TextBox
        {
            Dock            = DockStyle.Fill,
            BorderStyle     = BorderStyle.None,
            Font            = AppTheme.FontBody,
            BackColor       = AppTheme.BgInput,
            PlaceholderText = placeholder
        };
        TextBox.TextChanged += (_, _) => SearchChanged?.Invoke(this, TextBox.Text);

        Controls.AddRange([TextBox, iconLbl]);
    }

    public string Text
    {
        get => TextBox.Text;
        set => TextBox.Text = value;
    }
}

/// <summary>
/// Reusable toolbar panel with action buttons.
/// </summary>
public sealed class ActionToolbar : Panel
{
    private readonly FlowLayoutPanel _flow;

    public ActionToolbar()
    {
        Dock      = DockStyle.Top;
        Height    = 52;
        BackColor = AppTheme.BgCard;
        Padding   = new Padding(8, 8, 8, 8);
        BorderStyle = BorderStyle.None;

        _flow = new FlowLayoutPanel
        {
            Dock          = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            BackColor     = AppTheme.BgCard,
            WrapContents  = false
        };
        Controls.Add(_flow);
    }

    public Button AddButton(string text, Color backColor, EventHandler onClick)
    {
        var btn = new Button
        {
            Text      = text,
            Height    = AppTheme.ButtonHeight,
            AutoSize  = true,
            BackColor = backColor,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font      = AppTheme.FontSmallBold,
            Cursor    = Cursors.Hand,
            Padding   = new Padding(12, 0, 12, 0),
            Margin    = new Padding(0, 0, 6, 0)
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.Click += onClick;
        _flow.Controls.Add(btn);
        return btn;
    }
}
