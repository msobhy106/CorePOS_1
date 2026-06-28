using MediatR;
using CorePOS.WinForms.Theme;
using CorePOS.Application.Interfaces;

namespace CorePOS.WinForms.Forms.License;

// ════════════════════════════════════════════════════════════════════
// LICENSE FORM — View + Activate
// ════════════════════════════════════════════════════════════════════
/// <summary>
/// License information + activation screen.
/// Shows: current status, machine ID, expiry.
/// Allows: entering activation code.
/// </summary>
public sealed class LicenseForm : BaseForm
{
    private readonly ILicenseService _licenseService;

    // Info panel controls
    private Label  _lblStatus        = null!;
    private Label  _lblType          = null!;
    private Label  _lblExpiry        = null!;
    private Label  _lblLicensedTo    = null!;
    private Label  _lblMachineId     = null!;
    private Label  _lblDaysRemaining = null!;
    private Panel  _pnlStatusBadge   = null!;

    // Activation controls
    private TextBox _txtCode         = null!;
    private Button  _btnActivate     = null!;
    private Label   _lblActivResult  = null!;

    public LicenseForm(IMediator mediator, ILicenseService licenseService) : base(mediator)
    {
        _licenseService = licenseService;
        Text            = "الترخيص وتفعيل البرنامج";
        BackColor       = AppTheme.BgContent;
        Size            = new Size(680, 560);
        MinimumSize     = new Size(600, 500);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        StartPosition   = FormStartPosition.CenterParent;
        InitializeComponent();
        LoadLicenseInfo();
    }

    private void InitializeComponent()
    {
        // ── Header ────────────────────────────────────────────────
        var pnlHeader = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 80,
            BackColor = AppTheme.BgSidebar,
            Padding   = new Padding(24, 16, 24, 0)
        };
        pnlHeader.Controls.Add(new Label
        {
            Text      = "🔑 معلومات الترخيص",
            Font      = new Font("Segoe UI", 18f, FontStyle.Bold),
            ForeColor = Color.White,
            Dock      = DockStyle.Top,
            Height    = 44
        });
        pnlHeader.Controls.Add(new Label
        {
            Text      = "Core POS — Core Tech",
            Font      = AppTheme.FontBody,
            ForeColor = AppTheme.TextSidebar,
            Dock      = DockStyle.Top,
            Height    = 22
        });

