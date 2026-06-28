using MediatR;
using CorePOS.WinForms.Theme;
using CorePOS.WinForms.Infrastructure;
using CorePOS.Application.Features.Inventory.Queries;
using CorePOS.Application.Features.Inventory.Commands;

namespace CorePOS.WinForms.Forms.Inventory;

/// <summary>
/// Inventory management screen with 4 tabs:
/// 1. Stock View      — current stock per warehouse
/// 2. Inventory Count — full/partial count
/// 3. Transfers       — warehouse or branch transfers
/// 4. Adjustments     — increase / decrease
/// </summary>
public sealed class InventoryForm : BaseForm
{
    private TabControl _tabs = null!;

    // Tab 1 controls
    private DataGridView _dgvStock       = null!;
    private TextBox      _txtStockSearch = null!;
    private ComboBox     _cmbWarehouse   = null!;
    private Label        _lblStockCount  = null!;

    // Tab 2 controls
    private DataGridView _dgvCount       = null!;
    private TextBox      _txtCountSearch = null!;
    private RadioButton  _rbFull         = null!, _rbPartial = null!;
    private ComboBox     _cmbCountWH     = null!;

    // Tab 3 controls
    private DataGridView _dgvTransfer = null!;
    private ComboBox     _cmbFromWH   = null!, _cmbToWH = null!;
    private TextBox      _txtTransferNotes = null!;

    // Tab 4 controls
    private DataGridView _dgvAdj       = null!;
    private ComboBox     _cmbAdjType   = null!;
    private TextBox      _txtAdjReason = null!;

    public InventoryForm(IMediator mediator) : base(mediator)
    {
        Text      = "إدارة المخزون";
        BackColor = AppTheme.BgContent;
        InitializeComponent();
        LoadWarehouses();
        LoadStock();
    }

    private void InitializeComponent()
    {
        _tabs = new TabControl { Dock = DockStyle.Fill, Font = AppTheme.FontBody };

        _tabs.TabPages.Add(BuildStockTab());
        _tabs.TabPages.Add(BuildCountTab());
        _tabs.TabPages.Add(BuildTransferTab());
        _tabs.TabPages.Add(BuildAdjustmentTab());

        _tabs.SelectedIndexChanged += (_, _) =>
        {
            switch (_tabs.SelectedIndex)
            {
                case 0: LoadStock();      break;
                case 2: LoadTransfers();  break;
                case 3: LoadAdjustments();break;
            }
        };

        Controls.Add(_tabs);
    }

    // ══════════════════════════════════════════════════════════════
    // TAB 1 — CURRENT STOCK
    // ══════════════════════════════════════════════════════════════
    private TabPage BuildStockTab()
    {
        var tab = new TabPage("عرض المخزون") { BackColor = AppTheme.BgContent };

        var pnlTop = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = AppTheme.BgCard, Padding = new Padding(8) };
        _txtStockSearch = new TextBox { Dock = DockStyle.Right, Width = 260, Font = AppTheme.FontBody, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "🔍 بحث باسم أو باركود..." };
        var debounce = new System.Windows.Forms.Timer { Interval = 350 };
        debounce.Tick += (_, _) => { debounce.Stop(); LoadStock(); };
        _txtStockSearch.TextChanged += (_, _) => { debounce.Stop(); debounce.Start(); };

        _cmbWarehouse = new ComboBox { Dock = DockStyle.Right, Width = 180, DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody };
        _cmbWarehouse.SelectedIndexChanged += (_, _) => LoadStock();

        var btnExport = new Button { Text = "📤 تصدير", Dock = DockStyle.Left, Width = 100, Height = 34, FlatStyle = FlatStyle.Flat, BackColor = AppTheme.AccentPurple, ForeColor = Color.White, Font = AppTheme.FontSmall, Cursor = Cursors.Hand };
        btnExport.FlatAppearance.BorderSize = 0;
        btnExport.Click += (_, _) => { /* Phase 10 */ ShowSuccess("التصدير قيد التطوير"); };

        pnlTop.Controls.AddRange([btnExport, _cmbWarehouse, _txtStockSearch]);

