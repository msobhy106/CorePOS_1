using MediatR;
using CorePOS.WinForms.Theme;
using CorePOS.WinForms.Infrastructure;
using CorePOS.Application.Features.Sales.Queries;
using CorePOS.Application.Features.Sales.Commands;

namespace CorePOS.WinForms.Forms.Sales;

// ════════════════════════════════════════════════════════════════════
// SALE INVOICE VIEW DIALOG
// ════════════════════════════════════════════════════════════════════
public sealed class SaleInvoiceViewDialog : Form
{
    private readonly IMediator _mediator;
    private readonly int       _invoiceId;

    public SaleInvoiceViewDialog(IMediator mediator, int invoiceId)
    {
        _mediator  = mediator;
        _invoiceId = invoiceId;
        InitializeComponent();
        LoadInvoice();
    }

    private DataGridView _dgvItems = null!;
    private Label _lblInvoiceNo    = null!;
    private Label _lblDate         = null!;
    private Label _lblCustomer     = null!;
    private Label _lblCashier      = null!;
    private Label _lblSubtotal     = null!;
    private Label _lblDiscount     = null!;
    private Label _lblTax          = null!;
    private Label _lblTotal        = null!;
    private Label _lblPaid         = null!;
    private Label _lblRemaining    = null!;
    private Label _lblPayMethod    = null!;
    private Label _lblNotes        = null!;
    private Label _lblStatus       = null!;

