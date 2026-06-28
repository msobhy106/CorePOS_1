using FastReport;
using FastReport.Preview;
using FastReport.Export.PdfSimple;
using FastReport.Export.OoXML;
using MediatR;
using CorePOS.WinForms.Theme;
using CorePOS.WinForms.Infrastructure;
using CorePOS.Application.Interfaces;

namespace CorePOS.WinForms.Forms.Printing;

/// <summary>
/// Universal print preview window.
/// Shows FastReport preview control with toolbar:
/// Print, Export PDF, Export Excel, Zoom, Navigate pages.
/// Used by all reports and invoice printing.
/// </summary>
public sealed class PrintPreviewForm : Form
{
    private readonly IReportService   _reportService;
    private readonly IPrinterService  _printerService;

    // ── Controls ──────────────────────────────────────────────────
    private PreviewControl _preview        = null!;
    private Panel          _pnlToolbar     = null!;
    private Label          _lblTitle       = null!;
    private ComboBox       _cmbPrintSize   = null!;
    private ComboBox       _cmbZoom        = null!;
    private Label          _lblPageInfo    = null!;
    private Button         _btnPrint       = null!;
    private Button         _btnPdf         = null!;
    private Button         _btnExcel       = null!;

    // ── State ─────────────────────────────────────────────────────
    private Report?        _currentReport;
    private byte[]?        _currentPdfBytes;
    private readonly string _windowTitle;

    // ── Factory methods ───────────────────────────────────────────
    public static PrintPreviewForm ForInvoice(
        IReportService rs, IPrinterService ps, int invoiceId, string invoiceNo)
    {
        var form = new PrintPreviewForm(rs, ps, $"فاتورة رقم: {invoiceNo}");
        form.LoadInvoiceAsync(invoiceId);
        return form;
    }

    public static PrintPreviewForm ForBytes(
        IReportService rs, IPrinterService ps, byte[] pdfBytes, string title)
    {
        var form = new PrintPreviewForm(rs, ps, title);
        form.LoadBytesAsync(pdfBytes);
        return form;
    }

    public static PrintPreviewForm ForReport(
        IReportService rs, IPrinterService ps, Report report, string title)
    {
        var form = new PrintPreviewForm(rs, ps, title);
        form.LoadReport(report);
        return form;
    }

    // ── Constructor ───────────────────────────────────────────────
    private PrintPreviewForm(IReportService rs, IPrinterService ps, string title)
    {
        _reportService  = rs;
        _printerService = ps;
        _windowTitle    = title;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text            = $"معاينة الطباعة — {_windowTitle}";
        Size            = new Size(1000, 750);
        MinimumSize     = new Size(700, 500);
        WindowState     = FormWindowState.Maximized;
        FormBorderStyle = FormBorderStyle.Sizable;
        StartPosition   = FormStartPosition.CenterParent;
        BackColor       = AppTheme.BgContent;
        RightToLeft     = RightToLeft.Yes;
        RightToLeftLayout = true;

        // ── Toolbar ────────────────────────────────────────────────
        _pnlToolbar = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 54,
            BackColor = AppTheme.BgCard,
            Padding   = new Padding(8, 8, 8, 8)
        };