        // ── Status card ───────────────────────────────────────────
        var pnlCard = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 220,
            BackColor = AppTheme.BgCard,
            Padding   = new Padding(24, 16, 24, 16)
        };

        // Status badge
        _pnlStatusBadge = new Panel
        {
            Location  = new Point(24, 16),
            Size      = new Size(200, 36),
            BackColor = AppTheme.AccentBlue
        };
        _lblStatus = new Label
        {
            Dock      = DockStyle.Fill,
            Font      = AppTheme.FontBodyBold,
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleCenter
        };
        _pnlStatusBadge.Controls.Add(_lblStatus);
        pnlCard.Controls.Add(_pnlStatusBadge);

        // Info grid
        int y = 60;
        void InfoRow(string label, out Label valueLabel, int yy)
        {
            pnlCard.Controls.Add(new Label
            {
                Text      = label,
                Location  = new Point(280, yy),
                AutoSize  = true,
                Font      = AppTheme.FontSmall,
                ForeColor = AppTheme.TextLabel
            });
            valueLabel = new Label
            {
                Location  = new Point(24, yy),
                AutoSize  = true,
                Font      = AppTheme.FontBodyBold,
                ForeColor = AppTheme.TextPrimary
            };
            pnlCard.Controls.Add(valueLabel);
        }

        InfoRow("نوع الترخيص:",     out _lblType,         y);  y += 26;
        InfoRow("صالح حتى:",        out _lblExpiry,       y);  y += 26;
        InfoRow("الأيام المتبقية:", out _lblDaysRemaining,y);  y += 26;
        InfoRow("مرخص لـ:",         out _lblLicensedTo,   y);  y += 26;

        // Machine ID row
        pnlCard.Controls.Add(new Label
        {
            Text      = "معرّف الجهاز:",
            Location  = new Point(280, y),
            AutoSize  = true,
            Font      = AppTheme.FontSmall,
            ForeColor = AppTheme.TextLabel
        });
        _lblMachineId = new Label
        {
            Location  = new Point(24, y),
            AutoSize  = true,
            Font      = new Font("Consolas", 9f),
            ForeColor = AppTheme.TextSecondary,
            Cursor    = Cursors.Hand
        };
        _lblMachineId.Click += (_, _) =>
        {
            Clipboard.SetText(_lblMachineId.Text);
            ShowSuccess("تم نسخ معرّف الجهاز إلى الحافظة");
        };
        pnlCard.Controls.Add(_lblMachineId);

        // Copy machine ID button
        var btnCopyId = new Button
        {
            Text      = "📋 نسخ",
            Location  = new Point(_lblMachineId.Left + 140, y - 2),
            Width     = 70, Height = 24,
            FlatStyle = FlatStyle.Flat,
            BackColor = AppTheme.BgContent,
            ForeColor = AppTheme.AccentBlue,
            Font      = AppTheme.FontSmall,
            Cursor    = Cursors.Hand
        };
        btnCopyId.FlatAppearance.BorderColor = AppTheme.AccentBlue;
        btnCopyId.Click += (_, _) =>
        {
            Clipboard.SetText(_lblMachineId.Text);
            ShowSuccess("تم نسخ معرّف الجهاز");
        };
        pnlCard.Controls.Add(btnCopyId);

        // ── Activation section ────────────────────────────────────
        var pnlActivate = new Panel
        {
            Dock      = DockStyle.Fill,
            BackColor = AppTheme.BgContent,
            Padding   = new Padding(24, 16, 24, 16)
        };

        var lblActivTitle = new Label
        {
            Text      = "تفعيل البرنامج",
            Font      = AppTheme.FontH2,
            ForeColor = AppTheme.TextPrimary,
            Dock      = DockStyle.Top,
            Height    = 32
        };

        var lblInstructions = new Label
        {
            Text      = "للحصول على كود التفعيل، تواصل مع Core Tech وأرسل لهم معرّف جهازك أعلاه.",
            Font      = AppTheme.FontSmall,
            ForeColor = AppTheme.TextSecondary,
            Dock      = DockStyle.Top,
            Height    = 20
        };

        var lblCodeHint = new Label
        {
            Text      = "كود التفعيل:",
            Font      = AppTheme.FontBodyBold,
            ForeColor = AppTheme.TextLabel,
            Location  = new Point(0, 70),
            AutoSize  = true
        };

        _txtCode = new TextBox
        {
            Location        = new Point(0, 92),
            Width           = 580,
            Height          = AppTheme.InputHeight + 6,
            Font            = new Font("Consolas", 11f),
            BorderStyle     = BorderStyle.FixedSingle,
            PlaceholderText = "الصق كود التفعيل هنا..."
        };

        _lblActivResult = new Label
        {
            Location  = new Point(0, 128),
            AutoSize  = true,
            Font      = AppTheme.FontBodyBold,
            ForeColor = AppTheme.AccentGreen
        };

        _btnActivate = new Button
        {
            Text      = "✔ تفعيل البرنامج",
            Location  = new Point(0, 152),
            Width     = 200, Height = 44,
            FlatStyle = FlatStyle.Flat,
            BackColor = AppTheme.AccentGreen,
            ForeColor = Color.White,
            Font      = new Font("Segoe UI", 12f, FontStyle.Bold),
            Cursor    = Cursors.Hand
        };
        _btnActivate.FlatAppearance.BorderSize = 0;
        _btnActivate.Click += (_, _) => DoActivation();

        // Contact info
        var pnlContact = new Panel
        {
            Location  = new Point(0, 210),
            Width     = 580, Height = 56,
            BackColor = Color.FromArgb(219, 234, 254),
            Padding   = new Padding(12, 8, 12, 8)
        };
        pnlContact.Controls.Add(new Label
        {
            Text      = "📞 للدعم الفني: Core Tech — جوهرة مول، العاشر من رمضان\n" +
                        "يمكنك شراء الترخيص وتجديده من خلالنا مباشرة.",
            Dock      = DockStyle.Fill,
            Font      = AppTheme.FontSmall,
            ForeColor = Color.FromArgb(29, 78, 216)
        });

        pnlActivate.Controls.AddRange([
            lblActivTitle, lblInstructions,
            lblCodeHint, _txtCode, _lblActivResult,
            _btnActivate, pnlContact
        ]);

        Controls.AddRange([pnlActivate, pnlCard, pnlHeader]);
    }

    // ── Load license info ─────────────────────────────────────────
    private void LoadLicenseInfo()
    {
        var machineId       = _licenseService.GetMachineFingerprint();
        _lblMachineId.Text  = machineId;

        Task.Run(async () =>
        {
            var info = await _licenseService.ValidateLicenseAsync();
            InvokeOnUI(() => UpdateLicenseDisplay(info));
        });
    }

    private void UpdateLicenseDisplay(LicenseInfo info)
    {
        _lblType.Text         = info.LicenseType switch
        {
            "Trial"        => "تجريبي",
            "Standard"     => "قياسي",
            "Professional" => "احترافي",
            _              => info.LicenseType
        };
        _lblExpiry.Text       = info.ExpiryDate?.ToString("dd/MM/yyyy") ?? "—";
        _lblLicensedTo.Text   = string.IsNullOrEmpty(info.LicensedTo) ? "—" : info.LicensedTo;

        switch (info.Status)
        {
            case LicenseStatus.Trial:
                _lblStatus.Text         = $"🕐 تجريبي ({info.DaysRemaining} يوم)";
                _pnlStatusBadge.BackColor = AppTheme.AccentOrange;
                _lblDaysRemaining.Text  = $"{info.DaysRemaining} يوم متبقي";
                _lblDaysRemaining.ForeColor = info.DaysRemaining <= 3
                    ? AppTheme.AccentRed : AppTheme.AccentOrange;
                break;

            case LicenseStatus.Active:
                _lblStatus.Text         = "✅ مفعّل";
                _pnlStatusBadge.BackColor = AppTheme.AccentGreen;
                _lblDaysRemaining.Text  = $"{info.DaysRemaining} يوم";
                _lblDaysRemaining.ForeColor = info.WillExpireSoon
                    ? AppTheme.AccentOrange : AppTheme.AccentGreen;
                break;

            case LicenseStatus.Expired:
                _lblStatus.Text         = "❌ منتهي الصلاحية";
                _pnlStatusBadge.BackColor = AppTheme.AccentRed;
                _lblDaysRemaining.Text  = "0 يوم";
                _lblDaysRemaining.ForeColor = AppTheme.AccentRed;
                break;

            default:
                _lblStatus.Text         = "⚠ غير معروف";
                _pnlStatusBadge.BackColor = AppTheme.TextSecondary;
                _lblDaysRemaining.Text  = "—";
                break;
        }
    }

    // ── Activation ────────────────────────────────────────────────
    private void DoActivation()
    {
        var code = _txtCode.Text.Trim();
        if (string.IsNullOrEmpty(code))
        {
            _lblActivResult.Text      = "❌ أدخل كود التفعيل";
            _lblActivResult.ForeColor = AppTheme.AccentRed;
            return;
        }

        _btnActivate.Enabled      = false;
        _lblActivResult.Text      = "⏳ جاري التحقق...";
        _lblActivResult.ForeColor = AppTheme.TextSecondary;

        Task.Run(async () =>
        {
            var result = await _licenseService.ActivateLicenseAsync(code);
            InvokeOnUI(() =>
            {
                _btnActivate.Enabled = true;
                if (result.Success)
                {
                    _lblActivResult.Text      = $"✅ تم التفعيل بنجاح! صالح حتى: {result.ExpiryDate:dd/MM/yyyy}";
                    _lblActivResult.ForeColor = AppTheme.AccentGreen;
                    _txtCode.Clear();
                    LoadLicenseInfo();
                    ShowSuccess($"تم تفعيل البرنامج بنجاح!\nالنوع: {result.LicenseType}\nصالح حتى: {result.ExpiryDate:dd/MM/yyyy}");
                }
                else
                {
                    _lblActivResult.Text      = $"❌ {result.Error}";
                    _lblActivResult.ForeColor = AppTheme.AccentRed;
                }
            });
        });
    }
}

