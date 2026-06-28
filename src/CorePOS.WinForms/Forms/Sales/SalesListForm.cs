using MediatR;
using CorePOS.WinForms.Theme;
using CorePOS.WinForms.Infrastructure;
using CorePOS.WinForms.Controls;
using CorePOS.Application.Features.Sales.Queries;
using CorePOS.Application.Features.Sales.Commands;

namespace CorePOS.WinForms.Forms.Sales;

/// <summary>
/// Sales invoices list screen.
/// Search by: invoice no, customer name, date range, payment method.
/// Actions: view, edit, delete, return (full/partial).
/// </summary>
public sealed class SalesListForm : BaseForm
{
    private DataGridView _dgv      = null!;
    private TextBox  _txtSearch    = null!;
    private DateTimePicker _dtFrom = null!;
    private DateTimePicker _dtTo   = null!;
    private ComboBox _cmbPayMethod = null!;
    private Label    _lblCount     = null!;
    private Label    _lblTotal     = null!;
    private System.Windows.Forms.Timer _searchDebounce = null!;

    public SalesListForm(IMediator mediator) : base(mediator)
    {
        InitializeComponent();
        LoadInvoices();
    }

    private void InitializeComponent()
    {
        Text      = "فواتير المبيعات";
        BackColor = AppTheme.BgContent;
        Padding   = new Padding(16, 12, 16, 12);

        // ── Toolbar ────────────────────────────────────────────────
        var toolbar = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 100,
            BackColor = AppTheme.BgCard,
            Padding   = new Padding(12)
        };

