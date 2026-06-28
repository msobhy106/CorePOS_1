using MediatR;
using CorePOS.WinForms.Theme;
using CorePOS.WinForms.Infrastructure;
using CorePOS.Application.Features.Auth.Commands;

namespace CorePOS.WinForms.Forms.Auth;

/// <summary>
/// Login screen — first form shown on startup.
/// Clean card-based design. RTL Arabic. Keyboard-friendly (Enter to login).
/// </summary>
public sealed class LoginForm : BaseForm
{
    // ── Controls ──────────────────────────────────────────────────
    private TextBox  _txtUsername  = null!;
    private TextBox  _txtPassword  = null!;
    private Button   _btnLogin     = null!;
    private Label    _lblError     = null!;
    private CheckBox _chkShowPass  = null!;
    private Label    _lblVersion   = null!;

    public LoginForm(IMediator mediator) : base(mediator)
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        // ── Form setup ────────────────────────────────────────────
        Text            = "تسجيل الدخول — Core POS";
        Size            = new Size(1000, 600);
        MinimumSize     = new Size(800, 500);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox     = false;
        BackColor       = AppTheme.BgSidebar;
        StartPosition   = FormStartPosition.CenterScreen;

        // ── Left panel (branding) ─────────────────────────────────
        var leftPanel = new Panel
        {
            Dock      = DockStyle.Left,
            Width     = 420,
            BackColor = AppTheme.BgSidebar
        };