        var pnlSummary = new Panel { Dock = DockStyle.Bottom, Height = 34, BackColor = AppTheme.BgCard, Padding = new Padding(8, 0, 8, 0) };
        _lblStockCount = new Label { Dock = DockStyle.Right, AutoSize = false, Width = 400, TextAlign = ContentAlignment.MiddleRight, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary };
        pnlSummary.Controls.Add(_lblStockCount);

        _dgvStock = new DataGridView { Dock = DockStyle.Fill };
        AppTheme.StyleDataGrid(_dgvStock);
        _dgvStock.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "ProductId", Visible = false },
            new DataGridViewTextBoxColumn { Name = "Barcode",   HeaderText = "باركود",       Width = 110 },
            new DataGridViewTextBoxColumn { Name = "NameAr",    HeaderText = "الصنف",         FillWeight = 35 },
            new DataGridViewTextBoxColumn { Name = "Category",  HeaderText = "القسم",          FillWeight = 18 },
            new DataGridViewTextBoxColumn { Name = "Unit",      HeaderText = "الوحدة",         Width = 80 },
            new DataGridViewTextBoxColumn { Name = "Stock",     HeaderText = "الكمية",         Width = 80 },
            new DataGridViewTextBoxColumn { Name = "MinStock",  HeaderText = "الحد الأدنى",   Width = 90 },
            new DataGridViewTextBoxColumn { Name = "AvgCost",   HeaderText = "متوسط التكلفة", Width = 100 },
            new DataGridViewTextBoxColumn { Name = "LastCost",  HeaderText = "آخر تكلفة",     Width = 100 }
        );

        tab.Controls.AddRange([pnlSummary, _dgvStock, pnlTop]);
        return tab;
    }

    // ══════════════════════════════════════════════════════════════
    // TAB 2 — INVENTORY COUNT
    // ══════════════════════════════════════════════════════════════
    private TabPage BuildCountTab()
    {
        var tab = new TabPage("الجرد") { BackColor = AppTheme.BgContent };

        var pnlTop = new Panel { Dock = DockStyle.Top, Height = 96, BackColor = AppTheme.BgCard, Padding = new Padding(12, 8, 12, 8) };

        var row1 = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = AppTheme.BgCard };
        _rbFull    = new RadioButton { Text = "جرد كامل",   Checked = true, AutoSize = true, Dock = DockStyle.Right, Font = AppTheme.FontBody };
        _rbPartial = new RadioButton { Text = "جرد جزئي",   AutoSize = true, Dock = DockStyle.Right, Font = AppTheme.FontBody };
        _cmbCountWH = new ComboBox { Dock = DockStyle.Right, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody };
        var lblWH = new Label { Text = "المستودع:", Dock = DockStyle.Right, AutoSize = false, Width = 80, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel, TextAlign = ContentAlignment.MiddleRight };
        row1.Controls.AddRange([_rbFull, _rbPartial, lblWH, _cmbCountWH]);

        var row2 = new Panel { Dock = DockStyle.Bottom, Height = 44, BackColor = AppTheme.BgCard };
        _txtCountSearch = new TextBox { Dock = DockStyle.Right, Width = 260, Font = AppTheme.FontBody, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "بحث..." };
        _txtCountSearch.TextChanged += (_, _) => LoadCountItems();

        var btnStartCount = new Button { Text = "▶ بدء الجرد", Dock = DockStyle.Left, Width = 120, Height = 34, FlatStyle = FlatStyle.Flat, BackColor = AppTheme.AccentBlue, ForeColor = Color.White, Font = AppTheme.FontBodyBold, Cursor = Cursors.Hand };
        btnStartCount.FlatAppearance.BorderSize = 0;
        btnStartCount.Click += (_, _) => LoadCountItems();

        var btnApproveCount = new Button { Text = "✔ اعتماد الجرد", Dock = DockStyle.Left, Width = 140, Height = 34, FlatStyle = FlatStyle.Flat, BackColor = AppTheme.AccentGreen, ForeColor = Color.White, Font = AppTheme.FontBodyBold, Cursor = Cursors.Hand, Margin = new Padding(8, 0, 0, 0) };
        btnApproveCount.FlatAppearance.BorderSize = 0;
        btnApproveCount.Click += (_, _) => ApproveInventoryCount();

        row2.Controls.AddRange([_txtCountSearch, btnStartCount, btnApproveCount]);
        pnlTop.Controls.AddRange([row2, row1]);

        _dgvCount = new DataGridView { Dock = DockStyle.Fill };
        AppTheme.StyleDataGrid(_dgvCount);
        _dgvCount.ReadOnly = false;
        _dgvCount.EditMode = DataGridViewEditMode.EditOnEnter;
        _dgvCount.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "ProductId",   Visible = false },
            new DataGridViewTextBoxColumn { Name = "Barcode",     HeaderText = "باركود",           Width = 110, ReadOnly = true },
            new DataGridViewTextBoxColumn { Name = "NameAr",      HeaderText = "الصنف",             FillWeight = 40, ReadOnly = true },
            new DataGridViewTextBoxColumn { Name = "SystemQty",   HeaderText = "كمية النظام",       Width = 100, ReadOnly = true },
            new DataGridViewTextBoxColumn { Name = "ActualQty",   HeaderText = "الكمية الفعلية",   Width = 110 },
            new DataGridViewTextBoxColumn { Name = "Difference",  HeaderText = "الفرق",             Width = 80,  ReadOnly = true }
        );
        _dgvCount.CellEndEdit += (_, e) =>
        {
            if (_dgvCount.Columns[e.ColumnIndex].Name != "ActualQty" || e.RowIndex < 0) return;
            var row = _dgvCount.Rows[e.RowIndex];
            if (decimal.TryParse(row.Cells["SystemQty"].Value?.ToString(),  out var sys) &&
                decimal.TryParse(row.Cells["ActualQty"].Value?.ToString(), out var act))
            {
                row.Cells["Difference"].Value = $"{act - sys:N2}";
                row.DefaultCellStyle.ForeColor = act < sys ? AppTheme.AccentRed
                                               : act > sys ? AppTheme.AccentGreen
                                               : AppTheme.TextPrimary;
            }
        };

        tab.Controls.AddRange([_dgvCount, pnlTop]);
        return tab;
    }

    // ══════════════════════════════════════════════════════════════
    // TAB 3 — TRANSFERS
    // ══════════════════════════════════════════════════════════════
    private TabPage BuildTransferTab()
    {
        var tab = new TabPage("التحويلات") { BackColor = AppTheme.BgContent };

        var pnlForm = new Panel { Dock = DockStyle.Top, Height = 120, BackColor = AppTheme.BgCard, Padding = new Padding(16, 12, 16, 8) };

        int x = 16, y = 10;
        void AddLbl(string t) { pnlForm.Controls.Add(new Label { Text = t, Location = new Point(x, y), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel }); }
        void AddLblAt(string t, int px, int py) { pnlForm.Controls.Add(new Label { Text = t, Location = new Point(px, py), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel }); }

        AddLbl("من المستودع:");
        _cmbFromWH = new ComboBox { Location = new Point(x, y + 20), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody };
        pnlForm.Controls.Add(_cmbFromWH);

        AddLblAt("إلى المستودع:", 260, y);
        _cmbToWH = new ComboBox { Location = new Point(260, y + 20), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody };
        pnlForm.Controls.Add(_cmbToWH);

        AddLblAt("ملاحظات:", 500, y);
        _txtTransferNotes = new TextBox { Location = new Point(500, y + 20), Width = 260, Height = AppTheme.InputHeight, Font = AppTheme.FontBody, BorderStyle = BorderStyle.FixedSingle };
        pnlForm.Controls.Add(_txtTransferNotes);

        var btnAddItem = new Button { Text = "➕ إضافة صنف", Location = new Point(16, y + 68), Width = 130, Height = 34, FlatStyle = FlatStyle.Flat, BackColor = AppTheme.AccentBlue, ForeColor = Color.White, Font = AppTheme.FontSmall, Cursor = Cursors.Hand };
        btnAddItem.FlatAppearance.BorderSize = 0;
        btnAddItem.Click += (_, _) => AddTransferItem();

        var btnSaveTransfer = new Button { Text = "💾 حفظ التحويل", Location = new Point(160, y + 68), Width = 140, Height = 34, FlatStyle = FlatStyle.Flat, BackColor = AppTheme.AccentGreen, ForeColor = Color.White, Font = AppTheme.FontBodyBold, Cursor = Cursors.Hand };
        btnSaveTransfer.FlatAppearance.BorderSize = 0;
        btnSaveTransfer.Click += (_, _) => SaveTransfer();

        pnlForm.Controls.AddRange([btnAddItem, btnSaveTransfer]);

        _dgvTransfer = new DataGridView { Dock = DockStyle.Fill };
        AppTheme.StyleDataGrid(_dgvTransfer);
        _dgvTransfer.ReadOnly = false; _dgvTransfer.EditMode = DataGridViewEditMode.EditOnEnter;
        _dgvTransfer.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "ProductId", Visible = false },
            new DataGridViewTextBoxColumn { Name = "NameAr",    HeaderText = "الصنف",           FillWeight = 50, ReadOnly = true },
            new DataGridViewTextBoxColumn { Name = "AvailQty",  HeaderText = "المتاح في المصدر",Width = 130, ReadOnly = true },
            new DataGridViewTextBoxColumn { Name = "TransferQty",HeaderText = "كمية التحويل",  Width = 120 }
        );

        tab.Controls.AddRange([_dgvTransfer, pnlForm]);
        return tab;
    }

    // ══════════════════════════════════════════════════════════════
    // TAB 4 — ADJUSTMENTS
    // ══════════════════════════════════════════════════════════════
    private TabPage BuildAdjustmentTab()
    {
        var tab = new TabPage("التسويات") { BackColor = AppTheme.BgContent };

        var pnlTop2 = new Panel { Dock = DockStyle.Top, Height = 96, BackColor = AppTheme.BgCard, Padding = new Padding(12, 8, 12, 8) };

        var row1 = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = AppTheme.BgCard };
        _cmbAdjType = new ComboBox { Dock = DockStyle.Right, Width = 180, DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody };
        _cmbAdjType.Items.AddRange(["زيادة مخزون", "نقص مخزون"]);
        _cmbAdjType.SelectedIndex = 0;
        var lblAdjType = new Label { Text = "نوع التسوية:", Dock = DockStyle.Right, AutoSize = false, Width = 100, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel, TextAlign = ContentAlignment.MiddleRight };
        row1.Controls.AddRange([lblAdjType, _cmbAdjType]);

        var row2 = new Panel { Dock = DockStyle.Bottom, Height = 44, BackColor = AppTheme.BgCard };
        _txtAdjReason = new TextBox { Dock = DockStyle.Right, Width = 320, Font = AppTheme.FontBody, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "سبب التسوية..." };
        var btnAddAdj = new Button { Text = "➕ إضافة صنف", Dock = DockStyle.Left, Width = 130, Height = 34, FlatStyle = FlatStyle.Flat, BackColor = AppTheme.AccentBlue, ForeColor = Color.White, Font = AppTheme.FontSmall, Cursor = Cursors.Hand };
        btnAddAdj.FlatAppearance.BorderSize = 0; btnAddAdj.Click += (_, _) => AddAdjItem();
        var btnSaveAdj = new Button { Text = "💾 حفظ التسوية", Dock = DockStyle.Left, Width = 140, Height = 34, FlatStyle = FlatStyle.Flat, BackColor = AppTheme.AccentOrange, ForeColor = Color.White, Font = AppTheme.FontBodyBold, Cursor = Cursors.Hand, Margin = new Padding(8, 0, 0, 0) };
        btnSaveAdj.FlatAppearance.BorderSize = 0; btnSaveAdj.Click += (_, _) => SaveAdjustment();
        row2.Controls.AddRange([_txtAdjReason, btnAddAdj, btnSaveAdj]);

        pnlTop2.Controls.AddRange([row2, row1]);

        _dgvAdj = new DataGridView { Dock = DockStyle.Fill };
        AppTheme.StyleDataGrid(_dgvAdj);
        _dgvAdj.ReadOnly = false; _dgvAdj.EditMode = DataGridViewEditMode.EditOnEnter;
        _dgvAdj.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "ProductId", Visible = false },
            new DataGridViewTextBoxColumn { Name = "NameAr",    HeaderText = "الصنف",         FillWeight = 45, ReadOnly = true },
            new DataGridViewTextBoxColumn { Name = "CurrentQty",HeaderText = "الكمية الحالية",Width = 120, ReadOnly = true },
            new DataGridViewTextBoxColumn { Name = "AdjQty",    HeaderText = "كمية التسوية",  Width = 120 },
            new DataGridViewTextBoxColumn { Name = "NewQty",    HeaderText = "الكمية الجديدة",Width = 120, ReadOnly = true }
        );
        _dgvAdj.CellEndEdit += (_, e) =>
        {
            if (_dgvAdj.Columns[e.ColumnIndex].Name != "AdjQty" || e.RowIndex < 0) return;
            var row = _dgvAdj.Rows[e.RowIndex];
            if (!decimal.TryParse(row.Cells["CurrentQty"].Value?.ToString(), out var cur)) return;
            if (!decimal.TryParse(row.Cells["AdjQty"].Value?.ToString(),     out var adj)) return;
            var newQty = _cmbAdjType.SelectedIndex == 0 ? cur + adj : cur - adj;
            row.Cells["NewQty"].Value = $"{newQty:N2}";
            row.DefaultCellStyle.ForeColor = newQty < 0 ? AppTheme.AccentRed : AppTheme.TextPrimary;
        };

        tab.Controls.AddRange([_dgvAdj, pnlTop2]);
        return tab;
    }

    // ══════════════════════════════════════════════════════════════
    // DATA LOADING
    // ══════════════════════════════════════════════════════════════
    private void LoadWarehouses()
    {
        Task.Run(async () =>
        {
            var result = await _mediator.Send(new GetWarehousesQuery(UserSession.Current.BranchId));
            InvokeOnUI(() =>
            {
                if (!result.IsSuccess || result.Value == null) return;
                _cmbWarehouse.Items.Clear();
                _cmbCountWH.Items.Clear();
                _cmbFromWH.Items.Clear();
                _cmbToWH.Items.Clear();
                _cmbWarehouse.Items.Add(new ComboItem(0, "كل المستودعات"));
                foreach (var w in result.Value)
                {
                    var item = new ComboItem(w.WarehouseId, w.Name);
                    _cmbWarehouse.Items.Add(item);
                    _cmbCountWH.Items.Add(new ComboItem(w.WarehouseId, w.Name));
                    _cmbFromWH.Items.Add(new ComboItem(w.WarehouseId, w.Name));
                    _cmbToWH.Items.Add(new ComboItem(w.WarehouseId, w.Name));
                }
                _cmbWarehouse.SelectedIndex = 0;
                if (_cmbCountWH.Items.Count > 0) _cmbCountWH.SelectedIndex = 0;
                if (_cmbFromWH.Items.Count > 0)  _cmbFromWH.SelectedIndex  = 0;
                if (_cmbToWH.Items.Count > 1)    _cmbToWH.SelectedIndex    = 1;
            });
        });
    }

    private void LoadStock()
    {
        RunAsync(async () =>
        {
            var whId = (_cmbWarehouse.SelectedItem as ComboItem)?.Id;
            var q    = new GetStockListQuery(
                BranchId:    UserSession.Current.BranchId,
                WarehouseId: whId == 0 ? null : whId,
                Search:      _txtStockSearch.Text.Trim());
            var result = await _mediator.Send(q);
            InvokeOnUI(() =>
            {
                _dgvStock.Rows.Clear();
                if (!result.IsSuccess || result.Value == null) return;
                int lowStock = 0;
                foreach (var s in result.Value)
                {
                    var rowIdx = _dgvStock.Rows.Add(
                        s.ProductId, s.Barcode, s.NameAr, s.CategoryName,
                        s.UnitName, s.Quantity, s.MinStock,
                        $"{s.AverageCost:N2}", $"{s.LastCost:N2}");
                    if (s.Quantity <= s.MinStock)
                    {
                        _dgvStock.Rows[rowIdx].DefaultCellStyle.ForeColor = AppTheme.AccentRed;
                        lowStock++;
                    }
                }
                _lblStockCount.Text = $"الأصناف: {result.Value.Count:N0}  |  منخفضة المخزون: {lowStock}";
            });
        }, "جاري تحميل المخزون...");
    }

    private void LoadCountItems()
    {
        var whItem = _cmbCountWH.SelectedItem as ComboItem;
        if (whItem == null) return;
        RunAsync(async () =>
        {
            var q      = new GetStockListQuery(UserSession.Current.BranchId, whItem.Id, _txtCountSearch.Text.Trim());
            var result = await _mediator.Send(q);
            InvokeOnUI(() =>
            {
                _dgvCount.Rows.Clear();
                if (!result.IsSuccess || result.Value == null) return;
                foreach (var s in result.Value)
                    _dgvCount.Rows.Add(s.ProductId, s.Barcode, s.NameAr, s.Quantity, s.Quantity, "0");
            });
        }, "جاري تحميل بنود الجرد...");
    }

    private void ApproveInventoryCount()
    {
        if (_dgvCount.Rows.Count == 0) return;
        if (!Confirm("هل تريد اعتماد الجرد وتحديث المخزون؟")) return;

        var adjustments = new List<(int productId, decimal actualQty)>();
        foreach (DataGridViewRow row in _dgvCount.Rows)
        {
            if (row.IsNewRow) continue;
            if (row.Cells["ProductId"].Value is int pid &&
                decimal.TryParse(row.Cells["ActualQty"].Value?.ToString(), out var qty))
                adjustments.Add((pid, qty));
        }

        var whItem = _cmbCountWH.SelectedItem as ComboItem;
        if (whItem == null) return;

        RunAsync(async () =>
        {
            var cmd    = new ApproveInventoryCountCommand(whItem.Id, adjustments, UserSession.Current.UserId);
            var result = await _mediator.Send(cmd);
            InvokeOnUI(() =>
            {
                if (result.IsSuccess) { ShowSuccess("تم اعتماد الجرد بنجاح"); _dgvCount.Rows.Clear(); LoadStock(); }
                else ShowError(result.Error);
            });
        }, "جاري اعتماد الجرد...");
    }

    private void LoadTransfers()  { /* Load recent transfers history if needed */ }
    private void LoadAdjustments(){ /* Load recent adjustments history if needed */ }

    private void AddTransferItem()
    {
        using var dlg = new StockItemPickerDialog(_mediator, (_cmbFromWH.SelectedItem as ComboItem)?.Id ?? 0);
        if (dlg.ShowDialog() == DialogResult.OK && dlg.SelectedItem != null)
        {
            _dgvTransfer.Rows.Add(dlg.SelectedItem.ProductId, dlg.SelectedItem.NameAr, dlg.SelectedItem.Quantity, "1");
        }
    }

    private void SaveTransfer()
    {
        var fromWH = _cmbFromWH.SelectedItem as ComboItem;
        var toWH   = _cmbToWH.SelectedItem   as ComboItem;
        if (fromWH == null || toWH == null || fromWH.Id == toWH.Id)
        {
            ShowError("يجب اختيار مستودعين مختلفين"); return;
        }

        var items = new List<(int productId, decimal qty)>();
        foreach (DataGridViewRow row in _dgvTransfer.Rows)
        {
            if (row.IsNewRow) continue;
            if (row.Cells["ProductId"].Value is int pid &&
                decimal.TryParse(row.Cells["TransferQty"].Value?.ToString(), out var qty) && qty > 0)
                items.Add((pid, qty));
        }

        if (items.Count == 0) { ShowError("أضف أصناف للتحويل"); return; }

        RunAsync(async () =>
        {
            var cmd    = new CreateTransferCommand(fromWH.Id, toWH.Id, items, _txtTransferNotes.Text.Trim(), UserSession.Current.UserId);
            var result = await _mediator.Send(cmd);
            InvokeOnUI(() =>
            {
                if (result.IsSuccess) { ShowSuccess("تم حفظ التحويل"); _dgvTransfer.Rows.Clear(); }
                else ShowError(result.Error);
            });
        }, "جاري حفظ التحويل...");
    }

    private void AddAdjItem()
    {
        using var dlg = new StockItemPickerDialog(_mediator, (_cmbFromWH.SelectedItem as ComboItem)?.Id ?? UserSession.Current.WarehouseId);
        if (dlg.ShowDialog() == DialogResult.OK && dlg.SelectedItem != null)
            _dgvAdj.Rows.Add(dlg.SelectedItem.ProductId, dlg.SelectedItem.NameAr, dlg.SelectedItem.Quantity, "0", dlg.SelectedItem.Quantity.ToString("N2"));
    }

    private void SaveAdjustment()
    {
        var items = new List<(int productId, decimal adjQty)>();
        foreach (DataGridViewRow row in _dgvAdj.Rows)
        {
            if (row.IsNewRow) continue;
            if (row.Cells["ProductId"].Value is int pid &&
                decimal.TryParse(row.Cells["AdjQty"].Value?.ToString(), out var adj) && adj > 0)
                items.Add((pid, _cmbAdjType.SelectedIndex == 0 ? adj : -adj));
        }
        if (items.Count == 0) { ShowError("أضف أصناف للتسوية"); return; }
        if (string.IsNullOrWhiteSpace(_txtAdjReason.Text)) { ShowError("يرجى إدخال سبب التسوية"); return; }

        RunAsync(async () =>
        {
            var cmd    = new CreateAdjustmentCommand(UserSession.Current.WarehouseId, items, _txtAdjReason.Text.Trim(), UserSession.Current.UserId);
            var result = await _mediator.Send(cmd);
            InvokeOnUI(() =>
            {
                if (result.IsSuccess) { ShowSuccess("تم حفظ التسوية"); _dgvAdj.Rows.Clear(); LoadStock(); }
                else ShowError(result.Error);
            });
        }, "جاري حفظ التسوية...");
    }
}

