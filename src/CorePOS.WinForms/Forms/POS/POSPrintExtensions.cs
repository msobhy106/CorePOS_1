using MediatR;
using CorePOS.WinForms.Theme;
using CorePOS.WinForms.Infrastructure;
using CorePOS.WinForms.Forms.Printing;
using CorePOS.Application.Interfaces;
using CorePOS.Application.Features.Sales.Queries;

namespace CorePOS.WinForms.Forms.POS;

/// <summary>
/// Phase 10 additions to POSForm.
/// This partial class adds print integration to the existing POSForm.
/// 
/// HOW TO INTEGRATE:
/// 1. Add IReportService + IPrinterService to POSForm constructor
/// 2. Replace the empty PrintReceiptAsync() stub with this implementation
/// 3. Add the print size selection dialog
/// </summary>
public static class POSPrintExtensions
{
    /// <summary>
    /// Call this after a successful sale to handle printing.
    /// Replaces the stub PrintReceiptAsync() in POSForm.
    /// </summary>
    public static async Task HandlePostSalePrintAsync(
        IReportService  reportService,
        IPrinterService printerService,
        IMediator       mediator,
        int             invoiceId,
        string          invoiceNo,
        Form            parentForm)
    {
        try
        {
            // Check auto-print setting
            // In real app: read from UserSession or Settings
            bool autoPrint = true;
            string printSize = "80mm"; // from settings

            if (printSize is "58mm" or "80mm")
            {
                // Direct thermal print — no preview
                var invoice = await mediator.Send(new GetSaleInvoiceForPrintQuery(invoiceId));
                if (invoice.IsSuccess && invoice.Value != null)
                    await printerService.PrintInvoiceAsync(invoice.Value);
            }
            else
            {
                // A4/A5 — show print preview
                var bytes = await reportService.GenerateInvoicePrintAsync(invoiceId, "pdf");
                parentForm.Invoke(() =>
                {
                    using var preview = PrintPreviewForm.ForBytes(
                        reportService, printerService, bytes, $"فاتورة {invoiceNo}");
                    preview.ShowDialog(parentForm);
                });
            }
        }
        catch (Exception ex)
        {
            parentForm.Invoke(() =>
                MessageBox.Show($"تحذير: فشل الطباعة\n{ex.Message}\nتم حفظ الفاتورة بنجاح.",
                    "تحذير طباعة", MessageBoxButtons.OK, MessageBoxIcon.Warning));
        }
    }
}

// ════════════════════════════════════════════════════════════════════
// PRINT SIZE SELECTION DIALOG
// ════════════════════════════════════════════════════════════════════
/// <summary>
/// Shown before printing when "اسأل في كل مرة" setting is enabled.
/// User selects: 58mm / 80mm / A5 / A4 / بدون طباعة
/// </summary>
public sealed class PrintSizeDialog : Form
{
    public string? SelectedSize { get; private set; }

    public PrintSizeDialog(string defaultSize = "80mm")
    {
        Text            = "اختر حجم الطباعة";
        Size            = new Size(320, 280);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        MinimizeBox     = false;
        StartPosition   = FormStartPosition.CenterParent;
        BackColor       = AppTheme.BgCard;
        RightToLeft     = RightToLeft.Yes;
        RightToLeftLayout = true;

        var lblTitle = new Label
        {
            Text      = "اختر حجم الطباعة:",
            Font      = AppTheme.FontH2,
            ForeColor = AppTheme.TextPrimary,
            AutoSize  = true,
            Location  = new Point(16, 16)
        };

        var sizes = new[]
        {
            ("80mm  — طابعة حرارية عريضة", "80mm"),
            ("58mm  — طابعة حرارية صغيرة", "58mm"),
            ("A4    — ورقة عادية",          "A4"),
            ("A5    — نصف ورقة",            "A5"),
            ("بدون طباعة",                   "none")
        };

        int y = 52;
        RadioButton? firstRb = null;
        RadioButton? defaultRb = null;

        foreach (var (label, size) in sizes)
        {
            var rb = new RadioButton
            {
                Text     = label,
                Location = new Point(16, y),
                AutoSize = true,
                Font     = AppTheme.FontBody,
                Tag      = size,
                Checked  = size == defaultSize
            };
            Controls.Add(rb);
            firstRb    ??= rb;
            if (size == defaultSize) defaultRb = rb;
            y += 34;
        }

        if (defaultRb == null && firstRb != null)
            firstRb.Checked = true;

        var pnlBtns = new Panel
        {
            Location  = new Point(0, y + 10),
            Size      = new Size(320, 50),
            BackColor = AppTheme.BgCard
        };

        var btnOk = new Button
        {
            Text      = "✔ طباعة",
            Location  = new Point(16, 8),
            Width     = 120, Height = 34,
            FlatStyle = FlatStyle.Flat,
            BackColor = AppTheme.AccentBlue,
            ForeColor = Color.White,
            Font      = AppTheme.FontBodyBold,
            Cursor    = Cursors.Hand
        };
        btnOk.FlatAppearance.BorderSize = 0;
        btnOk.Click += (_, _) =>
        {
            foreach (Control c in Controls)
                if (c is RadioButton { Checked: true } rb)
                    SelectedSize = rb.Tag?.ToString();
            DialogResult = DialogResult.OK;
            Close();
        };

        var btnCancel = new Button
        {
            Text      = "إلغاء",
            Location  = new Point(160, 8),
            Width     = 90, Height = 34,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = AppTheme.TextSecondary,
            Font      = AppTheme.FontBody,
            Cursor    = Cursors.Hand
        };
        btnCancel.FlatAppearance.BorderColor = AppTheme.Border;
        btnCancel.Click += (_, _) => { SelectedSize = "none"; DialogResult = DialogResult.Cancel; Close(); };
        pnlBtns.Controls.AddRange([btnOk, btnCancel]);
        Controls.AddRange([lblTitle, pnlBtns]);
        Height = y + 80;
    }
}

// ════════════════════════════════════════════════════════════════════
// ADDITIONAL QUERY — GetSaleInvoiceForPrint
// ════════════════════════════════════════════════════════════════════
namespace CorePOS.Application.Features.Sales.Queries;
using MediatR;
using CorePOS.Application.Common;
using CorePOS.Domain.Entities;

/// <summary>Returns full SalesInvoice entity with Items + Customer for direct ESC/POS printing.</summary>
public record GetSaleInvoiceForPrintQuery(int InvoiceId) : IRequest<Result<SalesInvoice>>;