        var lblLogo = new Label
        {
            Text      = "Core POS",
            Font      = new Font("Segoe UI", 36f, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize  = true,
            Location  = new Point(60, 160)
        };

        var lblTagline = new Label
        {
            Text      = "نظام إدارة المبيعات الاحترافي",
            Font      = AppTheme.FontArabic,
            ForeColor = AppTheme.TextSidebar,
            AutoSize  = true,
            Location  = new Point(60, 230)
        };

        var lblFeatures = new Label
        {
            Text      = "✓ متعدد الفروع والمستودعات\n✓ تقارير شاملة\n✓ يعمل بدون إنترنت\n✓ دعم الطابعات الحرارية",
            Font      = AppTheme.FontBody,
            ForeColor = AppTheme.TextSidebar,
            AutoSize  = true,
            Location  = new Point(60, 300)
        };

        leftPanel.Controls.AddRange([lblLogo, lblTagline, lblFeatures]);

        // ── Right panel (login card) ───────────────────────────────
        var rightPanel = new Panel
        {
            Dock      = DockStyle.Fill,
            BackColor = AppTheme.BgContent,
            Padding   = new Padding(60, 0, 60, 0)
        };

        var card = new Panel
        {
            Width     = 340,
            Height    = 380,
            BackColor = AppTheme.BgCard,
            Anchor    = AnchorStyles.None
        };

        // Center card
        rightPanel.Resize += (_, _) =>
        {
            card.Location = new Point(
                (rightPanel.Width  - card.Width)  / 2,
                (rightPanel.Height - card.Height) / 2);
        };

        // Card contents
        var lblTitle = new Label
        {
            Text      = "تسجيل الدخول",
            Font      = AppTheme.FontH1,
            ForeColor = AppTheme.TextPrimary,
            AutoSize  = true,
            Location  = new Point(24, 28)
        };

        var lblUserHint = new Label
        {
            Text      = "اسم المستخدم",
            Font      = AppTheme.FontSmall,
            ForeColor = AppTheme.TextLabel,
            AutoSize  = true,
            Location  = new Point(24, 80)
        };

        _txtUsername = new TextBox
        {
            Location    = new Point(24, 100),
            Width       = 292,
            Height      = AppTheme.InputHeight,
            Font        = AppTheme.FontBody,
            BorderStyle = BorderStyle.FixedSingle,
            PlaceholderText = "أدخل اسم المستخدم"
        };

        var lblPassHint = new Label
        {
            Text      = "كلمة المرور",
            Font      = AppTheme.FontSmall,
            ForeColor = AppTheme.TextLabel,
            AutoSize  = true,
            Location  = new Point(24, 150)
        };

        _txtPassword = new TextBox
        {
            Location        = new Point(24, 170),
            Width           = 292,
            Height          = AppTheme.InputHeight,
            Font            = AppTheme.FontBody,
            BorderStyle     = BorderStyle.FixedSingle,
            PasswordChar    = '●',
            PlaceholderText = "أدخل كلمة المرور"
        };
        _txtPassword.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) DoLogin(); };

        _chkShowPass = new CheckBox
        {
            Text      = "إظهار كلمة المرور",
            Location  = new Point(24, 212),
            AutoSize  = true,
            Font      = AppTheme.FontSmall,
            ForeColor = AppTheme.TextSecondary
        };
        _chkShowPass.CheckedChanged += (_, _) =>
            _txtPassword.PasswordChar = _chkShowPass.Checked ? '\0' : '●';

        _lblError = new Label
        {
            Text      = string.Empty,
            ForeColor = AppTheme.AccentRed,
            Font      = AppTheme.FontSmall,
            AutoSize  = true,
            Location  = new Point(24, 240),
            MaximumSize = new Size(292, 0)
        };

        _btnLogin = new Button
        {
            Text      = "دخول",
            Location  = new Point(24, 300),
            Width     = 292,
            Height    = 44,
            FlatStyle = FlatStyle.Flat,
            BackColor = AppTheme.AccentBlue,
            ForeColor = Color.White,
            Font      = new Font("Segoe UI", 12f, FontStyle.Bold),
            Cursor    = Cursors.Hand
        };
        _btnLogin.FlatAppearance.BorderSize = 0;
        _btnLogin.Click += (_, _) => DoLogin();

        card.Controls.AddRange([
            lblTitle, lblUserHint, _txtUsername,
            lblPassHint, _txtPassword, _chkShowPass,
            _lblError, _btnLogin
        ]);

        // Drop shadow effect via border panel
        var shadow = new Panel
        {
            BackColor = AppTheme.Border,
            Location  = new Point(2, 2)
        };

        rightPanel.Controls.Add(card);
        rightPanel.Controls.Add(shadow);

        // Version label
        _lblVersion = new Label
        {
            Text      = "Core POS v1.0 — Core Tech",
            Font      = AppTheme.FontSmall,
            ForeColor = AppTheme.TextSecondary,
            AutoSize  = true,
            Anchor    = AnchorStyles.Bottom | AnchorStyles.Left,
            Location  = new Point(10, rightPanel.Height - 30)
        };
        rightPanel.Controls.Add(_lblVersion);
        rightPanel.Resize += (_, _) =>
            _lblVersion.Location = new Point(10, rightPanel.Height - 30);

        Controls.AddRange([leftPanel, rightPanel]);

        // Focus username
        Load += (_, _) => _txtUsername.Focus();
    }

    // ── Login Logic ───────────────────────────────────────────────
    private void DoLogin()
    {
        var username = _txtUsername.Text.Trim();
        var password = _txtPassword.Text;

        if (string.IsNullOrEmpty(username))
        {
            ShowFieldError("يرجى إدخال اسم المستخدم");
            _txtUsername.Focus();
            return;
        }
        if (string.IsNullOrEmpty(password))
        {
            ShowFieldError("يرجى إدخال كلمة المرور");
            _txtPassword.Focus();
            return;
        }

        _btnLogin.Enabled = false;
        _lblError.Text    = string.Empty;
        ShowLoading("جاري التحقق...");

        Task.Run(async () =>
        {
            try
            {
                var cmd    = new LoginCommand(username, password);
                var result = await _mediator.Send(cmd);

                InvokeOnUI(() =>
                {
                    HideLoading();
                    _btnLogin.Enabled = true;

                    if (result.IsSuccess)
                    {
                        // Session is already set inside LoginCommandHandler
                        var mainForm = Program.ServiceProvider.GetRequiredService<MainForm>();
                        mainForm.Show();
                        Hide();
                    }
                    else
                    {
                        ShowFieldError(result.Error);
                        _txtPassword.Clear();
                        _txtPassword.Focus();
                    }
                });
            }
            catch (Exception ex)
            {
                InvokeOnUI(() =>
                {
                    HideLoading();
                    _btnLogin.Enabled = true;
                    ShowFieldError("حدث خطأ: " + ex.Message);
                });
            }
        });
    }

    private void ShowFieldError(string msg)
    {
        _lblError.Text = msg;
    }
}