        var sep1 = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = AppTheme.Border };
        _pnlToolbar.Controls.Add(sep1);

        var flow = new FlowLayoutPanel
        {
            Dock          = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            BackColor     = AppTheme.BgCard,
            WrapContents  = false
        };

        Button Btn(string text, Color bg, EventHandler click, int w = 110)
        {
            var b = new Button
            {
                Text      = text,
                Width     = w,
                Height    = 36,
                FlatStyle = FlatStyle.Flat,
                BackColor = bg,
                ForeColor = Color.White,
                Font      = AppTheme.FontSmallBold,
                Cursor    = Cursors.Hand,
                Margin    = new Padding(0, 0, 6, 0)
            };
            b.FlatAppearance.BorderSize = 0;
            b.Click += click;
            return b;
        }

        _btnPrint = Btn("🖨 طباعة",       AppTheme.AccentBlue,   (_, _) => DoPrint());
        _btnPdf   = Btn("📄 تصدير PDF",   AppTheme.AccentRed,    (_, _) => ExportPdf());
        _btnExcel = Btn("📊 تصدير Excel", AppTheme.AccentGreen,  (_, _) => ExportExcel());

        var btnClose = Btn("✕ إغلاق", AppTheme.TextSecondary, (_, _) => Close(), 80);
        btnClose.BackColor = Color.White;
        btnClose.ForeColor = AppTheme.TextSecondary;
        btnClose.FlatAppearance.BorderColor = AppTheme.Border;
        btnClose.FlatAppearance.BorderSize  = 1;

        // Zoom combo
        var lblZoom = new Label
        {
            Text      = "تكبير:",
            AutoSize  = true,
            Font      = AppTheme.FontSmall,
            ForeColor = AppTheme.TextLabel,
            Dock      = DockStyle.None,
            Margin    = new Padding(0, 8, 4, 0)
        };
        _cmbZoom = new ComboBox
        {
            Width         = 90,
            Height        = 36,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font          = AppTheme.FontSmall,
            Margin        = new Padding(0, 0, 8, 0)
        };
        _cmbZoom.Items.AddRange(["50%", "75%", "100%", "125%", "150%", "200%", "صفحة كاملة"]);
        _cmbZoom.SelectedIndex = 2;
        _cmbZoom.SelectedIndexChanged += (_, _) => ApplyZoom();

        // Print size combo (for invoice reprints)
        var lblSize = new Label
        {
            Text      = "حجم:",
            AutoSize  = true,
            Font      = AppTheme.FontSmall,
            ForeColor = AppTheme.TextLabel,
            Margin    = new Padding(0, 8, 4, 0)
        };
        _cmbPrintSize = new ComboBox
        {
            Width         = 90,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font          = AppTheme.FontSmall,
            Margin        = new Padding(0, 0, 8, 0)
        };
        _cmbPrintSize.Items.AddRange(["A4", "A5", "80mm", "58mm"]);
        _cmbPrintSize.SelectedIndex = 0;

        // Page info label
        _lblPageInfo = new Label
        {
            Text      = "صفحة 1 من 1",
            Font      = AppTheme.FontSmall,
            ForeColor = AppTheme.TextSecondary,
            AutoSize  = true,
            Margin    = new Padding(0, 10, 12, 0)
        };

        // Title
        _lblTitle = new Label
        {
            Text      = _windowTitle,
            Font      = AppTheme.FontBodyBold,
            ForeColor = AppTheme.TextPrimary,
            AutoSize  = true,
            Margin    = new Padding(0, 10, 0, 0)
        };

        flow.Controls.AddRange([
            _btnPrint, _btnPdf, _btnExcel, btnClose,
            _cmbZoom, lblZoom, _cmbPrintSize, lblSize,
            _lblPageInfo, _lblTitle
        ]);
        _pnlToolbar.Controls.Add(flow);

        // ── Preview control ────────────────────────────────────────
        _preview = new PreviewControl
        {
            Dock         = DockStyle.Fill,
            BackColor    = Color.FromArgb(80, 80, 80),
            ShowToolbar  = false   // we use our own toolbar
        };
        _preview.PageChanged += (_, _) => UpdatePageInfo();

        Controls.AddRange([_preview, _pnlToolbar]);

        // Loading label (shown while report is preparing)
        var lblLoading = new Label
        {
            Text      = "⏳ جاري تحضير التقرير...",
            Font      = AppTheme.FontH1,
            ForeColor = AppTheme.TextSecondary,
            AutoSize  = true,
            Anchor    = AnchorStyles.None,
            Name      = "lblLoading"
        };
        Controls.Add(lblLoading);
        Resize += (_, _) =>
        {
            lblLoading.Location = new Point(
                (Width  - lblLoading.Width)  / 2,
                (Height - lblLoading.Height) / 2);
        };
    }

    // ── Load Methods ──────────────────────────────────────────────
    private void LoadInvoiceAsync(int invoiceId)
    {
        Task.Run(async () =>
        {
            try
            {
                var bytes = await _reportService.GenerateInvoicePrintAsync(invoiceId, "pdf");
                Invoke(() => ShowPdf(bytes));
            }
            catch (Exception ex)
            {
                Invoke(() => ShowError(ex.Message));
            }
        });
    }

    private void LoadBytesAsync(byte[] pdfBytes)
    {
        _currentPdfBytes = pdfBytes;
        Task.Run(() => Invoke(() => ShowPdf(pdfBytes)));
    }

    private void LoadReport(Report report)
    {
        _currentReport = report;
        Task.Run(() =>
        {
            try
            {
                if (!report.IsPrepared) report.Prepare();
                Invoke(() => ShowReportInPreview(report));
            }
            catch (Exception ex)
            {
                Invoke(() => ShowError(ex.Message));
            }
        });
    }

    private void ShowPdf(byte[] bytes)
    {
        _currentPdfBytes = bytes;
        HideLoading();

        // Write temp PDF and load in preview
        var tmp = Path.GetTempFileName() + ".pdf";
        File.WriteAllBytes(tmp, bytes);

        try
        {
            // FastReport can preview PDF directly
            using var report = new Report();
            _preview.Clear();
            // Load PDF bytes into preview via temp file
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(tmp) { UseShellExecute = true });
        }
        catch
        {
            // Fallback: show save dialog
            SavePdfDirect(bytes);
        }
    }

    private void ShowReportInPreview(Report report)
    {
        _currentReport = report;
        HideLoading();
        _preview.Report = report;
        UpdatePageInfo();
        ApplyZoom();
    }

    private void HideLoading()
    {
        var lbl = Controls.Find("lblLoading", false).FirstOrDefault();
        if (lbl != null) lbl.Visible = false;
    }

    private void ShowError(string msg)
    {
        HideLoading();
        MessageBox.Show($"فشل تحميل التقرير:\n{msg}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void UpdatePageInfo()
    {
        if (_currentReport?.PreparedPages != null)
            _lblPageInfo.Text = $"صفحة {_preview.CurrentPage + 1} من {_currentReport.PreparedPages.Count}";
    }

    private void ApplyZoom()
    {
        var zoom = _cmbZoom.SelectedItem?.ToString() ?? "100%";
        if (zoom == "صفحة كاملة")
        {
            _preview.ZoomMode = ZoomMode.PageWidth;
            return;
        }
        if (int.TryParse(zoom.Replace("%", ""), out var pct))
            _preview.Zoom = pct / 100f;
    }

    // ── Actions ───────────────────────────────────────────────────
    private void DoPrint()
    {
        if (_currentReport == null && _currentPdfBytes == null)
        { ShowError("لا يوجد محتوى للطباعة"); return; }

        using var dlg = new PrintDialog();
        dlg.AllowCurrentPage   = true;
        dlg.AllowPrintToFile   = false;
        dlg.AllowSomePages     = true;

        if (dlg.ShowDialog() != DialogResult.OK) return;

        try
        {
            if (_currentReport != null)
            {
                _currentReport.PrintSettings.PrinterName = dlg.PrinterSettings.PrinterName;
                _currentReport.Print();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"فشل الطباعة:\n{ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ExportPdf()
    {
        if (_currentPdfBytes == null && _currentReport == null) return;

        using var dlg = new SaveFileDialog
        {
            Title      = "حفظ بصيغة PDF",
            Filter     = "PDF Files|*.pdf",
            FileName   = $"{_windowTitle.Replace(":", "_").Replace("/", "_")}_{DateTime.Today:yyyyMMdd}.pdf"
        };

        if (dlg.ShowDialog() != DialogResult.OK) return;

        try
        {
            if (_currentPdfBytes != null)
            {
                File.WriteAllBytes(dlg.FileName, _currentPdfBytes);
            }
            else if (_currentReport != null)
            {
                using var ms = new MemoryStream();
                var pdf = new PDFSimpleExport { EmbedFonts = true };
                _currentReport.Export(pdf, ms);
                File.WriteAllBytes(dlg.FileName, ms.ToArray());
            }

            // Open the saved PDF
            if (MessageBox.Show("تم الحفظ بنجاح. هل تريد فتح الملف؟", "تم",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(dlg.FileName) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"فشل التصدير:\n{ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ExportExcel()
    {
        if (_currentReport == null) return;

        using var dlg = new SaveFileDialog
        {
            Title    = "حفظ بصيغة Excel",
            Filter   = "Excel Files|*.xlsx",
            FileName = $"{_windowTitle.Replace(":", "_").Replace("/", "_")}_{DateTime.Today:yyyyMMdd}.xlsx"
        };

        if (dlg.ShowDialog() != DialogResult.OK) return;

        try
        {
            using var ms = new MemoryStream();
            var xlsx = new XlsxExport();
            _currentReport.Export(xlsx, ms);
            File.WriteAllBytes(dlg.FileName, ms.ToArray());

            if (MessageBox.Show("تم الحفظ بنجاح. هل تريد فتح الملف؟", "تم",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(dlg.FileName) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"فشل التصدير:\n{ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static void SavePdfDirect(byte[] bytes)
    {
        using var dlg = new SaveFileDialog
        {
            Title  = "حفظ PDF",
            Filter = "PDF Files|*.pdf",
            FileName = $"CorePOS_{DateTime.Today:yyyyMMdd}.pdf"
        };
        if (dlg.ShowDialog() == DialogResult.OK)
            File.WriteAllBytes(dlg.FileName, bytes);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _currentReport?.Dispose();
        base.Dispose(disposing);
    }
}
