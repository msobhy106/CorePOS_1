using MediatR;
using CorePOS.WinForms.Theme;
using CorePOS.WinForms.Infrastructure;
using CorePOS.Application.Features.Products.Queries;
using CorePOS.Application.Features.Products.Commands;
using CorePOS.Application.Features.Categories.Queries;

namespace CorePOS.WinForms.Forms.MasterData;

/// <summary>
/// Products master data screen.
/// Left: category tree filter. Right: products grid with full CRUD.
/// </summary>
public sealed class ProductsForm : BaseForm
{
    private DataGridView _dgv      = null!;
    private TextBox  _txtSearch    = null!;
    private TreeView _tvCategories = null!;
    private Label    _lblCount     = null!;
    private System.Windows.Forms.Timer _debounce = null!;
    private int? _selectedCategoryId = null;

    public ProductsForm(IMediator mediator) : base(mediator)
    {
        InitializeComponent();
        LoadCategories();
        LoadProducts();
    }

    private void InitializeComponent()
    {
        Text      = "الأصناف";
        BackColor = AppTheme.BgContent;

        // ── Main split: category tree (left) | products (right) ───
        var split = new SplitContainer
        {
            Dock = DockStyle.Fill, Orientation = Orientation.Vertical,
            SplitterDistance = 200, BorderStyle = BorderStyle.None,
            Panel1MinSize = 160, Panel2MinSize = 400, IsSplitterFixed = false
        };

        // ── Category tree panel ────────────────────────────────────
        split.Panel1.BackColor = AppTheme.BgCard;
        split.Panel1.Padding   = new Padding(0);

        var lblCatTitle = new Label
        {
            Text = "الأقسام", Font = AppTheme.FontH2, ForeColor = AppTheme.TextPrimary,
            Dock = DockStyle.Top, Height = 40, TextAlign = ContentAlignment.MiddleCenter,
            BackColor = AppTheme.BgCard
        };

        _tvCategories = new TreeView
        {
            Dock      = DockStyle.Fill,
            Font      = AppTheme.FontBody,
            BackColor = AppTheme.BgCard,
            BorderStyle = BorderStyle.None,
            ShowLines = true,
            ShowPlusMinus = true,
            HideSelection = false
        };
        _tvCategories.AfterSelect += (_, e) =>
        {
            _selectedCategoryId = e.Node.Tag as int?;
            LoadProducts();
        };

        var btnAllCats = new Button
        {
            Text = "عرض الكل", Dock = DockStyle.Bottom, Height = 34,
            FlatStyle = FlatStyle.Flat, BackColor = AppTheme.BgContent,
            ForeColor = AppTheme.AccentBlue, Font = AppTheme.FontSmall, Cursor = Cursors.Hand
        };
        btnAllCats.FlatAppearance.BorderSize = 0;
        btnAllCats.Click += (_, _) => { _selectedCategoryId = null; _tvCategories.SelectedNode = null; LoadProducts(); };

        split.Panel1.Controls.AddRange([btnAllCats, _tvCategories, lblCatTitle]);

        // ── Products panel ─────────────────────────────────────────
        split.Panel2.BackColor = AppTheme.BgContent;
        split.Panel2.Padding   = new Padding(8, 8, 0, 8);

        // Toolbar
        var pnlToolbar = new Panel
        {
            Dock = DockStyle.Top, Height = 52, BackColor = AppTheme.BgCard,
            Padding = new Padding(8)
        };

        _txtSearch = new TextBox
        {
            Dock = DockStyle.Right, Width = 240,
            Font = AppTheme.FontBody, BorderStyle = BorderStyle.FixedSingle,
            PlaceholderText = "🔍 بحث باسم أو باركود..."
        };
        _debounce = new System.Windows.Forms.Timer { Interval = 350 };
        _debounce.Tick += (_, _) => { _debounce.Stop(); LoadProducts(); };
        _txtSearch.TextChanged += (_, _) => { _debounce.Stop(); _debounce.Start(); };

        var flowBtns = new FlowLayoutPanel
        {
            Dock = DockStyle.Left, Width = 360, FlowDirection = FlowDirection.LeftToRight,
            BackColor = AppTheme.BgCard, WrapContents = false
        };

        Button MakeBtn(string text, Color color, EventHandler click)
        {
            var b = new Button
            {
                Text = text, Width = 100, Height = 34, FlatStyle = FlatStyle.Flat,
                BackColor = color, ForeColor = Color.White, Font = AppTheme.FontSmall,
                Cursor = Cursors.Hand, Margin = new Padding(0, 0, 6, 0)
            };
            b.FlatAppearance.BorderSize = 0; b.Click += click; return b;
        }

        var btnAdd    = MakeBtn("➕ إضافة",  AppTheme.AccentGreen,  (_, _) => ShowProductDialog(null));
        var btnEdit   = MakeBtn("✏ تعديل",   AppTheme.AccentBlue,   (_, _) => EditProduct());
        var btnDelete = MakeBtn("🗑 حذف",     AppTheme.AccentRed,    (_, _) => DeleteProduct());
        var btnStock  = MakeBtn("📦 مخزون",  AppTheme.AccentOrange, (_, _) => ViewStock());

        btnAdd.Visible    = CanAdd(Modules.Products);
        btnEdit.Visible   = CanEdit(Modules.Products);
        btnDelete.Visible = CanDelete(Modules.Products);

        flowBtns.Controls.AddRange([btnAdd, btnEdit, btnDelete, btnStock]);
        pnlToolbar.Controls.AddRange([_txtSearch, flowBtns]);

        // Summary
        var pnlSummary = new Panel
        {
            Dock = DockStyle.Bottom, Height = 34, BackColor = AppTheme.BgCard,
            Padding = new Padding(8, 0, 8, 0)
        };
        _lblCount = new Label
        {
            Text = "عدد الأصناف: 0", Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary,
            Dock = DockStyle.Right, TextAlign = ContentAlignment.MiddleRight, AutoSize = false, Width = 200
        };
        pnlSummary.Controls.Add(_lblCount);

        // Grid
        var pnlGrid = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.BgCard };
        _dgv = new DataGridView { Dock = DockStyle.Fill };
        AppTheme.StyleDataGrid(_dgv);
        _dgv.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "ProductId", Visible = false },
            new DataGridViewTextBoxColumn { Name = "Barcode",   HeaderText = "باركود",      Width = 110 },
            new DataGridViewTextBoxColumn { Name = "Code",      HeaderText = "الكود",        Width = 80 },
            new DataGridViewTextBoxColumn { Name = "NameAr",    HeaderText = "الاسم عربي",   FillWeight = 30 },
            new DataGridViewTextBoxColumn { Name = "NameEn",    HeaderText = "الاسم إنجليزي",FillWeight = 20 },
            new DataGridViewTextBoxColumn { Name = "Category",  HeaderText = "القسم",         FillWeight = 15 },
            new DataGridViewTextBoxColumn { Name = "SalePrice", HeaderText = "سعر البيع",    Width = 90 },
            new DataGridViewTextBoxColumn { Name = "Stock",     HeaderText = "المخزون",      Width = 80 },
            new DataGridViewTextBoxColumn { Name = "Unit",      HeaderText = "الوحدة",       Width = 70 },
            new DataGridViewTextBoxColumn { Name = "Status",    HeaderText = "الحالة",       Width = 70 }
        );
        _dgv.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) EditProduct(); };

        pnlGrid.Controls.Add(_dgv);
        split.Panel2.Controls.AddRange([pnlSummary, pnlGrid, pnlToolbar]);

        Controls.Add(split);
    }

    // ── Data Loading ──────────────────────────────────────────────
    private void LoadCategories()
    {
        Task.Run(async () =>
        {
            var result = await _mediator.Send(new GetCategoriesTreeQuery());
            InvokeOnUI(() =>
            {
                if (!result.IsSuccess || result.Value == null) return;
                _tvCategories.Nodes.Clear();
                foreach (var cat in result.Value)
                {
                    var node = new TreeNode(cat.Name) { Tag = cat.CategoryId };
                    foreach (var sub in cat.SubCategories)
                        node.Nodes.Add(new TreeNode(sub.Name) { Tag = sub.CategoryId });
                    _tvCategories.Nodes.Add(node);
                }
                _tvCategories.ExpandAll();
            });
        });
    }

    private void LoadProducts()
    {
        RunAsync(async () =>
        {
            var q = new GetProductsListQuery(
                Search:     _txtSearch.Text.Trim(),
                CategoryId: _selectedCategoryId,
                BranchId:   UserSession.Current.BranchId,
                PageSize:   500);

            var result = await _mediator.Send(q);
            InvokeOnUI(() =>
            {
                _dgv.Rows.Clear();
                if (!result.IsSuccess || result.Value == null) return;

                foreach (var p in result.Value.Items)
                {
                    var rowIdx = _dgv.Rows.Add(
                        p.ProductId, p.Barcode, p.ProductCode, p.NameAr, p.NameEn,
                        p.CategoryName, $"{p.SalePrice:N2}", p.Stock, p.SaleUnitName,
                        p.IsActive ? "نشط" : "موقوف"
                    );
                    // Color low stock rows
                    if (p.Stock <= p.MinStock)
                        _dgv.Rows[rowIdx].DefaultCellStyle.ForeColor = AppTheme.AccentRed;
                }

                _lblCount.Text = $"عدد الأصناف: {result.Value.TotalCount:N0}";
            });
        }, "جاري تحميل الأصناف...");
    }

    // ── Actions ───────────────────────────────────────────────────
    private int? GetSelectedProductId()
    {
        if (_dgv.CurrentRow == null) return null;
        return _dgv.CurrentRow.Cells["ProductId"].Value as int?;
    }

    private void ShowProductDialog(int? productId)
    {
        using var dlg = new ProductEditDialog(_mediator, productId);
        if (dlg.ShowDialog() == DialogResult.OK)
            LoadProducts();
    }

    private void EditProduct()
    {
        var id = GetSelectedProductId();
        if (id == null) return;
        ShowProductDialog(id);
    }

    private void DeleteProduct()
    {
        var id = GetSelectedProductId();
        if (id == null) return;
        if (!Confirm("هل تريد حذف هذا الصنف؟")) return;

        RunAsync(async () =>
        {
            var cmd    = new DeleteProductCommand(id.Value, UserSession.Current.UserId);
            var result = await _mediator.Send(cmd);
            InvokeOnUI(() => { if (result.IsSuccess) LoadProducts(); else ShowError(result.Error); });
        });
    }

    private void ViewStock()
    {
        var id = GetSelectedProductId();
        if (id == null) return;
        using var dlg = new ProductStockDialog(_mediator, id.Value);
        dlg.ShowDialog();
    }
}