    private void InitializeComponent()
    {
        Text            = "تفاصيل الفاتورة";
        Size            = new Size(800, 600);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        StartPosition   = FormStartPosition.CenterParent;
        BackColor       = AppTheme.BgContent;
        RightToLeft     = RightToLeft.Yes;
        RightToLeftLayout = true;

        // ── Header info panel ──────────────────────────────────────
        var pnlHeader = new Panel
        {
            Dock = DockStyle.Top, Height = 100, BackColor = AppTheme.BgCard,
            Padding = new Padding(16, 12, 16, 12)
        };

        Label MakeInfoLabel(string prefix, out Label valLabel, int x, int y)
        {
            var lbl = new Label
            {
                Text = prefix, Font = AppTheme.FontSmallBold, ForeColor = AppTheme.TextLabel,
                Location = new Point(x, y), AutoSize = true
            };
            valLabel = new Label
            {
                Text = "...", Font = AppTheme.FontBody, ForeColor = AppTheme.TextPrimary,
                Location = new Point(x, y + 18), AutoSize = true
            };
            pnlHeader.Controls.AddRange([lbl, valLabel]);
            return lbl;
        }

        MakeInfoLabel("رقم الفاتورة:", out _lblInvoiceNo, pnlHeader.Width - 180, 10);
        MakeInfoLabel("التاريخ:",       out _lblDate,      pnlHeader.Width - 180, 56);
        MakeInfoLabel("العميل:",        out _lblCustomer,  20, 10);
        MakeInfoLabel("الكاشير:",       out _lblCashier,   20, 56);
        MakeInfoLabel("الحالة:",        out _lblStatus,    220, 10);
        MakeInfoLabel("طريقة الدفع:",  out _lblPayMethod,  220, 56);
        pnlHeader.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        // ── Items grid ─────────────────────────────────────────────
        var pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.BgCard, Padding = new Padding(0) };
        _dgvItems = new DataGridView { Dock = DockStyle.Fill };
        AppTheme.StyleDataGrid(_dgvItems);
        _dgvItems.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "RowNo",    HeaderText = "#",          Width = 40 },
            new DataGridViewTextBoxColumn { Name = "Barcode",  HeaderText = "باركود",     Width = 100 },
            new DataGridViewTextBoxColumn { Name = "NameAr",   HeaderText = "الصنف",      FillWeight = 40 },
            new DataGridViewTextBoxColumn { Name = "Unit",     HeaderText = "الوحدة",     Width = 70 },
            new DataGridViewTextBoxColumn { Name = "Price",    HeaderText = "السعر",      Width = 80 },
            new DataGridViewTextBoxColumn { Name = "Qty",      HeaderText = "الكمية",     Width = 70 },
            new DataGridViewTextBoxColumn { Name = "Discount", HeaderText = "خصم",        Width = 60 },
            new DataGridViewTextBoxColumn { Name = "LineTotal",HeaderText = "الإجمالي",   Width = 90 }
        );
        pnlGrid.Controls.Add(_dgvItems);

        // ── Totals panel ───────────────────────────────────────────
        var pnlTotals = new Panel
        {
            Dock = DockStyle.Bottom, Height = 130, BackColor = AppTheme.BgCard,
            Padding = new Padding(16, 8, 16, 8)
        };

        Panel MakeTotalRow2(string lbl, out Label val, int y, Color? color = null)
        {
            var row = new Panel { Location = new Point(0, y), Height = 24, Width = 400, BackColor = AppTheme.BgCard };
            var l = new Label
            {
                Text = lbl, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel,
                Dock = DockStyle.Right, Width = 120, TextAlign = ContentAlignment.MiddleRight, AutoSize = false
            };
            val = new Label
            {
                Text = "0.00", Font = AppTheme.FontSmallBold, ForeColor = color ?? AppTheme.TextPrimary,
                Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, AutoSize = false
            };
            row.Controls.AddRange([val, l]);
            pnlTotals.Controls.Add(row);
            return row;
        }

        MakeTotalRow2("المجموع الفرعي:", out _lblSubtotal,  0);
        MakeTotalRow2("الخصم:",          out _lblDiscount,  26, AppTheme.AccentRed);
        MakeTotalRow2("الضريبة:",        out _lblTax,       52, AppTheme.AccentOrange);
        MakeTotalRow2("الإجمالي:",       out _lblTotal,     78, AppTheme.AccentBlue);
        _lblTotal.Font = AppTheme.FontBodyBold;
        MakeTotalRow2("المدفوع:",        out _lblPaid,      0);
        MakeTotalRow2("المتبقي:",        out _lblRemaining, 0);

        var lblNotesHdr = new Label
        {
            Text = "ملاحظات:", Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel,
            Dock = DockStyle.Bottom, Height = 18, TextAlign = ContentAlignment.BottomRight
        };
        _lblNotes = new Label
        {
            Text = "-", Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary,
            Dock = DockStyle.Bottom, Height = 18, TextAlign = ContentAlignment.BottomRight
        };
        pnlTotals.Controls.AddRange([lblNotesHdr, _lblNotes]);

        // ── Action buttons ─────────────────────────────────────────
        var pnlBtns = new Panel
        {
            Dock = DockStyle.Bottom, Height = 52, BackColor = AppTheme.BgCard,
            Padding = new Padding(12, 8, 12, 8)
        };

        var btnPrint = new Button
        {
            Text = "🖨 طباعة", Dock = DockStyle.Right, Width = 110, Height = 34,
            FlatStyle = FlatStyle.Flat, BackColor = AppTheme.AccentBlue, ForeColor = Color.White,
            Font = AppTheme.FontBodyBold, Cursor = Cursors.Hand
        };
        btnPrint.FlatAppearance.BorderSize = 0;
        btnPrint.Click += (_, _) => { /* TODO: Phase 10 printing */ };

        var btnClose = new Button
        {
            Text = "إغلاق", Dock = DockStyle.Left, Width = 90, Height = 34,
            FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = AppTheme.TextSecondary,
            Font = AppTheme.FontBody, Cursor = Cursors.Hand
        };
        btnClose.FlatAppearance.BorderColor = AppTheme.Border;
        btnClose.Click += (_, _) => Close();
        pnlBtns.Controls.AddRange([btnPrint, btnClose]);

        Controls.AddRange([pnlBtns, pnlTotals, pnlGrid, pnlHeader]);
    }

    private void LoadInvoice()
    {
        Task.Run(async () =>
        {
            var q      = new GetSaleInvoiceDetailQuery(_invoiceId);
            var result = await _mediator.Send(q);

            Invoke(() =>
            {
                if (!result.IsSuccess || result.Value == null) { Close(); return; }
                var inv = result.Value;

                _lblInvoiceNo.Text  = inv.InvoiceNo;
                _lblDate.Text       = inv.InvoiceDate.ToString("dd/MM/yyyy  HH:mm");
                _lblCustomer.Text   = string.IsNullOrEmpty(inv.CustomerName) ? "بدون عميل" : inv.CustomerName;
                _lblCashier.Text    = inv.CashierName;
                _lblPayMethod.Text  = inv.PaymentMethodAr;
                _lblStatus.Text     = inv.StatusAr;
                _lblStatus.ForeColor = inv.Status == "Returned" ? AppTheme.AccentRed : AppTheme.AccentGreen;
                _lblSubtotal.Text   = $"{inv.Subtotal:N2}";
                _lblDiscount.Text   = $"{inv.Discount:N2}";
                _lblTax.Text        = $"{inv.Tax:N2}";
                _lblTotal.Text      = $"{inv.Total:N2}";
                _lblPaid.Text       = $"{inv.Paid:N2}";
                _lblRemaining.Text  = $"{inv.Total - inv.Paid:N2}";
                _lblNotes.Text      = string.IsNullOrEmpty(inv.Notes) ? "-" : inv.Notes;

                _dgvItems.Rows.Clear();
                int rowNo = 1;
                foreach (var item in inv.Items)
                {
                    _dgvItems.Rows.Add(
                        rowNo++, item.Barcode, item.NameAr, item.UnitName,
                        $"{item.Price:N2}", item.Qty, $"{item.Discount:N2}", $"{item.LineTotal:N2}"
                    );
                }
            });
        });
    }
}

