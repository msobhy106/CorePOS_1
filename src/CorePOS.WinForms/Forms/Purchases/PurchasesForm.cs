using MediatR;
using CorePOS.WinForms.Theme;
using CorePOS.WinForms.Infrastructure;
using CorePOS.Application.Features.Purchases.Queries;
using CorePOS.Application.Features.Purchases.Commands;
using CorePOS.Application.Features.Suppliers.Queries;
using CorePOS.Application.Features.Products.Queries;

namespace CorePOS.WinForms.Forms.Purchases;

/// <summary>
/// Purchases list screen — shows all purchase invoices with CRUD.
/// </summary>
public sealed class PurchasesListForm : BaseForm
{
    private DataGridView   _dgv        = null!;
    private TextBox        _txtSearch  = null!;
    private DateTimePicker _dtFrom     = null!;
    private DateTimePicker _dtTo       = null!;
    private ComboBox       _cmbStatus  = null!;
    private Label          _lblCount   = null!;
    private Label          _lblTotal   = null!;
    private System.Windows.Forms.Timer _debounce = null!;

    public PurchasesListForm(IMediator mediator) : base(mediator)
    {
        Text      = "فواتير المشتريات";
        BackColor = AppTheme.BgContent;
        InitializeComponent();
        LoadInvoices();
    }

    private void InitializeComponent()
    {
        // ── Toolbar ────────────────────────────────────────────────
        var pnlToolbar = new Panel
        {
            Dock = DockStyle.Top, Height = 104, BackColor = AppTheme.BgCard, Padding = new Padding(12)
        };

        // Row 1
        var row1 = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = AppTheme.BgCard };

        _txtSearch = new TextBox
        {
            Dock = DockStyle.Right, Width = 280, Font = AppTheme.FontBody,
            BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "رقم فاتورة / اسم مورد..."
        };
        _debounce = new System.Windows.Forms.Timer { Interval = 400 };
        _debounce.Tick += (_, _) => { _debounce.Stop(); LoadInvoices(); };
        _txtSearch.TextChanged += (_, _) => { _debounce.Stop(); _debounce.Start(); };

        var lblStatus = new Label { Text = "الحالة:", Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel, AutoSize = true, Dock = DockStyle.Right, Width = 50, TextAlign = ContentAlignment.MiddleRight };
        _cmbStatus = new ComboBox { Dock = DockStyle.Right, Width = 140, DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody };
        _cmbStatus.Items.AddRange(["الكل", "مسودة", "معتمد", "جزئي"]);
        _cmbStatus.SelectedIndex = 0;
        _cmbStatus.SelectedIndexChanged += (_, _) => LoadInvoices();
        row1.Controls.AddRange([_txtSearch, lblStatus, _cmbStatus]);

        // Row 2
        var row2 = new Panel { Dock = DockStyle.Bottom, Height = 48, BackColor = AppTheme.BgCard };

        var lblFrom = new Label { Text = "من:", AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel, Dock = DockStyle.Right, Width = 30, TextAlign = ContentAlignment.MiddleRight };
        _dtFrom = new DateTimePicker { Dock = DockStyle.Right, Width = 140, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(-30), Font = AppTheme.FontBody };
        _dtFrom.ValueChanged += (_, _) => LoadInvoices();

        var lblTo = new Label { Text = "إلى:", AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel, Dock = DockStyle.Right, Width = 36, TextAlign = ContentAlignment.MiddleRight };
        _dtTo = new DateTimePicker { Dock = DockStyle.Right, Width = 140, Format = DateTimePickerFormat.Short, Value = DateTime.Today, Font = AppTheme.FontBody };
        _dtTo.ValueChanged += (_, _) => LoadInvoices();

        var flow = new FlowLayoutPanel { Dock = DockStyle.Left, Width = 420, FlowDirection = FlowDirection.LeftToRight, BackColor = AppTheme.BgCard, WrapContents = false };

        Button Btn(string t, Color c, EventHandler h)
        {
            var b = new Button { Text = t, Width = 110, Height = 34, FlatStyle = FlatStyle.Flat, BackColor = c, ForeColor = Color.White, Font = AppTheme.FontSmall, Cursor = Cursors.Hand, Margin = new Padding(0, 0, 6, 0) };
            b.FlatAppearance.BorderSize = 0; b.Click += h; return b;
        }