// ════════════════════════════════════════════════════════════════════
// PRODUCT EDIT DIALOG (Add / Edit)
// ════════════════════════════════════════════════════════════════════
public sealed class ProductEditDialog : Form
{
    private readonly IMediator _mediator;
    private readonly int?      _productId;

    // Tab pages
    private TabControl _tabs = null!;

    // Basic info controls
    private TextBox _txtCode        = null!;
    private TextBox _txtBarcode     = null!;
    private TextBox _txtNameAr      = null!;
    private TextBox _txtNameEn      = null!;
    private ComboBox _cmbCategory   = null!;
    private ComboBox _cmbGroup      = null!;
    private ComboBox _cmbBaseUnit   = null!;
    private ComboBox _cmbSaleUnit   = null!;
    private ComboBox _cmbPurchUnit  = null!;
    private CheckBox _chkActive     = null!;

    // Price controls
    private TextBox _txtPurchasePrice = null!;
    private TextBox _txtSalePrice     = null!;
    private TextBox _txtWholesale     = null!;
    private TextBox _txtHalfWholesale = null!;
    private TextBox _txtSpecial       = null!;

    // Extra controls
    private TextBox _txtMinStock     = null!;
    private TextBox _txtReorderLevel = null!;
    private DateTimePicker _dtExpiry = null!;
    private CheckBox _chkHasExpiry   = null!;
    private TextBox _txtManufacturer = null!;
    private ComboBox _cmbSupplier    = null!;

