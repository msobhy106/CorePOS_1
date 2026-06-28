using MediatR;
using CorePOS.WinForms.Theme;
using CorePOS.WinForms.Infrastructure;
using CorePOS.WinForms.Controls;
using CorePOS.Application.Features.Sales.Commands;
using CorePOS.Application.Features.Products.Queries;
using CorePOS.Application.Features.Customers.Queries;

namespace CorePOS.WinForms.Forms.POS;

/// <summary>
/// Main POS / Cashier screen.
/// Layout: Right = product search + cart | Left = payment panel.
/// Full keyboard support. Barcode scanner ready. RTL Arabic.
/// </summary>
public sealed class POSForm : BaseForm
{
    // ── Cart items ────────────────────────────────────────────────
    private readonly List<CartItem> _cartItems = new();
    private int? _selectedCustomerId;
    private string _selectedCustomerName = string.Empty;

    // ── UI Controls ───────────────────────────────────────────────
    // Search area
    private TextBox _txtBarcode     = null!;
    private SearchBox _searchBox    = null!;
    private DataGridView _dgvSearch = null!;

    // Cart
    private DataGridView _dgvCart   = null!;

    // Customer
    private Label  _lblCustomer     = null!;
    private Button _btnSelectCustomer = null!;
    private Button _btnClearCustomer  = null!;

    // Totals
    private Label _lblSubtotal      = null!;
    private Label _lblDiscount      = null!;
    private Label _lblTax           = null!;
    private Label _lblTotal         = null!;
    private Label _lblPaid          = null!;
    private Label _lblRemaining     = null!;

    // Payment inputs
    private TextBox _txtDiscount    = null!;
    private TextBox _txtTax         = null!;
    private TextBox _txtPaid        = null!;
    private TextBox _txtNotes       = null!;

    // Payment method
    private ComboBox _cmbPayMethod  = null!;

    // Action buttons
    private Button _btnPay          = null!;
    private Button _btnHold         = null!;
    private Button _btnRetrieve     = null!;
    private Button _btnCancelItem   = null!;
    private Button _btnClearCart    = null!;
    private Button _btnDelivery     = null!;

    // Delivery
    private decimal _deliveryCost   = 0;
    private string  _deliveryAgent  = string.Empty;

    // Search timer (debounce)
    private System.Windows.Forms.Timer _searchDebounce = null!;
    private string _lastSearchQuery = string.Empty;

    public POSForm(IMediator mediator) : base(mediator)
    {
        InitializeComponent();
        CheckShift();
    }

    // ══════════════════════════════════════════════════════════════
    // INITIALIZE
    // ══════════════════════════════════════════════════════════════
    private void InitializeComponent()
    {
        Text      = "نقطة البيع";
        BackColor = AppTheme.BgContent;

        // ── Main layout: Right content (60%) | Left payment (40%) ─
        var splitMain = new SplitContainer
        {
            Dock             = DockStyle.Fill,
            Orientation      = Orientation.Vertical,
            SplitterDistance = 65,   // percentage handled in Resize
            BorderStyle      = BorderStyle.None,
            BackColor        = AppTheme.BgContent,
            Panel1MinSize    = 400,
            Panel2MinSize    = 280,
            IsSplitterFixed  = false
        };

        BuildRightPanel(splitMain.Panel1);
        BuildLeftPanel(splitMain.Panel2);

        Controls.Add(splitMain);

        // Fix splitter ratio on resize
        Resize += (_, _) =>
        {
            try { splitMain.SplitterDistance = (int)(splitMain.Width * 0.60); }
            catch { }
        };

        // Keyboard shortcuts
        KeyPreview = true;
        KeyDown   += POSForm_KeyDown;

        // Search debounce timer
        _searchDebounce = new System.Windows.Forms.Timer { Interval = 300 };
        _searchDebounce.Tick += (_, _) => { _searchDebounce.Stop(); ExecuteSearch(); };
    }

    // ── RIGHT PANEL: Barcode + Product search + Cart ──────────────
    private void BuildRightPanel(Panel parent)
    {
        parent.BackColor = AppTheme.BgContent;
        parent.Padding   = new Padding(12, 8, 6, 8);

        // ── Barcode row ────────────────────────────────────────────
        var pnlBarcode = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 48,
            BackColor = AppTheme.BgContent
        };

        var lblBarcodeHint = new Label
        {
            Text      = "باركود / بحث:",
            Font      = AppTheme.FontBodyBold,
            ForeColor = AppTheme.TextLabel,
            AutoSize  = true,
            Dock      = DockStyle.Right,
            TextAlign = ContentAlignment.MiddleRight,
            Width     = 110
        };