// ════════════════════════════════════════════════════════════════════
// LICENSE GUARD — startup license check dialog
// ════════════════════════════════════════════════════════════════════
/// <summary>
/// Shown on startup if license is expired or not found.
/// Options: Activate now, or Continue in trial (if days remain).
/// </summary>
public sealed class LicenseGuard : Form
{
    private readonly ILicenseService _licenseService;
    private readonly LicenseInfo     _licenseInfo;

    public bool AllowContinue { get; private set; } = false;

    public LicenseGuard(ILicenseService licenseService, LicenseInfo licenseInfo)
    {
        _licenseService = licenseService;
        _licenseInfo    = licenseInfo;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        bool isExpired = _licenseInfo.IsExpired || _licenseInfo.Status == LicenseStatus.NotFound;
        bool isTrial   = _licenseInfo.IsTrial;

        Text            = isExpired ? "انتهت صلاحية البرنامج" : "تحذير — الترخيص";
        Size            = new Size(520, 380);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        MinimizeBox     = false;
        StartPosition   = FormStartPosition.CenterScreen;
        BackColor       = AppTheme.BgCard;
        RightToLeft     = RightToLeft.Yes;
        RightToLeftLayout = true;

        // Icon + message
        var pnlTop = new Panel { Dock = DockStyle.Top, Height = 120, BackColor = isExpired ? AppTheme.AccentRed : AppTheme.AccentOrange };
        pnlTop.Controls.Add(new Label
        {
            Text      = isExpired ? "⛔" : "⚠",
            Font      = new Font("Segoe UI Emoji", 32f),
            Location  = new Point(16, 20),
            AutoSize  = true,
            ForeColor = Color.White
        });
        pnlTop.Controls.Add(new Label
        {
            Text      = isExpired ? "انتهت صلاحية البرنامج" : $"الترخيص التجريبي — {_licenseInfo.DaysRemaining} أيام متبقية",
            Location  = new Point(80, 32),
            AutoSize  = true,
            Font      = new Font("Segoe UI", 16f, FontStyle.Bold),
            ForeColor = Color.White
        });
        pnlTop.Controls.Add(new Label
        {
            Text      = isExpired
                ? "لقد انتهت صلاحية ترخيصك. يرجى تجديد الترخيص للمتابعة."
                : $"ينتهي الترخيص التجريبي بتاريخ: {_licenseInfo.ExpiryDate:dd/MM/yyyy}",
            Location  = new Point(80, 70),
            AutoSize  = true,
            Font      = AppTheme.FontBody,
            ForeColor = Color.White
        });

        // Activation section
        var pnlBody = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 16, 20, 16) };

        var lblCodeHint = new Label
        {
            Text      = "لديك كود تفعيل؟ أدخله هنا:",
            Location  = new Point(16, 16),
            AutoSize  = true,
            Font      = AppTheme.FontBodyBold,
            ForeColor = AppTheme.TextPrimary
        };

        var txtCode = new TextBox
        {
            Location        = new Point(16, 40),
            Width           = 440,
            Height          = AppTheme.InputHeight + 4,
            Font            = new Font("Consolas", 10f),
            BorderStyle     = BorderStyle.FixedSingle,
            PlaceholderText = "أدخل كود التفعيل..."
        };

        var lblResult = new Label
        {
            Location  = new Point(16, 76),
            AutoSize  = true,
            Font      = AppTheme.FontSmallBold
        };

        var btnActivate = new Button
        {
            Text      = "✔ تفعيل الآن",
            Location  = new Point(16, 108),
            Width     = 160, Height = 38,
            FlatStyle = FlatStyle.Flat,
            BackColor = AppTheme.AccentGreen,
            ForeColor = Color.White,
            Font      = AppTheme.FontBodyBold,
            Cursor    = Cursors.Hand
        };
        btnActivate.FlatAppearance.BorderSize = 0;
        btnActivate.Click += (_, _) =>
        {
            var code = txtCode.Text.Trim();
            if (string.IsNullOrEmpty(code)) return;
            btnActivate.Enabled = false;
            lblResult.Text      = "⏳ جاري التحقق...";
            lblResult.ForeColor = AppTheme.TextSecondary;
            Task.Run(async () =>
            {
                var r = await _licenseService.ActivateLicenseAsync(code);
                Invoke(() =>
                {
                    btnActivate.Enabled = true;
                    if (r.Success)
                    {
                        AllowContinue  = true;
                        lblResult.Text = $"✅ تم التفعيل بنجاح!";
                        lblResult.ForeColor = AppTheme.AccentGreen;
                        Task.Delay(1500).ContinueWith(_ => Invoke(Close));
                    }
                    else
                    {
                        lblResult.Text = $"❌ {r.Error}";
                        lblResult.ForeColor = AppTheme.AccentRed;
                    }
                });
            });
        };

        pnlBody.Controls.AddRange([lblCodeHint, txtCode, lblResult, btnActivate]);

        // Bottom buttons
        var pnlBtns = new Panel
        {
            Dock      = DockStyle.Bottom,
            Height    = 60,
            BackColor = AppTheme.BgContent,
            Padding   = new Padding(12, 10, 12, 10)
        };

        if (!isExpired && isTrial)
        {
            var btnContinue = new Button
            {
                Text      = $"⏭ متابعة التجريبي ({_licenseInfo.DaysRemaining} يوم)",
                Dock      = DockStyle.Left,
                Width     = 230, Height = 38,
                FlatStyle = FlatStyle.Flat,
                BackColor = AppTheme.AccentOrange,
                ForeColor = Color.White,
                Font      = AppTheme.FontBodyBold,
                Cursor    = Cursors.Hand
            };
            btnContinue.FlatAppearance.BorderSize = 0;
            btnContinue.Click += (_, _) => { AllowContinue = true; Close(); };
            pnlBtns.Controls.Add(btnContinue);
        }

        var btnExit = new Button
        {
            Text      = "✕ خروج",
            Dock      = DockStyle.Right,
            Width     = 100, Height = 38,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = AppTheme.AccentRed,
            Font      = AppTheme.FontBodyBold,
            Cursor    = Cursors.Hand
        };
        btnExit.FlatAppearance.BorderColor = AppTheme.AccentRed;
        btnExit.Click += (_, _) => { AllowContinue = false; Close(); };
        pnlBtns.Controls.Add(btnExit);

        Controls.AddRange([pnlBtns, pnlBody, pnlTop]);
    }
}
