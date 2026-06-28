using MediatR;
using System.Drawing.Printing;
using CorePOS.WinForms.Theme;
using CorePOS.WinForms.Infrastructure;
using CorePOS.WinForms.Forms.Printing;
using CorePOS.Application.Interfaces;
using CorePOS.Application.Features.Settings.Commands;
using CorePOS.Application.Features.Settings.Queries;

namespace CorePOS.WinForms.Forms.Settings;

/// <summary>
/// Phase 10 — Printer Settings Panel.
/// Embedded inside SettingsForm's "الطباعة" tab.
/// Manages: thermal printer name, print size, header/footer, cash drawer, auto-print.
/// </summary>
public sealed class PrintSettingsPanel : Panel
{
    private readonly IMediator       _mediator;
    private readonly IPrinterService _printerService;

    // ── Controls ──────────────────────────────────────────────────
    private ComboBox  _cmbPrinterName  = null!;
    private ComboBox  _cmbPrintSize    = null!;
    private TextBox   _txtHeader1      = null!;
    private TextBox   _txtHeader2      = null!;
    private TextBox   _txtFooter       = null!;
    private TextBox   _txtCompanyName  = null!;
    private TextBox   _txtCompanyPhone = null!;
    private TextBox   _txtCompanyAddr  = null!;
    private CheckBox  _chkAutoPrint    = null!;
    private CheckBox  _chkAskSize      = null!;
    private CheckBox  _chkCashDrawer   = null!;
    private CheckBox  _chkShowLogo     = null!;
    private Label     _lblPrinterStatus= null!;

    public PrintSettingsPanel(IMediator mediator, IPrinterService printerService)
    {
        _mediator       = mediator;
        _printerService = printerService;
        Dock      = DockStyle.Fill;
        BackColor = AppTheme.BgCard;
        BuildUI();
        LoadSettings();
    }