        flow.Controls.AddRange([
            Btn("➕ فاتورة جديدة", AppTheme.AccentGreen,  (_, _) => NewInvoice()),
            Btn("✏ تعديل",          AppTheme.AccentBlue,   (_, _) => EditInvoice()),
            Btn("✔ اعتماد",         AppTheme.AccentOrange, (_, _) => ApproveInvoice()),
            Btn("↩ مرتجع",          AppTheme.AccentRed,    (_, _) => ReturnInvoice()),
        ]);

        row2.Controls.AddRange([lblFrom, _dtFrom, lblTo, _dtTo, flow]);
        pnlToolbar.Controls.AddRange([row2, row1]);

        // ── Summary ────────────────────────────────────────────────
        var pnlSummary = new Panel { Dock = DockStyle.Bottom, Height = 36, BackColor = AppTheme.BgCard, Padding = new Padding(8, 0, 8, 0) };
        _lblTotal = new Label { Dock = DockStyle.Right, AutoSize = false, Width = 240, TextAlign = ContentAlignment.MiddleRight, Font = AppTheme.FontBodyBold, ForeColor = AppTheme.AccentOrange };
        _lblCount = new Label { Dock = DockStyle.Right, AutoSize = false, Width = 180, TextAlign = ContentAlignment.MiddleRight, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary };
        pnlSummary.Controls.AddRange([_lblTotal, _lblCount]);

        // ── Grid ───────────────────────────────────────────────────
        _dgv = new DataGridView { Dock = DockStyle.Fill };
        AppTheme.StyleDataGrid(_dgv);
        _dgv.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "InvoiceId",    Visible = false },
            new DataGridViewTextBoxColumn { Name = "InvoiceNo",    HeaderText = "رقم الفاتورة",  FillWeight = 12 },
            new DataGridViewTextBoxColumn { Name = "InvoiceDate",  HeaderText = "التاريخ",        FillWeight = 14 },
            new DataGridViewTextBoxColumn { Name = "SupplierName", HeaderText = "المورد",         FillWeight = 22 },
            new DataGridViewTextBoxColumn { Name = "ItemsCount",   HeaderText = "عدد الأصناف",   FillWeight = 10 },
            new DataGridViewTextBoxColumn { Name = "Total",        HeaderText = "الإجمالي",       FillWeight = 12 },
            new DataGridViewTextBoxColumn { Name = "Paid",         HeaderText = "المدفوع",        FillWeight = 10 },
            new DataGridViewTextBoxColumn { Name = "Remaining",    HeaderText = "المتبقي",        FillWeight = 10 },
            new DataGridViewTextBoxColumn { Name = "Status",       HeaderText = "الحالة",         FillWeight = 10 },
            new DataGridViewTextBoxColumn { Name = "CreatedBy",    HeaderText = "بواسطة",         FillWeight = 12 }
        );
        _dgv.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) EditInvoice(); };

        Controls.Add(_dgv);
        Controls.Add(pnlSummary);
        Controls.Add(pnlToolbar);
    }

    private void LoadInvoices()
    {
        RunAsync(async () =>
        {
            var q = new GetPurchaseInvoicesQuery(
                BranchId: UserSession.Current.BranchId,
                From:     _dtFrom.Value.Date,
                To:       _dtTo.Value.Date.AddDays(1).AddSeconds(-1),
                Search:   _txtSearch.Text.Trim(),
                Status:   _cmbStatus.SelectedIndex == 0 ? null : _cmbStatus.Text);

            var result = await _mediator.Send(q);
            InvokeOnUI(() =>
            {
                _dgv.Rows.Clear();
                if (!result.IsSuccess || result.Value == null) return;
                decimal total = 0;
                foreach (var inv in result.Value)
                {
                    var rowIdx = _dgv.Rows.Add(
                        inv.InvoiceId, inv.InvoiceNo,
                        inv.InvoiceDate.ToString("dd/MM/yyyy HH:mm"),
                        inv.SupplierName, inv.ItemsCount,
                        $"{inv.Total:N2}", $"{inv.Paid:N2}", $"{inv.Total - inv.Paid:N2}",
                        inv.StatusAr, inv.CreatedByName);

                    _dgv.Rows[rowIdx].Cells["Status"].Style.ForeColor = inv.Status switch
                    {
                        "Approved" => AppTheme.AccentGreen,
                        "Draft"    => AppTheme.AccentOrange,
                        _          => AppTheme.TextSecondary
                    };
                    total += inv.Total;
                }
                _lblCount.Text = $"عدد الفواتير: {result.Value.Count:N0}";
                _lblTotal.Text = $"إجمالي المشتريات: {total:N2} ج.م";
            });
        }, "جاري تحميل فواتير الشراء...");
    }

    private int? GetSelectedId() => _dgv.CurrentRow?.Cells["InvoiceId"].Value as int?;

    private void NewInvoice()
    {
        using var dlg = new PurchaseEditForm(_mediator, null);
        if (dlg.ShowDialog() == DialogResult.OK) LoadInvoices();
    }

    private void EditInvoice()
    {
        var id = GetSelectedId();
        if (id == null) return;
        using var dlg = new PurchaseEditForm(_mediator, id);
        if (dlg.ShowDialog() == DialogResult.OK) LoadInvoices();
    }

    private void ApproveInvoice()
    {
        var id = GetSelectedId();
        if (id == null) return;
        if (!Confirm("هل تريد اعتماد هذه الفاتورة؟ سيتم تحديث المخزون.")) return;
        RunAsync(async () =>
        {
            var result = await _mediator.Send(new ApprovePurchaseInvoiceCommand(id.Value, UserSession.Current.UserId));
            InvokeOnUI(() => { if (result.IsSuccess) LoadInvoices(); else ShowError(result.Error); });
        }, "جاري الاعتماد...");
    }

    private void ReturnInvoice()
    {
        var id = GetSelectedId();
        if (id == null) return;
        using var dlg = new PurchaseReturnDialog(_mediator, id.Value);
        if (dlg.ShowDialog() == DialogResult.OK) LoadInvoices();
    }
}