        _txtBarcode = new TextBox
        {
            Dock        = DockStyle.Fill,
            Font        = new Font("Segoe UI", 13f),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor   = AppTheme.BgCard,
            PlaceholderText = "امسح الباركود أو اكتب للبحث..."
        };
        _txtBarcode.TextChanged += (_, _) =>
        {
            _searchDebounce.Stop();
            _searchDebounce.Start();
        };
        _txtBarcode.KeyDown += TxtBarcode_KeyDown;

        pnlBarcode.Controls.AddRange([_txtBarcode, lblBarcodeHint]);

        // ── Product search results (appears only when searching) ───
        var pnlSearchResults = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 180,
            BackColor = AppTheme.BgCard,
            Visible   = false
        };

        _dgvSearch = new DataGridView { Dock = DockStyle.Fill };
        AppTheme.StyleDataGrid(_dgvSearch);
        _dgvSearch.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "Barcode",  HeaderText = "باركود",    FillWeight = 20 },
            new DataGridViewTextBoxColumn { Name = "NameAr",   HeaderText = "الصنف",     FillWeight = 40 },
            new DataGridViewTextBoxColumn { Name = "SalePrice",HeaderText = "السعر",     FillWeight = 20 },
            new DataGridViewTextBoxColumn { Name = "Stock",    HeaderText = "المخزون",   FillWeight = 20 }
        );
        _dgvSearch.CellDoubleClick += (_, e) =>
        {
            if (e.RowIndex >= 0) AddSearchResultToCart(e.RowIndex);
        };
        _dgvSearch.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter && _dgvSearch.CurrentRow != null)
                AddSearchResultToCart(_dgvSearch.CurrentRow.Index);
        };
        pnlSearchResults.Controls.Add(_dgvSearch);
        pnlSearchResults.Tag = "searchPanel";

        // ── Customer row ───────────────────────────────────────────
        var pnlCustomer = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 40,
            BackColor = AppTheme.BgContent,
            Padding   = new Padding(0, 4, 0, 4)
        };

        _lblCustomer = new Label
        {
            Text      = "بدون عميل",
            Font      = AppTheme.FontBody,
            ForeColor = AppTheme.TextSecondary,
            AutoSize  = true,
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight
        };

        _btnSelectCustomer = new Button
        {
            Text      = "👤 تحديد عميل",
            Width     = 110,
            Height    = 30,
            Dock      = DockStyle.Left,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = AppTheme.AccentBlue,
            Font      = AppTheme.FontSmall,
            Cursor    = Cursors.Hand
        };
        _btnSelectCustomer.FlatAppearance.BorderColor = AppTheme.AccentBlue;
        _btnSelectCustomer.Click += (_, _) => SelectCustomer();

        _btnClearCustomer = new Button
        {
            Text      = "✕",
            Width     = 28,
            Height    = 28,
            Dock      = DockStyle.Left,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = AppTheme.AccentRed,
            Font      = AppTheme.FontSmallBold,
            Cursor    = Cursors.Hand,
            Visible   = false
        };
        _btnClearCustomer.FlatAppearance.BorderSize = 0;
        _btnClearCustomer.Click += (_, _) => ClearCustomer();

        pnlCustomer.Controls.AddRange([_btnClearCustomer, _btnSelectCustomer, _lblCustomer]);

        // ── Cart DataGridView ──────────────────────────────────────
        var pnlCart = new Panel
        {
            Dock      = DockStyle.Fill,
            BackColor = AppTheme.BgCard,
            Padding   = new Padding(0)
        };

        var lblCartTitle = new Label
        {
            Text      = "🛒 الفاتورة",
            Font      = AppTheme.FontH2,
            ForeColor = AppTheme.TextPrimary,
            Dock      = DockStyle.Top,
            Height    = 36,
            Padding   = new Padding(8, 6, 0, 0)
        };

        _dgvCart = new DataGridView { Dock = DockStyle.Fill };
        AppTheme.StyleDataGrid(_dgvCart);
        _dgvCart.ReadOnly = false;
        _dgvCart.EditMode = DataGridViewEditMode.EditOnEnter;
        _dgvCart.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "RowNo",    HeaderText = "#",          Width = 36,  ReadOnly = true },
            new DataGridViewTextBoxColumn { Name = "Barcode",  HeaderText = "باركود",     Width = 90,  ReadOnly = true },
            new DataGridViewTextBoxColumn { Name = "NameAr",   HeaderText = "الصنف",      FillWeight = 35, ReadOnly = true },
            new DataGridViewTextBoxColumn { Name = "UnitName", HeaderText = "الوحدة",     Width = 70,  ReadOnly = true },
            new DataGridViewTextBoxColumn { Name = "Price",    HeaderText = "السعر",      Width = 80,  ReadOnly = true },
            new DataGridViewTextBoxColumn { Name = "Qty",      HeaderText = "الكمية",     Width = 70,  ReadOnly = false },
            new DataGridViewTextBoxColumn { Name = "Discount", HeaderText = "خصم",        Width = 60,  ReadOnly = false },
            new DataGridViewTextBoxColumn { Name = "LineTotal", HeaderText = "الإجمالي", Width = 90,  ReadOnly = true }
        );
        _dgvCart.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _dgvCart.CellEndEdit        += DgvCart_CellEndEdit;
        _dgvCart.KeyDown            += DgvCart_KeyDown;

        // Cart action buttons
        var pnlCartActions = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 40,
            BackColor = AppTheme.BgCard,
            Padding   = new Padding(8, 4, 8, 4)
        };

        _btnCancelItem = new Button
        {
            Text      = "🗑 حذف صنف",
            Dock      = DockStyle.Left,
            Width     = 100,
            Height    = 30,
            FlatStyle = FlatStyle.Flat,
            BackColor = AppTheme.AccentRed,
            ForeColor = Color.White,
            Font      = AppTheme.FontSmall,
            Cursor    = Cursors.Hand
        };
        _btnCancelItem.FlatAppearance.BorderSize = 0;
        _btnCancelItem.Click += (_, _) => RemoveSelectedCartItem();

        _btnClearCart = new Button
        {
            Text      = "🗑 مسح الكل",
            Dock      = DockStyle.Left,
            Width     = 90,
            Height    = 30,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = AppTheme.AccentRed,
            Font      = AppTheme.FontSmall,
            Cursor    = Cursors.Hand,
            Margin    = new Padding(6, 0, 0, 0)
        };
        _btnClearCart.FlatAppearance.BorderColor = AppTheme.AccentRed;
        _btnClearCart.Click += (_, _) => ClearCart();

        _btnDelivery = new Button
        {
            Text      = "🚚 توصيل",
            Dock      = DockStyle.Right,
            Width     = 90,
            Height    = 30,
            FlatStyle = FlatStyle.Flat,
            BackColor = AppTheme.AccentOrange,
            ForeColor = Color.White,
            Font      = AppTheme.FontSmall,
            Cursor    = Cursors.Hand
        };
        _btnDelivery.FlatAppearance.BorderSize = 0;
        _btnDelivery.Click += (_, _) => ShowDeliveryDialog();

        pnlCartActions.Controls.AddRange([_btnDelivery, _btnClearCart, _btnCancelItem]);
        pnlCart.Controls.AddRange([pnlCartActions, lblCartTitle, _dgvCart]);

        // Assemble right panel (DockStyle.Top stacks bottom-up)
        parent.Controls.Add(pnlCart);
        parent.Controls.Add(pnlCustomer);
        parent.Controls.Add(pnlSearchResults);
        parent.Controls.Add(pnlBarcode);

        // Show/hide search results
        _txtBarcode.TextChanged += (_, _) =>
        {
            pnlSearchResults.Visible = _txtBarcode.Text.Length > 0;
        };
    }

    // ── LEFT PANEL: Totals + Payment ──────────────────────────────
    private void BuildLeftPanel(Panel parent)
    {
        parent.BackColor = AppTheme.BgCard;
        parent.Padding   = new Padding(6, 8, 12, 8);

        // ── Totals section ─────────────────────────────────────────
        var pnlTotals = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 260,
            BackColor = AppTheme.BgCard,
            Padding   = new Padding(12)
        };

        var lblTotalsTitle = new Label
        {
            Text      = "الإجمالي",
            Font      = AppTheme.FontH2,
            ForeColor = AppTheme.TextPrimary,
            Dock      = DockStyle.Top,
            Height    = 32
        };

        // Helper to build total row
        Panel MakeTotalRow(string labelText, out Label valueLabel, Color? valueColor = null)
        {
            var row = new Panel { Dock = DockStyle.Top, Height = 34, BackColor = AppTheme.BgCard };
            var lbl = new Label
            {
                Text      = labelText,
                Font      = AppTheme.FontBody,
                ForeColor = AppTheme.TextLabel,
                Dock      = DockStyle.Right,
                TextAlign = ContentAlignment.MiddleRight,
                AutoSize  = false,
                Width     = 120
            };
            valueLabel = new Label
            {
                Text      = "0.00",
                Font      = AppTheme.FontBodyBold,
                ForeColor = valueColor ?? AppTheme.TextPrimary,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            row.Controls.AddRange([valueLabel, lbl]);
            return row;
        }

        var rowSubtotal  = MakeTotalRow("المجموع الفرعي:",  out _lblSubtotal);
        var rowDiscount  = MakeTotalRow("الخصم:",           out _lblDiscount,  AppTheme.AccentRed);
        var rowTax       = MakeTotalRow("الضريبة:",         out _lblTax,       AppTheme.AccentOrange);
        var rowTotal     = MakeTotalRow("الإجمالي النهائي:", out _lblTotal);
        _lblTotal.Font   = AppTheme.FontPOSLarge;
        _lblTotal.ForeColor = AppTheme.AccentBlue;

        var rowPaid      = MakeTotalRow("المدفوع:",         out _lblPaid,      AppTheme.AccentGreen);
        var rowRemaining = MakeTotalRow("المتبقي:",         out _lblRemaining, AppTheme.AccentRed);

        // Stack rows in reverse (DockStyle.Top)
        pnlTotals.Controls.Add(rowRemaining);
        pnlTotals.Controls.Add(rowPaid);
        pnlTotals.Controls.Add(rowTotal);
        pnlTotals.Controls.Add(rowTax);
        pnlTotals.Controls.Add(rowDiscount);
        pnlTotals.Controls.Add(rowSubtotal);
        pnlTotals.Controls.Add(lblTotalsTitle);

        // ── Payment inputs section ─────────────────────────────────
        var pnlPayInputs = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 220,
            BackColor = AppTheme.BgCard,
            Padding   = new Padding(12, 0, 12, 0)
        };

        Panel MakeInputRow(string lbl, out TextBox tb, string placeholder = "")
        {
            var row = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = AppTheme.BgCard };
            var label = new Label
            {
                Text      = lbl,
                Font      = AppTheme.FontSmall,
                ForeColor = AppTheme.TextLabel,
                Dock      = DockStyle.Right,
                Width     = 90,
                TextAlign = ContentAlignment.MiddleRight
            };
            tb = new TextBox
            {
                Dock            = DockStyle.Fill,
                Font            = AppTheme.FontBody,
                BorderStyle     = BorderStyle.FixedSingle,
                BackColor       = AppTheme.BgInput,
                PlaceholderText = placeholder
            };
            row.Controls.AddRange([tb, label]);
            return row;
        }

        var rowDiscountInput = MakeInputRow("خصم %:",    out _txtDiscount, "0");
        var rowTaxInput      = MakeInputRow("ضريبة %:",  out _txtTax,      "0");
        var rowPaidInput     = MakeInputRow("المدفوع:",  out _txtPaid,     "0.00");
        var rowNotesInput    = MakeInputRow("ملاحظات:",  out _txtNotes,    "اختياري");

        _txtDiscount.TextChanged += (_, _) => RecalculateTotals();
        _txtTax.TextChanged      += (_, _) => RecalculateTotals();
        _txtPaid.TextChanged     += (_, _) =>
        {
            if (decimal.TryParse(_txtPaid.Text, out var paid))
            {
                var total   = GetTotal();
                var remaining = total - paid;
                _lblPaid.Text      = $"{paid:N2}";
                _lblRemaining.Text = $"{remaining:N2}";
                _lblRemaining.ForeColor = remaining <= 0 ? AppTheme.AccentGreen : AppTheme.AccentRed;
            }
        };

        // Payment method
        var rowPayMethod = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = AppTheme.BgCard };
        var lblPayMethod = new Label
        {
            Text      = "الدفع:",
            Font      = AppTheme.FontSmall,
            ForeColor = AppTheme.TextLabel,
            Dock      = DockStyle.Right,
            Width     = 90,
            TextAlign = ContentAlignment.MiddleRight
        };
        _cmbPayMethod = new ComboBox
        {
            Dock          = DockStyle.Fill,
            Font          = AppTheme.FontBody,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _cmbPayMethod.Items.AddRange(["نقدي", "فيزا", "تحويل بنكي", "محفظة إلكترونية", "آجل", "مختلط"]);
        _cmbPayMethod.SelectedIndex = 0;
        _cmbPayMethod.SelectedIndexChanged += (_, _) => OnPayMethodChanged();
        rowPayMethod.Controls.AddRange([_cmbPayMethod, lblPayMethod]);

        pnlPayInputs.Controls.Add(rowNotesInput);
        pnlPayInputs.Controls.Add(rowPaidInput);
        pnlPayInputs.Controls.Add(rowTaxInput);
        pnlPayInputs.Controls.Add(rowDiscountInput);
        pnlPayInputs.Controls.Add(rowPayMethod);

        // ── Action buttons ─────────────────────────────────────────
        var pnlActions = new Panel
        {
            Dock      = DockStyle.Fill,
            BackColor = AppTheme.BgCard,
            Padding   = new Padding(12, 8, 12, 12)
        };

        _btnPay = new Button
        {
            Text      = "✔  إتمام الدفع  (F10)",
            Dock      = DockStyle.Top,
            Height    = 56,
            FlatStyle = FlatStyle.Flat,
            BackColor = AppTheme.AccentGreen,
            ForeColor = Color.White,
            Font      = new Font("Segoe UI", 14f, FontStyle.Bold),
            Cursor    = Cursors.Hand
        };
        _btnPay.FlatAppearance.BorderSize = 0;
        _btnPay.Click += (_, _) => ProcessSale();

        var pnlSecondaryBtns = new FlowLayoutPanel
        {
            Dock          = DockStyle.Top,
            Height        = 44,
            FlowDirection = FlowDirection.RightToLeft,
            BackColor     = AppTheme.BgCard,
            Padding       = new Padding(0, 6, 0, 0)
        };

        _btnHold = new Button
        {
            Text      = "⏸ تعليق (F8)",
            Width     = 115,
            Height    = 34,
            FlatStyle = FlatStyle.Flat,
            BackColor = AppTheme.AccentYellow,
            ForeColor = Color.White,
            Font      = AppTheme.FontSmallBold,
            Cursor    = Cursors.Hand
        };
        _btnHold.FlatAppearance.BorderSize = 0;
        _btnHold.Click += (_, _) => HoldInvoice();

        _btnRetrieve = new Button
        {
            Text      = "⏮ استرجاع (F9)",
            Width     = 125,
            Height    = 34,
            FlatStyle = FlatStyle.Flat,
            BackColor = AppTheme.AccentPurple,
            ForeColor = Color.White,
            Font      = AppTheme.FontSmallBold,
            Cursor    = Cursors.Hand
        };
        _btnRetrieve.FlatAppearance.BorderSize = 0;
        _btnRetrieve.Click += (_, _) => RetrieveInvoice();

        pnlSecondaryBtns.Controls.AddRange([_btnHold, _btnRetrieve]);

        pnlActions.Controls.Add(pnlSecondaryBtns);
        pnlActions.Controls.Add(_btnPay);

        // Assemble left panel
        parent.Controls.Add(pnlActions);
        parent.Controls.Add(pnlPayInputs);
        parent.Controls.Add(pnlTotals);
    }

    // ══════════════════════════════════════════════════════════════
    // SHIFT CHECK
    // ══════════════════════════════════════════════════════════════
    private void CheckShift()
    {
        if (!UserSession.Current.HasOpenShift)
        {
            var result = MessageBox.Show(
                "لا توجد وردية مفتوحة. هل تريد فتح وردية جديدة؟",
                "فتح وردية", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
                OpenShiftDialog();
        }
    }

    private void OpenShiftDialog()
    {
        using var dlg = new ShiftOpenDialog(_mediator);
        if (dlg.ShowDialog() == DialogResult.OK)
        {
            // Refresh top bar shift status
            FindMainForm()?.RefreshShiftStatus();
        }
    }

    // ══════════════════════════════════════════════════════════════
    // SEARCH
    // ══════════════════════════════════════════════════════════════
    private void TxtBarcode_KeyDown(object? s, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            var text = _txtBarcode.Text.Trim();
            if (string.IsNullOrEmpty(text)) return;

            // Try exact barcode match first
            _ = TryAddByBarcodeAsync(text);
        }
        else if (e.KeyCode == Keys.Down && _dgvSearch.Rows.Count > 0)
        {
            _dgvSearch.Focus();
            _dgvSearch.CurrentCell = _dgvSearch.Rows[0].Cells[0];
        }
    }

    private async Task TryAddByBarcodeAsync(string barcode)
    {
        try
        {
            var q      = new GetProductByBarcodeQuery(barcode, UserSession.Current.WarehouseId);
            var result = await _mediator.Send(q);

            if (result.IsSuccess && result.Value != null)
            {
                InvokeOnUI(() =>
                {
                    AddToCart(result.Value.ProductId, result.Value.Barcode,
                              result.Value.NameAr, result.Value.SaleUnitName,
                              result.Value.SalePrice, result.Value.Stock);
                    _txtBarcode.Clear();
                    _txtBarcode.Focus();
                });
            }
            else
            {
                // Fall through to text search
                _searchDebounce.Stop();
                _searchDebounce.Start();
            }
        }
        catch { }
    }

    private void ExecuteSearch()
    {
        var q = _txtBarcode.Text.Trim();
        if (q == _lastSearchQuery || q.Length < 1) return;
        _lastSearchQuery = q;

        _ = SearchProductsAsync(q);
    }

    private async Task SearchProductsAsync(string query)
    {
        try
        {
            var q      = new SearchProductsQuery(query, UserSession.Current.BranchId, 20);
            var result = await _mediator.Send(q);

            InvokeOnUI(() =>
            {
                _dgvSearch.Rows.Clear();
                if (result.IsSuccess && result.Value != null)
                {
                    foreach (var p in result.Value)
                        _dgvSearch.Rows.Add(p.Barcode, p.NameAr, $"{p.SalePrice:N2}", p.Stock);
                }
            });
        }
        catch { }
    }

    private void AddSearchResultToCart(int rowIndex)
    {
        var row = _dgvSearch.Rows[rowIndex];
        // In real app, product ID would be in a hidden column
        // Here we re-query by barcode
        var barcode = row.Cells["Barcode"].Value?.ToString() ?? string.Empty;
        _ = TryAddByBarcodeAsync(barcode);
        _txtBarcode.Clear();
        _txtBarcode.Focus();
    }

    // ══════════════════════════════════════════════════════════════
    // CART MANAGEMENT
    // ══════════════════════════════════════════════════════════════
    private void AddToCart(int productId, string barcode, string nameAr,
                           string unitName, decimal price, decimal stock)
    {
        if (stock <= 0)
        {
            ShowError($"الصنف [{nameAr}] غير متوفر في المخزون");
            return;
        }

        // Check if product already in cart → increase quantity
        var existing = _cartItems.FirstOrDefault(c => c.ProductId == productId);
        if (existing != null)
        {
            existing.Qty++;
            existing.LineTotal = (existing.Price - existing.ItemDiscount) * existing.Qty;
        }
        else
        {
            _cartItems.Add(new CartItem
            {
                ProductId    = productId,
                Barcode      = barcode,
                NameAr       = nameAr,
                UnitName     = unitName,
                Price        = price,
                Qty          = 1,
                ItemDiscount = 0,
                LineTotal    = price
            });
        }

        RefreshCartGrid();
        RecalculateTotals();
    }

    private void RefreshCartGrid()
    {
        _dgvCart.Rows.Clear();
        int rowNo = 1;
        foreach (var item in _cartItems)
        {
            _dgvCart.Rows.Add(
                rowNo++,
                item.Barcode,
                item.NameAr,
                item.UnitName,
                $"{item.Price:N2}",
                item.Qty,
                $"{item.ItemDiscount:N2}",
                $"{item.LineTotal:N2}"
            );
        }
    }

    private void DgvCart_CellEndEdit(object? s, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.RowIndex >= _cartItems.Count) return;
        var item = _cartItems[e.RowIndex];
        var cell = _dgvCart.Rows[e.RowIndex].Cells[e.ColumnIndex];

        if (_dgvCart.Columns[e.ColumnIndex].Name == "Qty")
        {
            if (decimal.TryParse(cell.Value?.ToString(), out var qty) && qty > 0)
            {
                item.Qty       = qty;
                item.LineTotal = (item.Price - item.ItemDiscount) * qty;
            }
        }
        else if (_dgvCart.Columns[e.ColumnIndex].Name == "Discount")
        {
            if (decimal.TryParse(cell.Value?.ToString(), out var disc))
            {
                item.ItemDiscount = disc;
                item.LineTotal    = (item.Price - disc) * item.Qty;
            }
        }

        RefreshCartGrid();
        RecalculateTotals();
    }

    private void DgvCart_KeyDown(object? s, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Delete) RemoveSelectedCartItem();
    }

    private void RemoveSelectedCartItem()
    {
        if (_dgvCart.CurrentRow == null) return;
        var idx = _dgvCart.CurrentRow.Index;
        if (idx >= 0 && idx < _cartItems.Count)
        {
            _cartItems.RemoveAt(idx);
            RefreshCartGrid();
            RecalculateTotals();
        }
    }

    private void ClearCart()
    {
        if (_cartItems.Count == 0) return;
        if (!Confirm("هل تريد مسح الفاتورة بالكامل؟")) return;
        _cartItems.Clear();
        _deliveryCost = 0;
        _deliveryAgent = string.Empty;
        RefreshCartGrid();
        RecalculateTotals();
        ClearCustomer();
    }

    // ══════════════════════════════════════════════════════════════
    // TOTALS
    // ══════════════════════════════════════════════════════════════
    private void RecalculateTotals()
    {
        var subtotal = _cartItems.Sum(c => c.Price * c.Qty);
        var itemDiscounts = _cartItems.Sum(c => c.ItemDiscount * c.Qty);

        decimal.TryParse(_txtDiscount.Text, out var discPct);
        decimal.TryParse(_txtTax.Text,      out var taxPct);

        var afterItemDisc = subtotal - itemDiscounts;
        var headerDisc    = afterItemDisc * discPct / 100;
        var afterDisc     = afterItemDisc - headerDisc;
        var tax           = afterDisc * taxPct / 100;
        var total         = afterDisc + tax + _deliveryCost;

        _lblSubtotal.Text  = $"{subtotal:N2}";
        _lblDiscount.Text  = $"-{(itemDiscounts + headerDisc):N2}";
        _lblTax.Text       = $"+{tax:N2}";
        _lblTotal.Text     = $"{total:N2}";

        // If cash, auto-fill paid
        if (_cmbPayMethod.SelectedIndex == 0) // نقدي
        {
            _txtPaid.Text       = $"{total:N2}";
            _lblPaid.Text       = $"{total:N2}";
            _lblRemaining.Text  = "0.00";
            _lblRemaining.ForeColor = AppTheme.AccentGreen;
        }
        else
        {
            decimal.TryParse(_txtPaid.Text, out var paid);
            var remaining = total - paid;
            _lblPaid.Text      = $"{paid:N2}";
            _lblRemaining.Text = $"{remaining:N2}";
            _lblRemaining.ForeColor = remaining <= 0 ? AppTheme.AccentGreen : AppTheme.AccentRed;
        }
    }

    private decimal GetTotal()
    {
        var subtotal = _cartItems.Sum(c => c.Price * c.Qty);
        var itemDisc = _cartItems.Sum(c => c.ItemDiscount * c.Qty);
        decimal.TryParse(_txtDiscount.Text, out var discPct);
        decimal.TryParse(_txtTax.Text,      out var taxPct);
        var afterItemDisc = subtotal - itemDisc;
        var headerDisc    = afterItemDisc * discPct / 100;
        var afterDisc     = afterItemDisc - headerDisc;
        var tax           = afterDisc * taxPct / 100;
        return afterDisc + tax + _deliveryCost;
    }

    private void OnPayMethodChanged()
    {
        bool isCash = _cmbPayMethod.SelectedIndex == 0;
        _txtPaid.ReadOnly = isCash;
        RecalculateTotals();
    }

    // ══════════════════════════════════════════════════════════════
    // CUSTOMER
    // ══════════════════════════════════════════════════════════════
    private void SelectCustomer()
    {
        using var dlg = new CustomerPickerDialog(_mediator);
        if (dlg.ShowDialog() == DialogResult.OK)
        {
            _selectedCustomerId   = dlg.SelectedCustomerId;
            _selectedCustomerName = dlg.SelectedCustomerName;
            _lblCustomer.Text     = $"👤 {_selectedCustomerName}";
            _lblCustomer.ForeColor = AppTheme.AccentBlue;
            _btnClearCustomer.Visible = true;
        }
    }

    private void ClearCustomer()
    {
        _selectedCustomerId   = null;
        _selectedCustomerName = string.Empty;
        _lblCustomer.Text     = "بدون عميل";
        _lblCustomer.ForeColor = AppTheme.TextSecondary;
        _btnClearCustomer.Visible = false;
    }

    // ══════════════════════════════════════════════════════════════
    // DELIVERY
    // ══════════════════════════════════════════════════════════════
    private void ShowDeliveryDialog()
    {
        using var dlg = new DeliveryDialog(_deliveryCost, _deliveryAgent);
        if (dlg.ShowDialog() == DialogResult.OK)
        {
            _deliveryCost  = dlg.DeliveryCost;
            _deliveryAgent = dlg.DeliveryAgent;
            _btnDelivery.BackColor = _deliveryCost > 0 ? AppTheme.AccentGreen : AppTheme.AccentOrange;
            RecalculateTotals();
        }
    }

    // ══════════════════════════════════════════════════════════════
    // HOLD / RETRIEVE
    // ══════════════════════════════════════════════════════════════
    private readonly List<HeldInvoice> _heldInvoices = new();

    private void HoldInvoice()
    {
        if (_cartItems.Count == 0) { ShowError("الفاتورة فارغة"); return; }
        _heldInvoices.Add(new HeldInvoice
        {
            Items        = new List<CartItem>(_cartItems),
            CustomerId   = _selectedCustomerId,
            CustomerName = _selectedCustomerName,
            HeldAt       = DateTime.Now
        });
        _cartItems.Clear();
        RefreshCartGrid();
        RecalculateTotals();
        ClearCustomer();
        ShowSuccess($"تم تعليق الفاتورة. عدد المعلقة: {_heldInvoices.Count}");
    }

    private void RetrieveInvoice()
    {
        if (_heldInvoices.Count == 0) { ShowError("لا توجد فواتير معلقة"); return; }
        using var dlg = new HeldInvoicePickerDialog(_heldInvoices);
        if (dlg.ShowDialog() == DialogResult.OK && dlg.SelectedIndex >= 0)
        {
            var held = _heldInvoices[dlg.SelectedIndex];
            _heldInvoices.RemoveAt(dlg.SelectedIndex);
            _cartItems.Clear();
            _cartItems.AddRange(held.Items);
            if (held.CustomerId.HasValue)
            {
                _selectedCustomerId   = held.CustomerId;
                _selectedCustomerName = held.CustomerName;
                _lblCustomer.Text     = $"👤 {_selectedCustomerName}";
                _lblCustomer.ForeColor = AppTheme.AccentBlue;
                _btnClearCustomer.Visible = true;
            }
            RefreshCartGrid();
            RecalculateTotals();
        }
    }

    // ══════════════════════════════════════════════════════════════
    // PROCESS SALE
    // ══════════════════════════════════════════════════════════════
    private void ProcessSale()
    {
        if (_cartItems.Count == 0) { ShowError("الفاتورة فارغة — أضف أصناف أولاً"); return; }
        if (!UserSession.Current.HasOpenShift) { ShowError("يجب فتح وردية أولاً"); return; }

        decimal.TryParse(_txtDiscount.Text, out var discPct);
        decimal.TryParse(_txtTax.Text,      out var taxPct);
        decimal.TryParse(_txtPaid.Text,     out var paid);
        var total = GetTotal();

        if (paid < total && _cmbPayMethod.SelectedIndex != 4 /*آجل*/)
        {
            if (!Confirm($"المبلغ المدفوع ({paid:N2}) أقل من الإجمالي ({total:N2}). هل تريد المتابعة كدين؟"))
                return;
        }

        var payMethod = _cmbPayMethod.SelectedIndex switch
        {
            0 => "Cash",
            1 => "Visa",
            2 => "BankTransfer",
            3 => "Wallet",
            4 => "Credit",
            5 => "Mixed",
            _ => "Cash"
        };

        var cmd = new CreateSaleInvoiceCommand(
            BranchId:        UserSession.Current.BranchId,
            WarehouseId:     UserSession.Current.WarehouseId,
            CashierId:       UserSession.Current.UserId,
            ShiftId:         UserSession.Current.ActiveShiftId!.Value,
            CustomerId:      _selectedCustomerId,
            Items:           _cartItems.Select(c => new SaleInvoiceItemDto(
                                 c.ProductId, c.Qty, c.Price, c.ItemDiscount)).ToList(),
            DiscountPercent: discPct,
            TaxPercent:      taxPct,
            Paid:            paid,
            PaymentMethod:   payMethod,
            DeliveryCost:    _deliveryCost,
            DeliveryAgent:   _deliveryAgent,
            Notes:           _txtNotes.Text.Trim()
        );

        _btnPay.Enabled = false;
        ShowLoading("جاري حفظ الفاتورة...");

        Task.Run(async () =>
        {
            try
            {
                var result = await _mediator.Send(cmd);
                InvokeOnUI(() =>
                {
                    HideLoading();
                    _btnPay.Enabled = true;

                    if (result.IsSuccess)
                    {
                        var invoiceNo = result.Value;
                        // Print receipt
                        _ = PrintReceiptAsync(invoiceNo);
                        // Clear cart for new invoice
                        _cartItems.Clear();
                        _deliveryCost  = 0;
                        _deliveryAgent = string.Empty;
                        _txtDiscount.Text = "0";
                        _txtTax.Text      = "0";
                        _txtNotes.Text    = string.Empty;
                        RefreshCartGrid();
                        RecalculateTotals();
                        ClearCustomer();
                        _txtBarcode.Focus();
                    }
                    else
                    {
                        ShowError("فشل حفظ الفاتورة: " + result.Error);
                    }
                });
            }
            catch (Exception ex)
            {
                InvokeOnUI(() =>
                {
                    HideLoading();
                    _btnPay.Enabled = true;
                    ShowError("خطأ: " + ex.Message);
                });
            }
        });
    }

    // ══════════════════════════════════════════════════════════════
    // PRINT
    // ══════════════════════════════════════════════════════════════
    private async Task PrintReceiptAsync(string invoiceNo)
    {
        // TODO: connect to PrintingService in Phase 10
        await Task.CompletedTask;
    }

    // ══════════════════════════════════════════════════════════════
    // KEYBOARD SHORTCUTS
    // ══════════════════════════════════════════════════════════════
    private void POSForm_KeyDown(object? s, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.F10: ProcessSale();    e.SuppressKeyPress = true; break;
            case Keys.F8:  HoldInvoice();    e.SuppressKeyPress = true; break;
            case Keys.F9:  RetrieveInvoice();e.SuppressKeyPress = true; break;
            case Keys.F5:  SelectCustomer(); e.SuppressKeyPress = true; break;
            case Keys.Escape:
                if (!string.IsNullOrEmpty(_txtBarcode.Text))
                { _txtBarcode.Clear(); e.SuppressKeyPress = true; }
                break;
        }
    }

    private MainForm? FindMainForm()
    {
        var parent = Parent;
        while (parent != null) { if (parent is MainForm mf) return mf; parent = parent.Parent; }
        return null;
    }
}

// ── Cart item model ────────────────────────────────────────────────
public class CartItem
{
    public int     ProductId    { get; set; }
    public string  Barcode      { get; set; } = string.Empty;
    public string  NameAr       { get; set; } = string.Empty;
    public string  UnitName     { get; set; } = string.Empty;
    public decimal Price        { get; set; }
    public decimal Qty          { get; set; }
    public decimal ItemDiscount { get; set; }
    public decimal LineTotal    { get; set; }
}

// ── Held invoice model ─────────────────────────────────────────────
public class HeldInvoice
{
    public List<CartItem> Items        { get; set; } = new();
    public int?           CustomerId   { get; set; }
    public string         CustomerName { get; set; } = string.Empty;
    public DateTime       HeldAt       { get; set; }
}