    private void BuildUI()
    {
        var scroll = new Panel
        {
            Dock      = DockStyle.Fill,
            AutoScroll= true,
            Padding   = new Padding(20, 16, 20, 16)
        };

        int y = 12;

        // ── Section: Printer ──────────────────────────────────────
        AddSectionTitle(scroll, "⚙ إعدادات الطابعة", ref y);

        // Printer name + refresh
        AddLabel(scroll, "اسم الطابعة الحرارية:", ref y);
        _cmbPrinterName = new ComboBox
        {
            Location      = new Point(16, y),
            Width         = 320,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font          = AppTheme.FontBody
        };
        LoadAvailablePrinters();
        scroll.Controls.Add(_cmbPrinterName);

        var btnRefresh = new Button
        {
            Text      = "🔄",
            Location  = new Point(346, y),
            Width     = 36, Height = AppTheme.InputHeight,
            FlatStyle = FlatStyle.Flat,
            BackColor = AppTheme.BgContent,
            Font      = AppTheme.FontBody,
            Cursor    = Cursors.Hand
        };
        btnRefresh.FlatAppearance.BorderColor = AppTheme.Border;
        btnRefresh.Click += (_, _) => LoadAvailablePrinters();
        scroll.Controls.Add(btnRefresh);

        _lblPrinterStatus = new Label
        {
            Location  = new Point(390, y + 6),
            AutoSize  = true,
            Font      = AppTheme.FontSmall,
            ForeColor = AppTheme.TextSecondary
        };
        scroll.Controls.Add(_lblPrinterStatus);
        y += AppTheme.InputHeight + 14;

        // Test print button
        var btnTest = new Button
        {
            Text      = "🖨 طباعة تجريبية",
            Location  = new Point(16, y),
            Width     = 150, Height = 34,
            FlatStyle = FlatStyle.Flat,
            BackColor = AppTheme.AccentBlue,
            ForeColor = Color.White,
            Font      = AppTheme.FontSmallBold,
            Cursor    = Cursors.Hand
        };
        btnTest.FlatAppearance.BorderSize = 0;
        btnTest.Click += async (_, _) => await DoTestPrint();
        scroll.Controls.Add(btnTest);

        var btnOpenDrawer = new Button
        {
            Text      = "🗄 اختبار الدرج النقدي",
            Location  = new Point(176, y),
            Width     = 170, Height = 34,
            FlatStyle = FlatStyle.Flat,
            BackColor = AppTheme.AccentOrange,
            ForeColor = Color.White,
            Font      = AppTheme.FontSmallBold,
            Cursor    = Cursors.Hand
        };
        btnOpenDrawer.FlatAppearance.BorderSize = 0;
        btnOpenDrawer.Click += async (_, _) => await DoOpenDrawer();
        scroll.Controls.Add(btnOpenDrawer);
        y += 50;

        // ── Section: Print Size ────────────────────────────────────
        AddSectionTitle(scroll, "📄 حجم الطباعة", ref y);

        AddLabel(scroll, "الحجم الافتراضي:", ref y);
        _cmbPrintSize = new ComboBox
        {
            Location      = new Point(16, y),
            Width         = 200,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font          = AppTheme.FontBody
        };
        _cmbPrintSize.Items.AddRange(["80mm — حراري عريض", "58mm — حراري صغير", "A4 — ورقة كاملة", "A5 — نصف ورقة"]);
        _cmbPrintSize.SelectedIndex = 0;
        scroll.Controls.Add(_cmbPrintSize);
        y += AppTheme.InputHeight + 14;

        _chkAskSize = AddCheckBox(scroll, "اسأل عن الحجم في كل مرة", ref y, false);
        _chkAutoPrint = AddCheckBox(scroll, "طباعة تلقائية بعد إتمام البيع", ref y, true);
        _chkCashDrawer = AddCheckBox(scroll, "فتح الدرج النقدي تلقائياً عند البيع النقدي", ref y, true);
        y += 8;

        // ── Section: Invoice Header/Footer ─────────────────────────
        AddSectionTitle(scroll, "🏷 رأس وذيل الفاتورة", ref y);

        AddLabel(scroll, "اسم الشركة/المحل:", ref y);
        _txtCompanyName = AddTextBox(scroll, "Core Tech", ref y, 440);

        AddLabel(scroll, "رقم الهاتف:", ref y);
        _txtCompanyPhone = AddTextBox(scroll, "01xxxxxxxxx", ref y, 300);

        AddLabel(scroll, "العنوان:", ref y);
        _txtCompanyAddr = AddTextBox(scroll, "جوهرة مول، العاشر من رمضان", ref y, 440);

        AddLabel(scroll, "سطر رأس الفاتورة 1 (اختياري):", ref y);
        _txtHeader1 = AddTextBox(scroll, "", ref y, 440);

        AddLabel(scroll, "سطر رأس الفاتورة 2 (اختياري):", ref y);
        _txtHeader2 = AddTextBox(scroll, "", ref y, 440);

        AddLabel(scroll, "نص ذيل الفاتورة:", ref y);
        _txtFooter = AddTextBox(scroll, "شكراً لتعاملكم معنا ♥", ref y, 440);

        _chkShowLogo = AddCheckBox(scroll, "إظهار شعار الشركة في الفاتورة (A4/A5 فقط)", ref y, false);
        y += 16;

        // ── Save button ────────────────────────────────────────────
        var btnSave = new Button
        {
            Text      = "💾 حفظ إعدادات الطباعة",
            Location  = new Point(16, y),
            Width     = 220, Height = 42,
            FlatStyle = FlatStyle.Flat,
            BackColor = AppTheme.AccentGreen,
            ForeColor = Color.White,
            Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
            Cursor    = Cursors.Hand
        };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += (_, _) => SaveSettings();
        scroll.Controls.Add(btnSave);

        Controls.Add(scroll);
    }