// ════════════════════════════════════════════════════════════════════
// PURCHASE EDIT FORM (Add / Edit)
// ════════════════════════════════════════════════════════════════════
public sealed class PurchaseEditForm : Form
{
    private readonly IMediator _mediator;
    private readonly int?      _invoiceId;

    private ComboBox     _cmbSupplier  = null!;
    private DateTimePicker _dtDate     = null!;
    private TextBox      _txtRefNo     = null!;
    private TextBox      _txtNotes     = null!;
    private DataGridView _dgvItems     = null!;
    private TextBox      _txtBarcodeSearch = null!;

    // Totals
    private Label _lblSubtotal   = null!;
    private Label _lblDiscount   = null!;
    private Label _lblTotal      = null!;
    private TextBox _txtDiscount = null!;
    private TextBox _txtPaid     = null!;

    private readonly List<PurchaseCartItem> _items = new();

    public PurchaseEditForm(IMediator mediator, int? invoiceId)
    {
        _mediator  = mediator;
        _invoiceId = invoiceId;
        InitializeComponent();
        LoadSuppliers();
        if (invoiceId.HasValue) LoadInvoice(invoiceId.Value);
    }

    private void InitializeComponent()
    {
        Text            = _invoiceId.HasValue ? "تعديل فاتورة شراء" : "فاتورة شراء جديدة";
        Size            = new Size(900, 620);
        WindowState     = FormWindowState.Maximized;
        FormBorderStyle = FormBorderStyle.Sizable;
        StartPosition   = FormStartPosition.CenterParent;
        BackColor       = AppTheme.BgContent;
        RightToLeft     = RightToLeft.Yes;
        RightToLeftLayout = true;

        // ── Header row ─────────────────────────────────────────────
        var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = AppTheme.BgCard, Padding = new Padding(16, 12, 16, 12) };