// ── Stock item picker dialog ───────────────────────────────────────
public sealed class StockItemPickerDialog : Form
{
    private readonly IMediator _mediator;
    private readonly int       _warehouseId;
    private DataGridView _dgv     = null!;
    private TextBox      _txtSrch = null!;
    public StockItemDto? SelectedItem { get; private set; }

    public StockItemPickerDialog(IMediator mediator, int warehouseId)
    {
        _mediator    = mediator;
        _warehouseId = warehouseId;
        InitializeComponent();
        Load2(string.Empty);
    }

    private void InitializeComponent()
    {
        Text = "اختر صنف"; Size = new Size(600, 420);
        FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false;
        StartPosition = FormStartPosition.CenterParent; BackColor = AppTheme.BgContent;
        RightToLeft = RightToLeft.Yes; RightToLeftLayout = true;

        _txtSrch = new TextBox { Dock = DockStyle.Top, Height = AppTheme.InputHeight, Font = AppTheme.FontBody, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "بحث..." };
        var d = new System.Windows.Forms.Timer { Interval = 300 };
        d.Tick += (_, _) => { d.Stop(); Load2(_txtSrch.Text.Trim()); };
        _txtSrch.TextChanged += (_, _) => { d.Stop(); d.Start(); };

        _dgv = new DataGridView { Dock = DockStyle.Fill };
        AppTheme.StyleDataGrid(_dgv);
        _dgv.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "ProductId", Visible = false },
            new DataGridViewTextBoxColumn { Name = "NameAr",    HeaderText = "الصنف",   FillWeight = 50 },
            new DataGridViewTextBoxColumn { Name = "Qty",       HeaderText = "الكمية",  Width = 90 }
        );
        _dgv.CellDoubleClick += (_, e) =>
        {
            if (e.RowIndex < 0) return;
            SelectedItem = new StockItemDto
            {
                ProductId = (int)_dgv.Rows[e.RowIndex].Cells["ProductId"].Value,
                NameAr    = _dgv.Rows[e.RowIndex].Cells["NameAr"].Value?.ToString() ?? string.Empty,
                Quantity  = decimal.TryParse(_dgv.Rows[e.RowIndex].Cells["Qty"].Value?.ToString(), out var q) ? q : 0
            };
            DialogResult = DialogResult.OK; Close();
        };

        var btnCancel = new Button { Text = "إلغاء", Dock = DockStyle.Bottom, Height = 40, FlatStyle = FlatStyle.Flat, BackColor = AppTheme.BgContent, ForeColor = AppTheme.TextSecondary, Font = AppTheme.FontBody };
        btnCancel.FlatAppearance.BorderSize = 0; btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        Controls.AddRange([btnCancel, _dgv, _txtSrch]);
    }

    private void Load2(string search)
    {
        Task.Run(async () =>
        {
            var result = await _mediator.Send(new GetStockListQuery(0, _warehouseId, search));
            Invoke(() =>
            {
                _dgv.Rows.Clear();
                if (!result.IsSuccess || result.Value == null) return;
                foreach (var s in result.Value)
                    _dgv.Rows.Add(s.ProductId, s.NameAr, s.Quantity);
            });
        });
    }
}

public class StockItemDto { public int ProductId { get; set; } public string NameAr { get; set; } = string.Empty; public decimal Quantity { get; set; } }