    // ── Load installed printers ────────────────────────────────────
    private void LoadAvailablePrinters()
    {
        var current = _cmbPrinterName.SelectedItem?.ToString() ?? string.Empty;
        _cmbPrinterName.Items.Clear();
        _cmbPrinterName.Items.Add("(الطابعة الافتراضية)");

        foreach (string printer in PrinterSettings.InstalledPrinters)
            _cmbPrinterName.Items.Add(printer);

        // Reselect
        bool found = false;
        for (int i = 0; i < _cmbPrinterName.Items.Count; i++)
        {
            if (_cmbPrinterName.Items[i]?.ToString() == current)
            {
                _cmbPrinterName.SelectedIndex = i;
                found = true;
                break;
            }
        }
        if (!found) _cmbPrinterName.SelectedIndex = 0;

        _lblPrinterStatus.Text = $"{_cmbPrinterName.Items.Count - 1} طابعة متاحة";
    }

    // ── Test print ────────────────────────────────────────────────
    private async Task DoTestPrint()
    {
        var printerName = GetSelectedPrinterName();
        try
        {
            await _printerService.PrintTestPageAsync(printerName);
            MessageBox.Show("تم إرسال صفحة الاختبار للطابعة", "نجاح",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"فشل الاختبار:\n{ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // ── Test cash drawer ─────────────────────────────────────────
    private async Task DoOpenDrawer()
    {
        var printerName = GetSelectedPrinterName();
        try
        {
            await _printerService.OpenCashDrawerAsync(printerName);
            MessageBox.Show("تم إرسال أمر فتح الدرج", "نجاح",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"فشل:\n{ex.Message}", "خطأ",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private string GetSelectedPrinterName()
    {
        var sel = _cmbPrinterName.SelectedItem?.ToString() ?? string.Empty;
        return sel == "(الطابعة الافتراضية)" ? string.Empty : sel;
    }

    // ── Load settings ─────────────────────────────────────────────
    private void LoadSettings()
    {
        Task.Run(async () =>
        {
            var result = await _mediator.Send(new GetSettingsQuery());
            Invoke(() =>
            {
                if (!result.IsSuccess || result.Value == null) return;
                var s = result.Value;

                SetComboByValue(_cmbPrinterName,
                    s.GetValueOrDefault("ThermalPrinterName", ""),
                    "(الطابعة الافتراضية)");

                SetComboIndex(_cmbPrintSize, s.GetValueOrDefault("DefaultPrintSize", "80mm") switch
                {
                    "80mm" => 0, "58mm" => 1, "A4" => 2, "A5" => 3, _ => 0
                });

                _chkAutoPrint.Checked  = s.GetValueOrDefault("AutoPrint",     "true") == "true";
                _chkAskSize.Checked    = s.GetValueOrDefault("AskPrintSize",  "false") == "true";
                _chkCashDrawer.Checked = s.GetValueOrDefault("OpenCashDrawer","true") == "true";
                _chkShowLogo.Checked   = s.GetValueOrDefault("ShowLogo",      "false") == "true";

                _txtCompanyName.Text  = s.GetValueOrDefault("CompanyNameAr",    "");
                _txtCompanyPhone.Text = s.GetValueOrDefault("CompanyPhone",     "");
                _txtCompanyAddr.Text  = s.GetValueOrDefault("CompanyAddress",   "");
                _txtHeader1.Text      = s.GetValueOrDefault("InvoiceHeader1",   "");
                _txtHeader2.Text      = s.GetValueOrDefault("InvoiceHeader2",   "");
                _txtFooter.Text       = s.GetValueOrDefault("InvoiceFooterText","شكراً لتعاملكم معنا ♥");
            });
        });
    }

    // ── Save settings ─────────────────────────────────────────────
    private void SaveSettings()
    {
        var printSizeMap = new[] { "80mm", "58mm", "A4", "A5" };
        var printSize    = printSizeMap[Math.Max(0, _cmbPrintSize.SelectedIndex)];

        var settings = new Dictionary<string, string>
        {
            ["ThermalPrinterName"]  = GetSelectedPrinterName(),
            ["DefaultPrintSize"]    = printSize,
            ["AutoPrint"]           = _chkAutoPrint.Checked.ToString().ToLower(),
            ["AskPrintSize"]        = _chkAskSize.Checked.ToString().ToLower(),
            ["OpenCashDrawer"]      = _chkCashDrawer.Checked.ToString().ToLower(),
            ["ShowLogo"]            = _chkShowLogo.Checked.ToString().ToLower(),
            ["CompanyNameAr"]       = _txtCompanyName.Text.Trim(),
            ["CompanyPhone"]        = _txtCompanyPhone.Text.Trim(),
            ["CompanyAddress"]      = _txtCompanyAddr.Text.Trim(),
            ["InvoiceHeader1"]      = _txtHeader1.Text.Trim(),
            ["InvoiceHeader2"]      = _txtHeader2.Text.Trim(),
            ["InvoiceFooterText"]   = _txtFooter.Text.Trim()
        };

        Task.Run(async () =>
        {
            var result = await _mediator.Send(new SaveSettingsCommand(
                settings, UserSession.Current.UserId));
            Invoke(() =>
            {
                if (result.IsSuccess)
                    MessageBox.Show("تم حفظ إعدادات الطباعة", "نجاح",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show(result.Error, "خطأ",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        });
    }

    // ── UI Helpers ────────────────────────────────────────────────
    private static void AddSectionTitle(Panel p, string text, ref int y)
    {
        var sep = new Panel { Location = new Point(16, y), Width = 600, Height = 1, BackColor = AppTheme.Border };
        p.Controls.Add(sep);
        y += 4;

        var lbl = new Label
        {
            Text      = text,
            Location  = new Point(16, y),
            AutoSize  = true,
            Font      = AppTheme.FontH2,
            ForeColor = AppTheme.AccentBlue
        };
        p.Controls.Add(lbl);
        y += 30;
    }

    private static void AddLabel(Panel p, string text, ref int y)
    {
        p.Controls.Add(new Label
        {
            Text      = text,
            Location  = new Point(16, y),
            AutoSize  = true,
            Font      = AppTheme.FontSmall,
            ForeColor = AppTheme.TextLabel
        });
        y += 20;
    }

    private static TextBox AddTextBox(Panel p, string placeholder, ref int y, int w = 360)
    {
        var tb = new TextBox
        {
            Location        = new Point(16, y),
            Width           = w,
            Height          = AppTheme.InputHeight,
            Font            = AppTheme.FontBody,
            BorderStyle     = BorderStyle.FixedSingle,
            PlaceholderText = placeholder
        };
        p.Controls.Add(tb);
        y += AppTheme.InputHeight + 12;
        return tb;
    }

    private static CheckBox AddCheckBox(Panel p, string text, ref int y, bool defaultChecked = false)
    {
        var chk = new CheckBox
        {
            Text     = text,
            Location = new Point(16, y),
            AutoSize = true,
            Font     = AppTheme.FontBody,
            Checked  = defaultChecked
        };
        p.Controls.Add(chk);
        y += 30;
        return chk;
    }

    private static void SetComboByValue(ComboBox cmb, string value, string fallback)
    {
        for (int i = 0; i < cmb.Items.Count; i++)
        {
            if (cmb.Items[i]?.ToString() == value)
            { cmb.SelectedIndex = i; return; }
        }
        for (int i = 0; i < cmb.Items.Count; i++)
        {
            if (cmb.Items[i]?.ToString() == fallback)
            { cmb.SelectedIndex = i; return; }
        }
        if (cmb.Items.Count > 0) cmb.SelectedIndex = 0;
    }

    private static void SetComboIndex(ComboBox cmb, int index)
    {
        if (index >= 0 && index < cmb.Items.Count)
            cmb.SelectedIndex = index;
    }
}