    public ProductEditDialog(IMediator mediator, int? productId)
    {
        _mediator  = mediator;
        _productId = productId;
        InitializeComponent();
        LoadCombos();
        if (productId.HasValue) LoadProduct(productId.Value);
    }

    private void InitializeComponent()
    {
        Text            = _productId.HasValue ? "تعديل صنف" : "إضافة صنف جديد";
        Size            = new Size(680, 520);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        StartPosition   = FormStartPosition.CenterParent;
        BackColor       = AppTheme.BgContent;
        RightToLeft     = RightToLeft.Yes;
        RightToLeftLayout = true;

        _tabs = new TabControl { Dock = DockStyle.Fill, Font = AppTheme.FontBody };

        // ── Tab 1: Basic Info ──────────────────────────────────────
        var tabBasic = new TabPage("البيانات الأساسية") { BackColor = AppTheme.BgCard };
        var pnlBasic = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16), BackColor = AppTheme.BgCard };

        int y = 8;
        void AddField(string lbl, out TextBox tb, int width = 280, bool fullRow = false)
        {
            pnlBasic.Controls.Add(new Label
            {
                Text = lbl, Location = new Point(fullRow ? 16 : 320, y), AutoSize = true,
                Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel
            });
            y += 20;
            tb = new TextBox
            {
                Location = new Point(fullRow ? 16 : 320, y), Width = fullRow ? 600 : width,
                Height = AppTheme.InputHeight, Font = AppTheme.FontBody, BorderStyle = BorderStyle.FixedSingle
            };
            pnlBasic.Controls.Add(tb);
            if (!fullRow) y += 44;
        }
        void AddCombo(string lbl, out ComboBox cb, int x, int iy)
        {
            pnlBasic.Controls.Add(new Label
            {
                Text = lbl, Location = new Point(x, iy), AutoSize = true,
                Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel
            });
            cb = new ComboBox
            {
                Location = new Point(x, iy + 20), Width = 280,
                DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody
            };
            pnlBasic.Controls.Add(cb);
        }

        // Row 1: Code + Barcode side by side
        y = 8;
        pnlBasic.Controls.Add(new Label { Text = "الكود:", Location = new Point(16, y), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel });
        pnlBasic.Controls.Add(new Label { Text = "الباركود:", Location = new Point(320, y), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel });
        y += 20;
        _txtCode    = new TextBox { Location = new Point(16, y),  Width = 280, Height = AppTheme.InputHeight, Font = AppTheme.FontBody, BorderStyle = BorderStyle.FixedSingle };
        _txtBarcode = new TextBox { Location = new Point(320, y), Width = 280, Height = AppTheme.InputHeight, Font = AppTheme.FontBody, BorderStyle = BorderStyle.FixedSingle };
        pnlBasic.Controls.AddRange([_txtCode, _txtBarcode]);
        y += 44;

        // Row 2: Arabic name
        pnlBasic.Controls.Add(new Label { Text = "الاسم عربي:", Location = new Point(16, y), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel });
        y += 20;
        _txtNameAr = new TextBox { Location = new Point(16, y), Width = 584, Height = AppTheme.InputHeight, Font = AppTheme.FontArabic, BorderStyle = BorderStyle.FixedSingle };
        pnlBasic.Controls.Add(_txtNameAr);
        y += 44;

        // Row 3: English name
        pnlBasic.Controls.Add(new Label { Text = "الاسم إنجليزي:", Location = new Point(16, y), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel });
        y += 20;
        _txtNameEn = new TextBox { Location = new Point(16, y), Width = 584, Height = AppTheme.InputHeight, Font = AppTheme.FontBody, BorderStyle = BorderStyle.FixedSingle };
        pnlBasic.Controls.Add(_txtNameEn);
        y += 44;

        // Row 4: Category + Group
        AddCombo("القسم:", out _cmbCategory, 16,  y);
        AddCombo("المجموعة:", out _cmbGroup, 320, y);
        y += 68;

        // Row 5: Units
        AddCombo("الوحدة الأساسية:", out _cmbBaseUnit, 16,  y);
        AddCombo("وحدة البيع:",      out _cmbSaleUnit, 320, y);
        y += 68;

        AddCombo("وحدة الشراء:", out _cmbPurchUnit, 16, y);
        _chkActive = new CheckBox { Text = "صنف نشط", Location = new Point(320, y + 12), AutoSize = true, Font = AppTheme.FontBody, Checked = true };
        pnlBasic.Controls.Add(_chkActive);
        y += 68;

        tabBasic.Controls.Add(pnlBasic);

        // ── Tab 2: Prices ──────────────────────────────────────────
        var tabPrices = new TabPage("الأسعار") { BackColor = AppTheme.BgCard };
        var pnlPrices = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16), BackColor = AppTheme.BgCard };
        int py = 8;
        void PriceRow(string lbl, out TextBox tb, string hint = "")
        {
            pnlPrices.Controls.Add(new Label { Text = lbl, Location = new Point(16, py), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel });
            py += 20;
            tb = new TextBox { Location = new Point(16, py), Width = 240, Height = AppTheme.InputHeight, Font = AppTheme.FontBody, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = hint };
            pnlPrices.Controls.Add(tb);
            py += 44;
        }
        PriceRow("سعر الشراء:",      out _txtPurchasePrice, "0.00");
        PriceRow("سعر البيع:",       out _txtSalePrice,     "0.00");
        PriceRow("سعر الجملة:",      out _txtWholesale,     "0.00");
        PriceRow("سعر نصف الجملة:", out _txtHalfWholesale, "0.00");
        PriceRow("السعر الخاص:",     out _txtSpecial,       "0.00");
        tabPrices.Controls.Add(pnlPrices);

        // ── Tab 3: Extra Properties ────────────────────────────────
        var tabExtra = new TabPage("خصائص إضافية") { BackColor = AppTheme.BgCard };
        var pnlExtra = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16), BackColor = AppTheme.BgCard };
        int ey = 8;
        void ExtraRow(string lbl, out TextBox tb)
        {
            pnlExtra.Controls.Add(new Label { Text = lbl, Location = new Point(16, ey), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel });
            ey += 20;
            tb = new TextBox { Location = new Point(16, ey), Width = 240, Height = AppTheme.InputHeight, Font = AppTheme.FontBody, BorderStyle = BorderStyle.FixedSingle };
            pnlExtra.Controls.Add(tb);
            ey += 44;
        }
        ExtraRow("حد أدنى للمخزون:", out _txtMinStock);
        ExtraRow("حد إعادة الطلب:",  out _txtReorderLevel);
        ExtraRow("المصنع:",           out _txtManufacturer);

        // Expiry date
        pnlExtra.Controls.Add(new Label { Text = "تاريخ الصلاحية:", Location = new Point(16, ey), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel });
        ey += 20;
        _chkHasExpiry = new CheckBox { Text = "له تاريخ صلاحية", Location = new Point(16, ey), AutoSize = true, Font = AppTheme.FontBody };
        _dtExpiry = new DateTimePicker { Location = new Point(180, ey), Width = 180, Format = DateTimePickerFormat.Short, Enabled = false };
        _chkHasExpiry.CheckedChanged += (_, _) => _dtExpiry.Enabled = _chkHasExpiry.Checked;
        pnlExtra.Controls.AddRange([_chkHasExpiry, _dtExpiry]);
        ey += 44;

        // Default supplier
        pnlExtra.Controls.Add(new Label { Text = "المورد الافتراضي:", Location = new Point(16, ey), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel });
        ey += 20;
        _cmbSupplier = new ComboBox { Location = new Point(16, ey), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody };
        pnlExtra.Controls.Add(_cmbSupplier);

        tabExtra.Controls.Add(pnlExtra);

        _tabs.TabPages.AddRange([tabBasic, tabPrices, tabExtra]);

        // ── Bottom buttons ─────────────────────────────────────────
        var pnlBtns = new Panel
        {
            Dock = DockStyle.Bottom, Height = 52, BackColor = AppTheme.BgCard,
            Padding = new Padding(12, 8, 12, 8)
        };

        var btnSave = new Button
        {
            Text = "💾 حفظ", Dock = DockStyle.Right, Width = 110, Height = 34,
            FlatStyle = FlatStyle.Flat, BackColor = AppTheme.AccentGreen, ForeColor = Color.White,
            Font = AppTheme.FontBodyBold, Cursor = Cursors.Hand
        };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += (_, _) => SaveProduct();

        var btnCancel = new Button
        {
            Text = "إلغاء", Dock = DockStyle.Left, Width = 90, Height = 34,
            FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = AppTheme.TextSecondary,
            Font = AppTheme.FontBody, Cursor = Cursors.Hand
        };
        btnCancel.FlatAppearance.BorderColor = AppTheme.Border;
        btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        pnlBtns.Controls.AddRange([btnSave, btnCancel]);

        Controls.AddRange([pnlBtns, _tabs]);
    }

    private void LoadCombos()
    {
        Task.Run(async () =>
        {
            // Load categories, units, suppliers in parallel
            var catTask  = _mediator.Send(new GetCategoriesListQuery());
            var unitTask = _mediator.Send(new GetUnitsQuery());
            var supTask  = _mediator.Send(new GetSuppliersListQuery());
            await Task.WhenAll(catTask, unitTask, supTask);

            Invoke(() =>
            {
                if (catTask.Result.IsSuccess && catTask.Result.Value != null)
                {
                    _cmbCategory.Items.Clear();
                    _cmbCategory.Items.Add(new ComboItem(0, "-- اختر قسم --"));
                    foreach (var c in catTask.Result.Value)
                        _cmbCategory.Items.Add(new ComboItem(c.CategoryId, c.Name));
                    _cmbCategory.SelectedIndex = 0;
                    _cmbGroup.Items.AddRange([new ComboItem(0, "-- اختر مجموعة --")]);
                    _cmbGroup.SelectedIndex = 0;
                }

                if (unitTask.Result.IsSuccess && unitTask.Result.Value != null)
                {
                    var units = unitTask.Result.Value.Select(u => new ComboItem(u.UnitId, u.Name)).ToArray<object>();
                    _cmbBaseUnit.Items.AddRange(units);
                    _cmbSaleUnit.Items.AddRange(units);
                    _cmbPurchUnit.Items.AddRange(units);
                    if (units.Length > 0)
                    {
                        _cmbBaseUnit.SelectedIndex  = 0;
                        _cmbSaleUnit.SelectedIndex  = 0;
                        _cmbPurchUnit.SelectedIndex = 0;
                    }
                }

                if (supTask.Result.IsSuccess && supTask.Result.Value != null)
                {
                    _cmbSupplier.Items.Add(new ComboItem(0, "-- بدون مورد افتراضي --"));
                    foreach (var s in supTask.Result.Value)
                        _cmbSupplier.Items.Add(new ComboItem(s.SupplierId, s.Name));
                    _cmbSupplier.SelectedIndex = 0;
                }
            });
        });
    }

    private void LoadProduct(int productId)
    {
        Task.Run(async () =>
        {
            var q      = new GetProductByIdQuery(productId);
            var result = await _mediator.Send(q);
            Invoke(() =>
            {
                if (!result.IsSuccess || result.Value == null) return;
                var p = result.Value;
                _txtCode.Text          = p.ProductCode;
                _txtBarcode.Text       = p.Barcode;
                _txtNameAr.Text        = p.NameAr;
                _txtNameEn.Text        = p.NameEn;
                _chkActive.Checked     = p.IsActive;
                _txtPurchasePrice.Text = p.PurchasePrice.ToString("N2");
                _txtSalePrice.Text     = p.SalePrice.ToString("N2");
                _txtWholesale.Text     = p.WholesalePrice.ToString("N2");
                _txtHalfWholesale.Text = p.HalfWholesalePrice.ToString("N2");
                _txtSpecial.Text       = p.SpecialPrice.ToString("N2");
                _txtMinStock.Text      = p.MinStock.ToString();
                _txtReorderLevel.Text  = p.ReorderLevel.ToString();
                _txtManufacturer.Text  = p.Manufacturer;
                if (p.ExpiryDate.HasValue)
                {
                    _chkHasExpiry.Checked = true;
                    _dtExpiry.Value       = p.ExpiryDate.Value;
                }
                // Select combos by ID
                SelectComboById(_cmbCategory, p.CategoryId);
                SelectComboById(_cmbBaseUnit, p.BaseUnitId);
                SelectComboById(_cmbSaleUnit, p.SaleUnitId);
                SelectComboById(_cmbPurchUnit, p.PurchaseUnitId);
                if (p.DefaultSupplierId.HasValue) SelectComboById(_cmbSupplier, p.DefaultSupplierId.Value);
            });
        });
    }

    private static void SelectComboById(ComboBox cmb, int id)
    {
        foreach (var item in cmb.Items)
            if (item is ComboItem ci && ci.Id == id) { cmb.SelectedItem = item; return; }
    }

    private void SaveProduct()
    {
        if (string.IsNullOrWhiteSpace(_txtNameAr.Text))
        {
            MessageBox.Show("الاسم العربي مطلوب", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        decimal.TryParse(_txtPurchasePrice.Text, out var purchPrice);
        decimal.TryParse(_txtSalePrice.Text,     out var salePrice);
        decimal.TryParse(_txtWholesale.Text,     out var wholesale);
        decimal.TryParse(_txtHalfWholesale.Text, out var halfWholesale);
        decimal.TryParse(_txtSpecial.Text,       out var special);
        int.TryParse(_txtMinStock.Text,          out var minStock);
        int.TryParse(_txtReorderLevel.Text,      out var reorder);

        var catItem  = _cmbCategory.SelectedItem as ComboItem;
        var baseUnit = _cmbBaseUnit.SelectedItem  as ComboItem;
        var saleUnit = _cmbSaleUnit.SelectedItem  as ComboItem;
        var purchUnit= _cmbPurchUnit.SelectedItem as ComboItem;
        var supplier = _cmbSupplier.SelectedItem  as ComboItem;

        Task.Run(async () =>
        {
            object cmd;
            if (_productId.HasValue)
            {
                cmd = new UpdateProductCommand(
                    ProductId: _productId.Value,
                    ProductCode: _txtCode.Text.Trim(),
                    Barcode: _txtBarcode.Text.Trim(),
                    NameAr: _txtNameAr.Text.Trim(),
                    NameEn: _txtNameEn.Text.Trim(),
                    CategoryId: catItem?.Id ?? 0,
                    BaseUnitId: baseUnit?.Id ?? 0,
                    SaleUnitId: saleUnit?.Id ?? 0,
                    PurchaseUnitId: purchUnit?.Id ?? 0,
                    PurchasePrice: purchPrice, SalePrice: salePrice,
                    WholesalePrice: wholesale, HalfWholesalePrice: halfWholesale, SpecialPrice: special,
                    MinStock: minStock, ReorderLevel: reorder,
                    Manufacturer: _txtManufacturer.Text.Trim(),
                    ExpiryDate: _chkHasExpiry.Checked ? _dtExpiry.Value : null,
                    DefaultSupplierId: supplier?.Id == 0 ? null : supplier?.Id,
                    IsActive: _chkActive.Checked,
                    ModifiedBy: UserSession.Current.UserId);
            }
            else
            {
                cmd = new CreateProductCommand(
                    ProductCode: _txtCode.Text.Trim(),
                    Barcode: _txtBarcode.Text.Trim(),
                    NameAr: _txtNameAr.Text.Trim(),
                    NameEn: _txtNameEn.Text.Trim(),
                    CategoryId: catItem?.Id ?? 0,
                    BaseUnitId: baseUnit?.Id ?? 0,
                    SaleUnitId: saleUnit?.Id ?? 0,
                    PurchaseUnitId: purchUnit?.Id ?? 0,
                    PurchasePrice: purchPrice, SalePrice: salePrice,
                    WholesalePrice: wholesale, HalfWholesalePrice: halfWholesale, SpecialPrice: special,
                    MinStock: minStock, ReorderLevel: reorder,
                    Manufacturer: _txtManufacturer.Text.Trim(),
                    ExpiryDate: _chkHasExpiry.Checked ? _dtExpiry.Value : null,
                    DefaultSupplierId: supplier?.Id == 0 ? null : supplier?.Id,
                    IsActive: _chkActive.Checked,
                    CreatedBy: UserSession.Current.UserId);
            }

            var result = cmd is UpdateProductCommand uc
                ? await _mediator.Send(uc)
                : await _mediator.Send((CreateProductCommand)cmd);

            Invoke(() =>
            {
                if (result.IsSuccess) { DialogResult = DialogResult.OK; Close(); }
                else MessageBox.Show(result.Error, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        });
    }
}

// ════════════════════════════════════════════════════════════════════
// PRODUCT STOCK DIALOG
// ════════════════════════════════════════════════════════════════════
public sealed class ProductStockDialog : Form
{
    private readonly IMediator _mediator;
    private readonly int       _productId;
    private DataGridView _dgv = null!;

    public ProductStockDialog(IMediator mediator, int productId)
    {
        _mediator  = mediator;
        _productId = productId;
        InitializeComponent();
        LoadStock();
    }

    private void InitializeComponent()
    {
        Text = "مخزون الصنف"; Size = new Size(500, 380);
        FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = AppTheme.BgContent;
        RightToLeft = RightToLeft.Yes; RightToLeftLayout = true;

        _dgv = new DataGridView { Dock = DockStyle.Fill };
        AppTheme.StyleDataGrid(_dgv);
        _dgv.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "Warehouse", HeaderText = "المستودع",    FillWeight = 40 },
            new DataGridViewTextBoxColumn { Name = "Branch",    HeaderText = "الفرع",        FillWeight = 30 },
            new DataGridViewTextBoxColumn { Name = "Stock",     HeaderText = "الكمية",       FillWeight = 15 },
            new DataGridViewTextBoxColumn { Name = "AvgCost",   HeaderText = "متوسط التكلفة",FillWeight = 15 }
        );

        var btnClose = new Button
        {
            Text = "إغلاق", Dock = DockStyle.Bottom, Height = 40,
            FlatStyle = FlatStyle.Flat, BackColor = AppTheme.BgContent, ForeColor = AppTheme.TextSecondary,
            Font = AppTheme.FontBody, Cursor = Cursors.Hand
        };
        btnClose.FlatAppearance.BorderSize = 0;
        btnClose.Click += (_, _) => Close();
        Controls.AddRange([btnClose, _dgv]);
    }

    private void LoadStock()
    {
        Task.Run(async () =>
        {
            var q      = new GetProductStockQuery(_productId);
            var result = await _mediator.Send(q);
            Invoke(() =>
            {
                _dgv.Rows.Clear();
                if (!result.IsSuccess || result.Value == null) return;
                foreach (var s in result.Value)
                    _dgv.Rows.Add(s.WarehouseName, s.BranchName, s.Quantity, $"{s.AverageCost:N2}");
            });
        });
    }
}

// ── Helper record ──────────────────────────────────────────────────
public record ComboItem(int Id, string Name)
{
    public override string ToString() => Name;
}