        // Row 1: search + pay method filter
        var pnlRow1 = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = AppTheme.BgCard };

        _txtSearch = new TextBox
        {
            Dock            = DockStyle.Right,
            Width           = 280,
            Height          = AppTheme.InputHeight,
            Font            = AppTheme.FontBody,
            BorderStyle     = BorderStyle.FixedSingle,
            PlaceholderText = "رقم فاتورة / اسم عميل..."
        };
        _searchDebounce = new System.Windows.Forms.Timer { Interval = 400 };
        _searchDebounce.Tick += (_, _) => { _searchDebounce.Stop(); LoadInvoices(); };
        _txtSearch.TextChanged += (_, _) => { _searchDebounce.Stop(); _searchDebounce.Start(); };

        var lblPayMethod = new Label
        {
            Text = "طريقة الدفع:", Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel,
            AutoSize = true, Dock = DockStyle.Right, TextAlign = ContentAlignment.MiddleRight, Width = 90
        };
        _cmbPayMethod = new ComboBox
        {
            Dock = DockStyle.Right, Width = 140, DropDownStyle = ComboBoxStyle.DropDownList,
            Font = AppTheme.FontBody
        };
        _cmbPayMethod.Items.AddRange(["الكل", "نقدي", "فيزا", "تحويل بنكي", "محفظة", "آجل", "مختلط"]);
        _cmbPayMethod.SelectedIndex = 0;
        _cmbPayMethod.SelectedIndexChanged += (_, _) => LoadInvoices();

        var lblSearch = new Label
        {
            Text = "🔍", Font = new Font("Segoe UI", 12f), AutoSize = true,
            Dock = DockStyle.Right, TextAlign = ContentAlignment.MiddleCenter, Width = 28
        };
        pnlRow1.Controls.AddRange([lblSearch, _txtSearch, lblPayMethod, _cmbPayMethod]);

        // Row 2: date range + action buttons
        var pnlRow2 = new Panel { Dock = DockStyle.Bottom, Height = 44, BackColor = AppTheme.BgCard };

        var lblFrom = new Label
        {
            Text = "من:", AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel,
            Dock = DockStyle.Right, TextAlign = ContentAlignment.MiddleRight, Width = 30
        };
        _dtFrom = new DateTimePicker
        {
            Dock = DockStyle.Right, Width = 140, Format = DateTimePickerFormat.Short,
            Value = DateTime.Today.AddDays(-30), Font = AppTheme.FontBody
        };
        _dtFrom.ValueChanged += (_, _) => LoadInvoices();

        var lblTo = new Label
        {
            Text = "إلى:", AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel,
            Dock = DockStyle.Right, TextAlign = ContentAlignment.MiddleRight, Width = 36
        };
        _dtTo = new DateTimePicker
        {
            Dock = DockStyle.Right, Width = 140, Format = DateTimePickerFormat.Short,
            Value = DateTime.Today, Font = AppTheme.FontBody
        };
        _dtTo.ValueChanged += (_, _) => LoadInvoices();

        // Action buttons
        Button MakeBtn(string text, Color color, EventHandler onClick)
        {
            var b = new Button
            {
                Text = text, Width = 110, Height = 32, Dock = DockStyle.Left,
                FlatStyle = FlatStyle.Flat, BackColor = color, ForeColor = Color.White,
                Font = AppTheme.FontSmall, Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 6, 0)
            };
            b.FlatAppearance.BorderSize = 0;
            b.Click += onClick;
            return b;
        }

        var btnNew      = MakeBtn("➕ فاتورة جديدة", AppTheme.AccentGreen,  (_, _) => NavigateToPOS());
        var btnView     = MakeBtn("👁 عرض",          AppTheme.AccentBlue,   (_, _) => ViewInvoice());
        var btnReturn   = MakeBtn("↩ مرتجع",         AppTheme.AccentOrange, (_, _) => ReturnInvoice());
        var btnDelete   = MakeBtn("🗑 حذف",           AppTheme.AccentRed,    (_, _) => DeleteInvoice());
        var btnExport   = MakeBtn("📤 تصدير",         AppTheme.AccentPurple, (_, _) => ExportInvoices());

        // Apply permissions
        btnNew.Visible    = CanAdd(Modules.Sales);
        btnReturn.Visible = CanEdit(Modules.Sales);
        btnDelete.Visible = CanDelete(Modules.Sales);
        btnExport.Visible = CanExport(Modules.Reports);

        var flowBtns = new FlowLayoutPanel
        {
            Dock = DockStyle.Left, Width = 480, FlowDirection = FlowDirection.LeftToRight,
            BackColor = AppTheme.BgCard, WrapContents = false
        };
        flowBtns.Controls.AddRange([btnNew, btnView, btnReturn, btnDelete, btnExport]);
        pnlRow2.Controls.AddRange([lblFrom, _dtFrom, lblTo, _dtTo, flowBtns]);

        toolbar.Controls.AddRange([pnlRow2, pnlRow1]);

        // ── Summary bar ────────────────────────────────────────────
        var pnlSummary = new Panel
        {
            Dock = DockStyle.Bottom, Height = 38, BackColor = AppTheme.BgCard,
            Padding = new Padding(12, 0, 12, 0)
        };

        _lblCount = new Label
        {
            Text = "عدد الفواتير: 0", Font = AppTheme.FontSmall,
            ForeColor = AppTheme.TextSecondary, Dock = DockStyle.Right,
            TextAlign = ContentAlignment.MiddleRight, AutoSize = false, Width = 160
        };
        _lblTotal = new Label
        {
            Text = "إجمالي: 0.00 ج.م", Font = AppTheme.FontBodyBold,
            ForeColor = AppTheme.AccentBlue, Dock = DockStyle.Right,
            TextAlign = ContentAlignment.MiddleRight, AutoSize = false, Width = 200
        };

        var sepLine = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = AppTheme.Border };
        pnlSummary.Controls.AddRange([_lblCount, _lblTotal, sepLine]);

        // ── Grid ───────────────────────────────────────────────────
        var pnlGrid = new Panel
        {
            Dock = DockStyle.Fill, BackColor = AppTheme.BgCard, Padding = new Padding(0)
        };

        _dgv = new DataGridView { Dock = DockStyle.Fill };
        AppTheme.StyleDataGrid(_dgv);
        _dgv.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "InvoiceId",    Visible   = false },
            new DataGridViewTextBoxColumn { Name = "InvoiceNo",    HeaderText = "رقم الفاتورة",   FillWeight = 12 },
            new DataGridViewTextBoxColumn { Name = "InvoiceDate",  HeaderText = "التاريخ",        FillWeight = 14 },
            new DataGridViewTextBoxColumn { Name = "CustomerName", HeaderText = "العميل",         FillWeight = 20 },
            new DataGridViewTextBoxColumn { Name = "ItemsCount",   HeaderText = "عدد الأصناف",   FillWeight = 10 },
            new DataGridViewTextBoxColumn { Name = "Subtotal",     HeaderText = "المجموع",        FillWeight = 12 },
            new DataGridViewTextBoxColumn { Name = "Discount",     HeaderText = "الخصم",          FillWeight = 8  },
            new DataGridViewTextBoxColumn { Name = "Tax",          HeaderText = "الضريبة",        FillWeight = 8  },
            new DataGridViewTextBoxColumn { Name = "Total",        HeaderText = "الإجمالي",       FillWeight = 12 },
            new DataGridViewTextBoxColumn { Name = "Paid",         HeaderText = "المدفوع",        FillWeight = 10 },
            new DataGridViewTextBoxColumn { Name = "PayMethod",    HeaderText = "طريقة الدفع",   FillWeight = 12 },
            new DataGridViewTextBoxColumn { Name = "Status",       HeaderText = "الحالة",         FillWeight = 10 },
            new DataGridViewTextBoxColumn { Name = "CashierName",  HeaderText = "الكاشير",        FillWeight = 12 }
        );
        _dgv.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) ViewInvoice(); };
        _dgv.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) ViewInvoice(); };

        pnlGrid.Controls.Add(_dgv);

        Controls.Add(pnlGrid);
        Controls.Add(pnlSummary);
        Controls.Add(toolbar);
    }

    // ── Data Loading ──────────────────────────────────────────────
    private void LoadInvoices()
    {
        RunAsync(async () =>
        {
            var q      = new GetSalesInvoicesQuery(
                BranchId:      UserSession.Current.BranchId,
                From:          _dtFrom.Value.Date,
                To:            _dtTo.Value.Date.AddDays(1).AddSeconds(-1),
                Search:        _txtSearch.Text.Trim(),
                PaymentMethod: _cmbPayMethod.SelectedIndex == 0 ? null : _cmbPayMethod.Text);

            var result = await _mediator.Send(q);

            InvokeOnUI(() =>
            {
                _dgv.Rows.Clear();
                if (!result.IsSuccess || result.Value == null) return;

                var invoices = result.Value;
                decimal totalSum = 0;

                foreach (var inv in invoices)
                {
                    _dgv.Rows.Add(
                        inv.InvoiceId,
                        inv.InvoiceNo,
                        inv.InvoiceDate.ToString("dd/MM/yyyy HH:mm"),
                        inv.CustomerName,
                        inv.ItemsCount,
                        $"{inv.Subtotal:N2}",
                        $"{inv.Discount:N2}",
                        $"{inv.Tax:N2}",
                        $"{inv.Total:N2}",
                        $"{inv.Paid:N2}",
                        inv.PaymentMethodAr,
                        inv.StatusAr,
                        inv.CashierName
                    );

                    // Color status cell
                    var statusCell = _dgv.Rows[_dgv.Rows.Count - 1].Cells["Status"];
                    statusCell.Style.ForeColor = inv.Status switch
                    {
                        "Returned" => AppTheme.AccentRed,
                        "Partial"  => AppTheme.AccentOrange,
                        _          => AppTheme.AccentGreen
                    };

                    totalSum += inv.Total;
                }

                _lblCount.Text = $"عدد الفواتير: {invoices.Count:N0}";
                _lblTotal.Text = $"إجمالي: {totalSum:N2} ج.م";
            });
        }, "جاري تحميل الفواتير...");
    }

    // ── Actions ───────────────────────────────────────────────────
    private int? GetSelectedInvoiceId()
    {
        if (_dgv.CurrentRow == null) return null;
        return _dgv.CurrentRow.Cells["InvoiceId"].Value as int?;
    }

    private void ViewInvoice()
    {
        var id = GetSelectedInvoiceId();
        if (id == null) return;
        using var dlg = new SaleInvoiceViewDialog(_mediator, id.Value);
        dlg.ShowDialog();
    }

    private void ReturnInvoice()
    {
        var id = GetSelectedInvoiceId();
        if (id == null) return;
        using var dlg = new SaleReturnDialog(_mediator, id.Value);
        if (dlg.ShowDialog() == DialogResult.OK)
            LoadInvoices();
    }

    private void DeleteInvoice()
    {
        var id = GetSelectedInvoiceId();
        if (id == null) return;
        if (!Confirm("هل تريد حذف هذه الفاتورة؟ لا يمكن التراجع عن هذه العملية.")) return;

        RunAsync(async () =>
        {
            var cmd    = new DeleteSaleInvoiceCommand(id.Value, UserSession.Current.UserId);
            var result = await _mediator.Send(cmd);
            InvokeOnUI(() =>
            {
                if (result.IsSuccess) LoadInvoices();
                else ShowError(result.Error);
            });
        }, "جاري الحذف...");
    }

    private void ExportInvoices()
    {
        // TODO: Phase 10 — export to Excel / PDF
        ShowSuccess("سيتم دعم التصدير في الإصدار القادم");
    }

    private void NavigateToPOS()
    {
        var parent = Parent;
        while (parent != null)
        {
            if (parent is MainForm mf) { mf.NavigateTo(Navigation.NavPages.POS); return; }
            parent = parent.Parent;
        }
    }
}