// ════════════════════════════════════════════════════════════════════
// SALE RETURN DIALOG
// ════════════════════════════════════════════════════════════════════
public sealed class SaleReturnDialog : Form
{
    private readonly IMediator _mediator;
    private readonly int       _invoiceId;
    private DataGridView _dgv    = null!;
    private RadioButton  _rbFull = null!;
    private RadioButton  _rbPart = null!;

    public SaleReturnDialog(IMediator mediator, int invoiceId)
    {
        _mediator  = mediator;
        _invoiceId = invoiceId;
        InitializeComponent();
        LoadInvoiceItems();
    }

    private void InitializeComponent()
    {
        Text            = "مرتجع مبيعات";
        Size            = new Size(700, 520);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        StartPosition   = FormStartPosition.CenterParent;
        BackColor       = AppTheme.BgContent;
        RightToLeft     = RightToLeft.Yes;
        RightToLeftLayout = true;

        var pnlType = new Panel
        {
            Dock = DockStyle.Top, Height = 52, BackColor = AppTheme.BgCard,
            Padding = new Padding(16, 12, 16, 0)
        };
        var lblType = new Label
        {
            Text = "نوع المرتجع:", Font = AppTheme.FontBodyBold, ForeColor = AppTheme.TextPrimary,
            AutoSize = true, Location = new Point(16, 16)
        };
        _rbFull = new RadioButton
        {
            Text = "مرتجع كلي", Font = AppTheme.FontBody, Checked = true,
            Location = new Point(130, 14), AutoSize = true
        };
        _rbPart = new RadioButton
        {
            Text = "مرتجع جزئي", Font = AppTheme.FontBody,
            Location = new Point(230, 14), AutoSize = true
        };
        _rbFull.CheckedChanged += (_, _) => SetGridEditable(!_rbFull.Checked);
        pnlType.Controls.AddRange([lblType, _rbFull, _rbPart]);

        _dgv = new DataGridView { Dock = DockStyle.Fill };
        AppTheme.StyleDataGrid(_dgv);
        _dgv.ReadOnly = true;
        _dgv.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "ItemId",     HeaderText = "ID",          Visible = false },
            new DataGridViewTextBoxColumn { Name = "NameAr",     HeaderText = "الصنف",       FillWeight = 40 },
            new DataGridViewTextBoxColumn { Name = "Unit",       HeaderText = "الوحدة",      Width = 80 },
            new DataGridViewTextBoxColumn { Name = "OriginalQty",HeaderText = "الكمية الأصلية", Width = 100 },
            new DataGridViewTextBoxColumn { Name = "ReturnQty",  HeaderText = "كمية المرتجع", Width = 110, ReadOnly = false },
            new DataGridViewTextBoxColumn { Name = "Price",      HeaderText = "السعر",        Width = 80 },
            new DataGridViewTextBoxColumn { Name = "ReturnTotal",HeaderText = "إجمالي المرتجع", Width = 110 }
        );
        _dgv.CellEndEdit += (_, e) =>
        {
            if (_dgv.Columns[e.ColumnIndex].Name != "ReturnQty" || e.RowIndex < 0) return;
            var row = _dgv.Rows[e.RowIndex];
            if (decimal.TryParse(row.Cells["ReturnQty"].Value?.ToString(), out var qty) &&
                decimal.TryParse(row.Cells["Price"].Value?.ToString(),     out var price))
                row.Cells["ReturnTotal"].Value = $"{qty * price:N2}";
        };

        var pnlBtns = new Panel
        {
            Dock = DockStyle.Bottom, Height = 52, BackColor = AppTheme.BgCard,
            Padding = new Padding(12, 8, 12, 8)
        };

        var btnConfirm = new Button
        {
            Text = "✔ تأكيد المرتجع", Dock = DockStyle.Right, Width = 150, Height = 34,
            FlatStyle = FlatStyle.Flat, BackColor = AppTheme.AccentOrange, ForeColor = Color.White,
            Font = AppTheme.FontBodyBold, Cursor = Cursors.Hand
        };
        btnConfirm.FlatAppearance.BorderSize = 0;
        btnConfirm.Click += (_, _) => DoReturn();

        var btnCancel = new Button
        {
            Text = "إلغاء", Dock = DockStyle.Left, Width = 90, Height = 34,
            FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = AppTheme.TextSecondary,
            Font = AppTheme.FontBody, Cursor = Cursors.Hand
        };
        btnCancel.FlatAppearance.BorderColor = AppTheme.Border;
        btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        pnlBtns.Controls.AddRange([btnConfirm, btnCancel]);

        Controls.AddRange([pnlBtns, _dgv, pnlType]);
    }

    private void LoadInvoiceItems()
    {
        Task.Run(async () =>
        {
            var q      = new GetSaleInvoiceDetailQuery(_invoiceId);
            var result = await _mediator.Send(q);
            Invoke(() =>
            {
                if (!result.IsSuccess || result.Value == null) return;
                _dgv.Rows.Clear();
                foreach (var item in result.Value.Items)
                {
                    _dgv.Rows.Add(
                        item.InvoiceItemId, item.NameAr, item.UnitName,
                        item.Qty, item.Qty, // default return qty = original qty
                        $"{item.Price:N2}", $"{item.LineTotal:N2}"
                    );
                }
            });
        });
    }

    private void SetGridEditable(bool editable)
    {
        _dgv.ReadOnly = !editable;
        _dgv.Columns["ReturnQty"].ReadOnly = !editable;
    }

    private void DoReturn()
    {
        var items = new List<(int itemId, decimal qty)>();
        foreach (DataGridViewRow row in _dgv.Rows)
        {
            if (row.IsNewRow) continue;
            var itemId = (int)row.Cells["ItemId"].Value;
            if (_rbFull.Checked)
            {
                decimal.TryParse(row.Cells["OriginalQty"].Value?.ToString(), out var qty);
                items.Add((itemId, qty));
            }
            else
            {
                if (decimal.TryParse(row.Cells["ReturnQty"].Value?.ToString(), out var qty) && qty > 0)
                    items.Add((itemId, qty));
            }
        }

        if (items.Count == 0)
        {
            MessageBox.Show("يرجى تحديد كميات المرتجع", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Task.Run(async () =>
        {
            var cmd = new CreateSaleReturnCommand(_invoiceId, items, UserSession.Current.UserId);
            var result = await _mediator.Send(cmd);
            Invoke(() =>
            {
                if (result.IsSuccess)
                {
                    MessageBox.Show("تم تسجيل المرتجع بنجاح", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    MessageBox.Show(result.Error, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });
        });
    }
}