        // Row 1: Supplier + Date + Ref
        int hx = 16, hy = 8;
        void HLabel(string t, int x, int y) => pnlHeader.Controls.Add(new Label { Text = t, Location = new Point(x, y), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel });
        void HRow1()
        {
            HLabel("المورد:", hx, hy); hy += 20;
            _cmbSupplier = new ComboBox { Location = new Point(hx, hy), Width = 280, DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody };
            pnlHeader.Controls.Add(_cmbSupplier);
        }
        HRow1();
        hx = 320; hy = 8;
        HLabel("التاريخ:", hx, hy); hy += 20;
        _dtDate = new DateTimePicker { Location = new Point(hx, hy), Width = 180, Format = DateTimePickerFormat.Short };
        pnlHeader.Controls.Add(_dtDate);
        hx = 520; hy = 8;
        HLabel("رقم المرجع:", hx, hy); hy += 20;
        _txtRefNo = new TextBox { Location = new Point(hx, hy), Width = 200, Height = AppTheme.InputHeight, Font = AppTheme.FontBody, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "رقم فاتورة المورد" };
        pnlHeader.Controls.Add(_txtRefNo);

        hx = 16; hy = 60;
        HLabel("ملاحظات:", hx, hy); hy += 20;
        _txtNotes = new TextBox { Location = new Point(hx, hy), Width = 700, Height = AppTheme.InputHeight, Font = AppTheme.FontBody, BorderStyle = BorderStyle.FixedSingle };
        pnlHeader.Controls.Add(_txtNotes);

        // ── Search/add product row ─────────────────────────────────
        var pnlSearch = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = AppTheme.BgContent, Padding = new Padding(8) };
        var lblSearchHint = new Label { Text = "إضافة صنف (باركود/اسم):", AutoSize = true, Dock = DockStyle.Right, Width = 180, Font = AppTheme.FontBodyBold, ForeColor = AppTheme.TextLabel, TextAlign = ContentAlignment.MiddleRight };
        _txtBarcodeSearch = new TextBox { Dock = DockStyle.Fill, Font = AppTheme.FontBody, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "امسح الباركود أو ابحث..." };
        _txtBarcodeSearch.KeyDown += async (_, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; await SearchAndAddProduct(); }
        };
        pnlSearch.Controls.AddRange([_txtBarcodeSearch, lblSearchHint]);

        // ── Items grid ─────────────────────────────────────────────
        var pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.BgCard };
        _dgvItems = new DataGridView { Dock = DockStyle.Fill };
        AppTheme.StyleDataGrid(_dgvItems);
        _dgvItems.ReadOnly = false;
        _dgvItems.EditMode = DataGridViewEditMode.EditOnEnter;
        _dgvItems.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "RowNo",     HeaderText = "#",              Width = 36,  ReadOnly = true },
            new DataGridViewTextBoxColumn { Name = "Barcode",   HeaderText = "باركود",          Width = 100, ReadOnly = true },
            new DataGridViewTextBoxColumn { Name = "NameAr",    HeaderText = "الصنف",           FillWeight = 35, ReadOnly = true },
            new DataGridViewTextBoxColumn { Name = "Unit",      HeaderText = "الوحدة",          Width = 70,  ReadOnly = true },
            new DataGridViewTextBoxColumn { Name = "PurchPrice",HeaderText = "سعر الشراء",      Width = 100, ReadOnly = false },
            new DataGridViewTextBoxColumn { Name = "Qty",       HeaderText = "الكمية",          Width = 80,  ReadOnly = false },
            new DataGridViewTextBoxColumn { Name = "Discount",  HeaderText = "خصم",             Width = 70,  ReadOnly = false },
            new DataGridViewTextBoxColumn { Name = "LineTotal", HeaderText = "الإجمالي",        Width = 100, ReadOnly = true }
        );
        _dgvItems.CellEndEdit += DgvItems_CellEndEdit;
        _dgvItems.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Delete && _dgvItems.CurrentRow != null)
            {
                int idx = _dgvItems.CurrentRow.Index;
                if (idx >= 0 && idx < _items.Count) { _items.RemoveAt(idx); RefreshGrid(); RecalcTotals(); }
            }
        };
        pnlGrid.Controls.Add(_dgvItems);

        // ── Totals + action panel ──────────────────────────────────
        var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 80, BackColor = AppTheme.BgCard, Padding = new Padding(12, 8, 12, 8) };

        // Totals labels
        Label TL(string t) => new() { Text = t, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel, AutoSize = true };
        Label TV(out Label l, Color? c = null) { l = new Label { Font = AppTheme.FontBodyBold, ForeColor = c ?? AppTheme.TextPrimary, AutoSize = true }; return l; }

        var pnlTotals = new FlowLayoutPanel { Dock = DockStyle.Right, Width = 540, FlowDirection = FlowDirection.LeftToRight, BackColor = AppTheme.BgCard };
        pnlTotals.Controls.AddRange([TL("المجموع: "), TV(out _lblSubtotal), TL("   الخصم %: ")]);

        _txtDiscount = new TextBox { Width = 60, Height = 26, Font = AppTheme.FontBody, BorderStyle = BorderStyle.FixedSingle, Text = "0" };
        _txtDiscount.TextChanged += (_, _) => RecalcTotals();
        pnlTotals.Controls.Add(_txtDiscount);
        pnlTotals.Controls.AddRange([TL("   الإجمالي: "), TV(out _lblTotal, AppTheme.AccentBlue), TL("   المدفوع: ")]);
        _txtPaid = new TextBox { Width = 90, Height = 26, Font = AppTheme.FontBody, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "0.00" };
        pnlTotals.Controls.Add(_txtPaid);

        // Buttons
        var pnlBtns = new FlowLayoutPanel { Dock = DockStyle.Left, Width = 300, FlowDirection = FlowDirection.LeftToRight, BackColor = AppTheme.BgCard };
        Button AB(string t, Color c, EventHandler h) { var b = new Button { Text = t, Width = 120, Height = 40, FlatStyle = FlatStyle.Flat, BackColor = c, ForeColor = Color.White, Font = AppTheme.FontBodyBold, Cursor = Cursors.Hand, Margin = new Padding(0, 0, 8, 0) }; b.FlatAppearance.BorderSize = 0; b.Click += h; return b; }

        pnlBtns.Controls.AddRange([
            AB("💾 حفظ مسودة",  AppTheme.AccentOrange, (_, _) => SaveInvoice(false)),
            AB("✔ حفظ واعتماد", AppTheme.AccentGreen,  (_, _) => SaveInvoice(true))
        ]);

        pnlBottom.Controls.AddRange([pnlTotals, pnlBtns]);

        Controls.AddRange([pnlBottom, pnlGrid, pnlSearch, pnlHeader]);
    }

    private void LoadSuppliers()
    {
        Task.Run(async () =>
        {
            var result = await _mediator.Send(new GetSuppliersListQuery(string.Empty, 200));
            Invoke(() =>
            {
                if (!result.IsSuccess || result.Value == null) return;
                _cmbSupplier.Items.Add(new ComboItem(0, "-- اختر مورد --"));
                foreach (var s in result.Value.Items)
                    _cmbSupplier.Items.Add(new ComboItem(s.SupplierId, s.Name));
                _cmbSupplier.SelectedIndex = 0;
            });
        });
    }

    private void LoadInvoice(int id)
    {
        Task.Run(async () =>
        {
            var result = await _mediator.Send(new GetPurchaseInvoiceDetailQuery(id));
            Invoke(() =>
            {
                if (!result.IsSuccess || result.Value == null) return;
                var inv = result.Value;
                _dtDate.Value   = inv.InvoiceDate;
                _txtRefNo.Text  = inv.SupplierRefNo;
                _txtNotes.Text  = inv.Notes;
                _txtPaid.Text   = inv.Paid.ToString("N2");
                _txtDiscount.Text = inv.DiscountPercent.ToString();

                foreach (var item in inv.Items)
                    _items.Add(new PurchaseCartItem
                    {
                        ProductId    = item.ProductId,
                        Barcode      = item.Barcode,
                        NameAr       = item.NameAr,
                        UnitName     = item.UnitName,
                        PurchasePrice= item.Price,
                        Qty          = item.Qty,
                        Discount     = item.Discount,
                        LineTotal    = item.LineTotal
                    });
                RefreshGrid();
                RecalcTotals();

                // Select supplier
                foreach (var item in _cmbSupplier.Items)
                    if (item is ComboItem ci && ci.Id == inv.SupplierId)
                    { _cmbSupplier.SelectedItem = item; break; }
            });
        });
    }

    private async Task SearchAndAddProduct()
    {
        var query = _txtBarcodeSearch.Text.Trim();
        if (string.IsNullOrEmpty(query)) return;

        var q      = new GetProductByBarcodeQuery(query, UserSession.Current.WarehouseId);
        var result = await _mediator.Send(q);

        if (result.IsSuccess && result.Value != null)
        {
            var p        = result.Value;
            var existing = _items.FirstOrDefault(x => x.ProductId == p.ProductId);
            if (existing != null) { existing.Qty++; existing.LineTotal = existing.PurchasePrice * existing.Qty; }
            else _items.Add(new PurchaseCartItem { ProductId = p.ProductId, Barcode = p.Barcode, NameAr = p.NameAr, UnitName = p.PurchaseUnitName, PurchasePrice = p.PurchasePrice, Qty = 1, Discount = 0, LineTotal = p.PurchasePrice });
            RefreshGrid();
            RecalcTotals();
            _txtBarcodeSearch.Clear();
        }
        else
        {
            // Try text search then show picker
            var sq = new SearchProductsQuery(query, UserSession.Current.BranchId, 10);
            var sr = await _mediator.Send(sq);
            if (sr.IsSuccess && sr.Value?.Count > 0)
            {
                using var picker = new ProductPickerDialog(sr.Value);
                if (picker.ShowDialog() == DialogResult.OK && picker.SelectedProduct != null)
                {
                    var p2 = picker.SelectedProduct;
                    _items.Add(new PurchaseCartItem { ProductId = p2.ProductId, Barcode = p2.Barcode, NameAr = p2.NameAr, UnitName = p2.PurchaseUnitName, PurchasePrice = p2.PurchasePrice, Qty = 1, Discount = 0, LineTotal = p2.PurchasePrice });
                    RefreshGrid(); RecalcTotals();
                }
            }
            _txtBarcodeSearch.Clear();
        }
    }

    private void RefreshGrid()
    {
        _dgvItems.Rows.Clear();
        int n = 1;
        foreach (var item in _items)
            _dgvItems.Rows.Add(n++, item.Barcode, item.NameAr, item.UnitName, $"{item.PurchasePrice:N2}", item.Qty, $"{item.Discount:N2}", $"{item.LineTotal:N2}");
    }

    private void DgvItems_CellEndEdit(object? s, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.RowIndex >= _items.Count) return;
        var item = _items[e.RowIndex];
        var col  = _dgvItems.Columns[e.ColumnIndex].Name;
        var val  = _dgvItems.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString() ?? "0";

        switch (col)
        {
            case "PurchPrice": if (decimal.TryParse(val, out var p) && p > 0) item.PurchasePrice = p; break;
            case "Qty":        if (decimal.TryParse(val, out var q) && q > 0) item.Qty           = q; break;
            case "Discount":   if (decimal.TryParse(val, out var d))          item.Discount       = d; break;
        }
        item.LineTotal = (item.PurchasePrice - item.Discount) * item.Qty;
        RefreshGrid();
        RecalcTotals();
    }

    private void RecalcTotals()
    {
        var subtotal = _items.Sum(i => i.PurchasePrice * i.Qty);
        var itemDisc = _items.Sum(i => i.Discount * i.Qty);
        decimal.TryParse(_txtDiscount.Text, out var discPct);
        var afterItemDisc = subtotal - itemDisc;
        var headerDisc    = afterItemDisc * discPct / 100;
        var total         = afterItemDisc - headerDisc;

        _lblSubtotal.Text = $"{subtotal:N2}";
        _lblDiscount.Text = $"{itemDisc + headerDisc:N2}";
        _lblTotal.Text    = $"{total:N2}";
    }

    private void SaveInvoice(bool approve)
    {
        if (_cmbSupplier.SelectedItem is not ComboItem supplier || supplier.Id == 0)
        {
            MessageBox.Show("يرجى اختيار المورد", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
        }
        if (_items.Count == 0)
        {
            MessageBox.Show("أضف أصناف أولاً", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
        }

        decimal.TryParse(_txtDiscount.Text, out var discPct);
        decimal.TryParse(_txtPaid.Text,     out var paid);

        var cmd = _invoiceId.HasValue
            ? (object)new UpdatePurchaseInvoiceCommand(
                _invoiceId.Value, supplier.Id, _dtDate.Value,
                _txtRefNo.Text.Trim(), _txtNotes.Text.Trim(),
                _items.Select(i => new PurchaseItemDto(i.ProductId, i.Qty, i.PurchasePrice, i.Discount)).ToList(),
                discPct, paid, approve, UserSession.Current.UserId)
            : new CreatePurchaseInvoiceCommand(
                UserSession.Current.BranchId, UserSession.Current.WarehouseId,
                supplier.Id, _dtDate.Value, _txtRefNo.Text.Trim(), _txtNotes.Text.Trim(),
                _items.Select(i => new PurchaseItemDto(i.ProductId, i.Qty, i.PurchasePrice, i.Discount)).ToList(),
                discPct, paid, approve, UserSession.Current.UserId);

        Task.Run(async () =>
        {
            var result = cmd is UpdatePurchaseInvoiceCommand uc
                ? await _mediator.Send(uc)
                : await _mediator.Send((CreatePurchaseInvoiceCommand)cmd);

            Invoke(() =>
            {
                if (result.IsSuccess) { DialogResult = DialogResult.OK; Close(); }
                else MessageBox.Show(result.Error, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        });
    }
}

// ── Purchase return dialog ─────────────────────────────────────────
public sealed class PurchaseReturnDialog : Form
{
    private readonly IMediator _mediator;
    private readonly int       _invoiceId;
    private DataGridView _dgv = null!;

    public PurchaseReturnDialog(IMediator mediator, int invoiceId)
    {
        _mediator  = mediator;
        _invoiceId = invoiceId;
        InitializeComponent();
        LoadItems();
    }

    private void InitializeComponent()
    {
        Text = "مرتجع مشتريات"; Size = new Size(680, 460);
        FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false;
        StartPosition = FormStartPosition.CenterParent; BackColor = AppTheme.BgContent;
        RightToLeft = RightToLeft.Yes; RightToLeftLayout = true;

        _dgv = new DataGridView { Dock = DockStyle.Fill };
        AppTheme.StyleDataGrid(_dgv);
        _dgv.ReadOnly = false; _dgv.EditMode = DataGridViewEditMode.EditOnEnter;
        _dgv.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "ItemId",      Visible = false },
            new DataGridViewTextBoxColumn { Name = "NameAr",      HeaderText = "الصنف",             FillWeight = 40 },
            new DataGridViewTextBoxColumn { Name = "OrigQty",     HeaderText = "الكمية الأصلية",    Width = 110, ReadOnly = true },
            new DataGridViewTextBoxColumn { Name = "ReturnQty",   HeaderText = "كمية المرتجع",      Width = 110 },
            new DataGridViewTextBoxColumn { Name = "Price",       HeaderText = "السعر",              Width = 90,  ReadOnly = true }
        );

        var pnlBtns = new Panel { Dock = DockStyle.Bottom, Height = 52, BackColor = AppTheme.BgCard, Padding = new Padding(12, 8, 12, 8) };
        var btnOk = new Button { Text = "✔ تأكيد المرتجع", Dock = DockStyle.Right, Width = 150, Height = 34, FlatStyle = FlatStyle.Flat, BackColor = AppTheme.AccentOrange, ForeColor = Color.White, Font = AppTheme.FontBodyBold, Cursor = Cursors.Hand };
        btnOk.FlatAppearance.BorderSize = 0; btnOk.Click += (_, _) => DoReturn();
        var btnCancel = new Button { Text = "إلغاء", Dock = DockStyle.Left, Width = 90, Height = 34, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = AppTheme.TextSecondary, Font = AppTheme.FontBody, Cursor = Cursors.Hand };
        btnCancel.FlatAppearance.BorderColor = AppTheme.Border;
        btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        pnlBtns.Controls.AddRange([btnOk, btnCancel]);
        Controls.AddRange([pnlBtns, _dgv]);
    }

    private void LoadItems()
    {
        Task.Run(async () =>
        {
            var result = await _mediator.Send(new GetPurchaseInvoiceDetailQuery(_invoiceId));
            Invoke(() =>
            {
                if (!result.IsSuccess || result.Value == null) return;
                _dgv.Rows.Clear();
                foreach (var item in result.Value.Items)
                    _dgv.Rows.Add(item.InvoiceItemId, item.NameAr, item.Qty, item.Qty, $"{item.Price:N2}");
            });
        });
    }

    private void DoReturn()
    {
        var items = new List<(int itemId, decimal qty)>();
        foreach (DataGridViewRow row in _dgv.Rows)
        {
            if (row.IsNewRow) continue;
            if (decimal.TryParse(row.Cells["ReturnQty"].Value?.ToString(), out var qty) && qty > 0)
                items.Add(((int)row.Cells["ItemId"].Value, qty));
        }
        if (items.Count == 0) { MessageBox.Show("يرجى تحديد كميات المرتجع", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

        Task.Run(async () =>
        {
            var result = await _mediator.Send(new CreatePurchaseReturnCommand(_invoiceId, items, UserSession.Current.UserId));
            Invoke(() =>
            {
                if (result.IsSuccess) { MessageBox.Show("تم تسجيل المرتجع", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information); DialogResult = DialogResult.OK; Close(); }
                else MessageBox.Show(result.Error, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        });
    }
}

// ── Product picker dialog ──────────────────────────────────────────
public sealed class ProductPickerDialog : Form
{
    private DataGridView _dgv = null!;
    public ProductSearchDto? SelectedProduct { get; private set; }

    public ProductPickerDialog(List<ProductSearchDto> products)
    {
        Text = "اختر صنف"; Size = new Size(600, 380);
        FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false;
        StartPosition = FormStartPosition.CenterParent; BackColor = AppTheme.BgContent;
        RightToLeft = RightToLeft.Yes; RightToLeftLayout = true;

        _dgv = new DataGridView { Dock = DockStyle.Fill };
        AppTheme.StyleDataGrid(_dgv);
        _dgv.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "Idx",      Visible = false },
            new DataGridViewTextBoxColumn { Name = "Barcode",  HeaderText = "باركود",   Width = 110 },
            new DataGridViewTextBoxColumn { Name = "NameAr",   HeaderText = "الصنف",    FillWeight = 50 },
            new DataGridViewTextBoxColumn { Name = "Price",    HeaderText = "السعر",    Width = 90 },
            new DataGridViewTextBoxColumn { Name = "Stock",    HeaderText = "المخزون",  Width = 80 }
        );

        for (int i = 0; i < products.Count; i++)
            _dgv.Rows.Add(i, products[i].Barcode, products[i].NameAr, $"{products[i].SalePrice:N2}", products[i].Stock);

        _dgv.CellDoubleClick += (_, e) =>
        {
            if (e.RowIndex < 0) return;
            SelectedProduct = products[(int)_dgv.Rows[e.RowIndex].Cells["Idx"].Value];
            DialogResult = DialogResult.OK; Close();
        };

        var btnCancel = new Button { Text = "إلغاء", Dock = DockStyle.Bottom, Height = 40, FlatStyle = FlatStyle.Flat, BackColor = AppTheme.BgContent, ForeColor = AppTheme.TextSecondary, Font = AppTheme.FontBody, Cursor = Cursors.Hand };
        btnCancel.FlatAppearance.BorderSize = 0;
        btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        Controls.AddRange([btnCancel, _dgv]);
    }
}

// ── Cart item models ───────────────────────────────────────────────
public class PurchaseCartItem
{
    public int     ProductId     { get; set; }
    public string  Barcode       { get; set; } = string.Empty;
    public string  NameAr        { get; set; } = string.Empty;
    public string  UnitName      { get; set; } = string.Empty;
    public decimal PurchasePrice { get; set; }
    public decimal Qty           { get; set; }
    public decimal Discount      { get; set; }
    public decimal LineTotal     { get; set; }
}
