using MediatR;
using CorePOS.WinForms.Theme;
using CorePOS.WinForms.Infrastructure;
using CorePOS.Application.Features.Customers.Queries;
using CorePOS.Application.Features.Customers.Commands;
using CorePOS.Application.Features.Suppliers.Queries;
using CorePOS.Application.Features.Suppliers.Commands;
using CorePOS.Application.Features.Employees.Queries;
using CorePOS.Application.Features.Employees.Commands;

namespace CorePOS.WinForms.Forms.MasterData;

// ════════════════════════════════════════════════════════════════════
// BASE MASTER DATA FORM (reusable for Customers / Suppliers)
// ════════════════════════════════════════════════════════════════════
public abstract class MasterDataListForm : BaseForm
{
    protected DataGridView Dgv        = null!;
    protected TextBox      TxtSearch  = null!;
    protected Label        LblCount   = null!;
    private System.Windows.Forms.Timer _debounce = null!;
    protected abstract string ModuleName { get; }
    protected abstract string ListTitle  { get; }

    protected MasterDataListForm(IMediator mediator) : base(mediator)
    {
        InitBaseComponent();
    }

    private void InitBaseComponent()
    {
        BackColor = AppTheme.BgContent;

        // Toolbar
        var pnlToolbar = new Panel
        {
            Dock = DockStyle.Top, Height = 52, BackColor = AppTheme.BgCard, Padding = new Padding(8)
        };

        TxtSearch = new TextBox
        {
            Dock = DockStyle.Right, Width = 260, Font = AppTheme.FontBody,
            BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "🔍 بحث..."
        };
        _debounce = new System.Windows.Forms.Timer { Interval = 350 };
        _debounce.Tick += (_, _) => { _debounce.Stop(); Reload(); };
        TxtSearch.TextChanged += (_, _) => { _debounce.Stop(); _debounce.Start(); };

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

        var btnAdd    = MakeBtn("➕ إضافة",  AppTheme.AccentGreen, (_, _) => AddNew());
        var btnEdit   = MakeBtn("✏ تعديل",   AppTheme.AccentBlue,  (_, _) => EditSelected());
        var btnDelete = MakeBtn("🗑 حذف",     AppTheme.AccentRed,   (_, _) => DeleteSelected());
        var btnView   = MakeBtn("📄 كشف حساب", AppTheme.AccentPurple, (_, _) => ViewStatement());

        btnAdd.Visible    = CanAdd(ModuleName);
        btnEdit.Visible   = CanEdit(ModuleName);
        btnDelete.Visible = CanDelete(ModuleName);

        flowBtns.Controls.AddRange([btnAdd, btnEdit, btnDelete, btnView]);
        pnlToolbar.Controls.AddRange([TxtSearch, flowBtns]);

        // Summary bar
        var pnlSummary = new Panel
        {
            Dock = DockStyle.Bottom, Height = 34, BackColor = AppTheme.BgCard, Padding = new Padding(8, 0, 8, 0)
        };
        LblCount = new Label
        {
            Text = "0", Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary,
            Dock = DockStyle.Right, AutoSize = false, Width = 200, TextAlign = ContentAlignment.MiddleRight
        };
        pnlSummary.Controls.Add(LblCount);

        // Grid
        Dgv = new DataGridView { Dock = DockStyle.Fill };
        AppTheme.StyleDataGrid(Dgv);
        Dgv.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) EditSelected(); };
        SetupColumns();

        Controls.AddRange([pnlSummary, Dgv, pnlToolbar]);

        Reload();
    }

    protected abstract void SetupColumns();
    protected abstract void Reload();
    protected abstract void AddNew();
    protected abstract void EditSelected();
    protected abstract void DeleteSelected();
    protected virtual void ViewStatement() { }
}

// ════════════════════════════════════════════════════════════════════
// CUSTOMERS FORM
// ════════════════════════════════════════════════════════════════════
public sealed class CustomersForm : MasterDataListForm
{
    protected override string ModuleName => Modules.Customers;
    protected override string ListTitle  => "العملاء";

    public CustomersForm(IMediator mediator) : base(mediator) { Text = "العملاء"; }

    protected override void SetupColumns()
    {
        Dgv.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "CustomerId", Visible = false },
            new DataGridViewTextBoxColumn { Name = "Name",    HeaderText = "الاسم",          FillWeight = 30 },
            new DataGridViewTextBoxColumn { Name = "Phone",   HeaderText = "الهاتف",          FillWeight = 18 },
            new DataGridViewTextBoxColumn { Name = "Address", HeaderText = "العنوان",         FillWeight = 20 },
            new DataGridViewTextBoxColumn { Name = "Balance", HeaderText = "الرصيد",          FillWeight = 12 },
            new DataGridViewTextBoxColumn { Name = "CreditLimit", HeaderText = "حد الائتمان", FillWeight = 12 },
            new DataGridViewTextBoxColumn { Name = "Points",  HeaderText = "نقاط الولاء",     FillWeight = 10 },
            new DataGridViewTextBoxColumn { Name = "Branch",  HeaderText = "الفرع",            FillWeight = 10 }
        );
    }

    protected override void Reload()
    {
        RunAsync(async () =>
        {
            var q      = new GetCustomersListQuery(TxtSearch.Text.Trim(), UserSession.Current.BranchId, 500);
            var result = await _mediator.Send(q);
            InvokeOnUI(() =>
            {
                Dgv.Rows.Clear();
                if (!result.IsSuccess || result.Value == null) return;
                foreach (var c in result.Value.Items)
                {
                    var rowIdx = Dgv.Rows.Add(
                        c.CustomerId, c.Name, c.Phone, c.Address,
                        $"{c.Balance:N2}", $"{c.CreditLimit:N2}", c.LoyaltyPoints, c.BranchName);
                    // Color negative balance
                    if (c.Balance > 0)
                        Dgv.Rows[rowIdx].Cells["Balance"].Style.ForeColor = AppTheme.AccentRed;
                }
                LblCount.Text = $"عدد العملاء: {result.Value.TotalCount:N0}";
            });
        });
    }

    protected override void AddNew()
    {
        using var dlg = new CustomerEditDialog(_mediator, null);
        if (dlg.ShowDialog() == DialogResult.OK) Reload();
    }

    protected override void EditSelected()
    {
        if (Dgv.CurrentRow == null) return;
        var id = (int)Dgv.CurrentRow.Cells["CustomerId"].Value;
        using var dlg = new CustomerEditDialog(_mediator, id);
        if (dlg.ShowDialog() == DialogResult.OK) Reload();
    }

    protected override void DeleteSelected()
    {
        if (Dgv.CurrentRow == null) return;
        var id   = (int)Dgv.CurrentRow.Cells["CustomerId"].Value;
        var name = Dgv.CurrentRow.Cells["Name"].Value?.ToString() ?? string.Empty;
        if (!Confirm($"هل تريد حذف العميل [{name}]؟")) return;
        RunAsync(async () =>
        {
            var result = await _mediator.Send(new DeleteCustomerCommand(id, UserSession.Current.UserId));
            InvokeOnUI(() => { if (result.IsSuccess) Reload(); else ShowError(result.Error); });
        });
    }

    protected override void ViewStatement()
    {
        if (Dgv.CurrentRow == null) return;
        var id = (int)Dgv.CurrentRow.Cells["CustomerId"].Value;
        using var dlg = new CustomerStatementDialog(_mediator, id);
        dlg.ShowDialog();
    }
}

// ────────────────────────────────────────────────────────────────────
// CUSTOMER EDIT DIALOG
// ────────────────────────────────────────────────────────────────────
public sealed class CustomerEditDialog : Form
{
    private readonly IMediator _mediator;
    private readonly int?      _customerId;

    private TextBox _txtName       = null!;
    private TextBox _txtPhone      = null!;
    private TextBox _txtAddress    = null!;
    private TextBox _txtEmail      = null!;
    private TextBox _txtInstaPay   = null!;
    private TextBox _txtTaxNo      = null!;
    private TextBox _txtCreditLim  = null!;
    private TextBox _txtPayPeriod  = null!;
    private ComboBox _cmbBranch    = null!;

    public CustomerEditDialog(IMediator mediator, int? customerId)
    {
        _mediator   = mediator;
        _customerId = customerId;
        InitializeComponent();
        if (customerId.HasValue) LoadCustomer(customerId.Value);
    }

    private void InitializeComponent()
    {
        Text = _customerId.HasValue ? "تعديل عميل" : "إضافة عميل جديد";
        Size = new Size(520, 500); FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false; StartPosition = FormStartPosition.CenterParent;
        BackColor = AppTheme.BgCard; RightToLeft = RightToLeft.Yes; RightToLeftLayout = true;

        var pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16), BackColor = AppTheme.BgCard };
        int y = 12;

        void Row(string lbl, out TextBox tb, string ph = "")
        {
            pnl.Controls.Add(new Label { Text = lbl, Location = new Point(16, y), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel });
            y += 20;
            tb = new TextBox { Location = new Point(16, y), Width = 460, Height = AppTheme.InputHeight, Font = AppTheme.FontBody, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = ph };
            pnl.Controls.Add(tb);
            y += 44;
        }

        Row("الاسم (مطلوب):",         out _txtName,      "اسم العميل");
        Row("الهاتف:",                  out _txtPhone,     "01xxxxxxxxx");
        Row("العنوان:",                 out _txtAddress,   "العنوان");
        Row("البريد الإلكتروني:",      out _txtEmail,     "email@example.com");
        Row("رقم انستا باي:",           out _txtInstaPay,  "رقم الهاتف أو الحساب");
        Row("الرقم الضريبي:",           out _txtTaxNo,     "اختياري");

        // Credit limit + Payment period side by side
        pnl.Controls.Add(new Label { Text = "حد الائتمان:", Location = new Point(16, y), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel });
        pnl.Controls.Add(new Label { Text = "فترة السداد (يوم):", Location = new Point(256, y), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel });
        y += 20;
        _txtCreditLim = new TextBox { Location = new Point(16, y),  Width = 220, Height = AppTheme.InputHeight, Font = AppTheme.FontBody, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "0.00" };
        _txtPayPeriod = new TextBox { Location = new Point(256, y), Width = 220, Height = AppTheme.InputHeight, Font = AppTheme.FontBody, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "30" };
        pnl.Controls.AddRange([_txtCreditLim, _txtPayPeriod]);
        y += 44;

        pnl.Controls.Add(new Label { Text = "الفرع:", Location = new Point(16, y), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel });
        y += 20;
        _cmbBranch = new ComboBox { Location = new Point(16, y), Width = 460, DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody };
        _cmbBranch.Items.Add(new ComboItem(UserSession.Current.BranchId, UserSession.Current.BranchName));
        _cmbBranch.SelectedIndex = 0;
        pnl.Controls.Add(_cmbBranch);

        var pnlBtns = new Panel { Dock = DockStyle.Bottom, Height = 52, BackColor = AppTheme.BgCard, Padding = new Padding(12, 8, 12, 8) };
        var btnSave = new Button
        {
            Text = "💾 حفظ", Dock = DockStyle.Right, Width = 110, Height = 34,
            FlatStyle = FlatStyle.Flat, BackColor = AppTheme.AccentGreen, ForeColor = Color.White,
            Font = AppTheme.FontBodyBold, Cursor = Cursors.Hand
        };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += (_, _) => Save();
        var btnCancel = new Button
        {
            Text = "إلغاء", Dock = DockStyle.Left, Width = 90, Height = 34,
            FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = AppTheme.TextSecondary,
            Font = AppTheme.FontBody, Cursor = Cursors.Hand
        };
        btnCancel.FlatAppearance.BorderColor = AppTheme.Border;
        btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        pnlBtns.Controls.AddRange([btnSave, btnCancel]);

        Controls.AddRange([pnlBtns, pnl]);
    }

    private void LoadCustomer(int id)
    {
        Task.Run(async () =>
        {
            var result = await _mediator.Send(new GetCustomerByIdQuery(id));
            Invoke(() =>
            {
                if (!result.IsSuccess || result.Value == null) return;
                var c = result.Value;
                _txtName.Text      = c.Name;
                _txtPhone.Text     = c.Phone;
                _txtAddress.Text   = c.Address;
                _txtEmail.Text     = c.Email;
                _txtInstaPay.Text  = c.InstaPayNo;
                _txtTaxNo.Text     = c.TaxNo;
                _txtCreditLim.Text = c.CreditLimit.ToString("N2");
                _txtPayPeriod.Text = c.PaymentPeriodDays.ToString();
            });
        });
    }

    private void Save()
    {
        if (string.IsNullOrWhiteSpace(_txtName.Text))
        {
            MessageBox.Show("اسم العميل مطلوب", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        decimal.TryParse(_txtCreditLim.Text, out var credit);
        int.TryParse(_txtPayPeriod.Text, out var period);
        var branchItem = _cmbBranch.SelectedItem as ComboItem;

        Task.Run(async () =>
        {
            object cmd = _customerId.HasValue
                ? new UpdateCustomerCommand(_customerId.Value, _txtName.Text.Trim(), _txtPhone.Text.Trim(),
                    _txtAddress.Text.Trim(), _txtEmail.Text.Trim(), _txtInstaPay.Text.Trim(),
                    _txtTaxNo.Text.Trim(), credit, period, branchItem?.Id ?? 0, UserSession.Current.UserId)
                : new CreateCustomerCommand(_txtName.Text.Trim(), _txtPhone.Text.Trim(),
                    _txtAddress.Text.Trim(), _txtEmail.Text.Trim(), _txtInstaPay.Text.Trim(),
                    _txtTaxNo.Text.Trim(), credit, period, branchItem?.Id ?? 0, UserSession.Current.UserId);

            var result = cmd is UpdateCustomerCommand uc
                ? await _mediator.Send(uc)
                : await _mediator.Send((CreateCustomerCommand)cmd);

            Invoke(() =>
            {
                if (result.IsSuccess) { DialogResult = DialogResult.OK; Close(); }
                else MessageBox.Show(result.Error, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        });
    }
}

// ════════════════════════════════════════════════════════════════════
// CUSTOMER STATEMENT DIALOG
// ════════════════════════════════════════════════════════════════════
public sealed class CustomerStatementDialog : Form
{
    private readonly IMediator _mediator;
    private readonly int       _customerId;
    private DataGridView _dgv = null!;
    private Label _lblBalance  = null!;

    public CustomerStatementDialog(IMediator mediator, int customerId)
    {
        _mediator   = mediator;
        _customerId = customerId;
        InitializeComponent();
        LoadStatement();
    }

    private void InitializeComponent()
    {
        Text = "كشف حساب عميل"; Size = new Size(700, 520);
        FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false;
        StartPosition = FormStartPosition.CenterParent; BackColor = AppTheme.BgContent;
        RightToLeft = RightToLeft.Yes; RightToLeftLayout = true;

        var pnlTop = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = AppTheme.BgCard, Padding = new Padding(12, 8, 12, 8) };
        _lblBalance = new Label { Font = AppTheme.FontH2, ForeColor = AppTheme.AccentBlue, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight };
        pnlTop.Controls.Add(_lblBalance);

        _dgv = new DataGridView { Dock = DockStyle.Fill };
        AppTheme.StyleDataGrid(_dgv);
        _dgv.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "Date",      HeaderText = "التاريخ",      FillWeight = 20 },
            new DataGridViewTextBoxColumn { Name = "Type",      HeaderText = "النوع",         FillWeight = 20 },
            new DataGridViewTextBoxColumn { Name = "Ref",       HeaderText = "المرجع",        FillWeight = 20 },
            new DataGridViewTextBoxColumn { Name = "Debit",     HeaderText = "مدين",          FillWeight = 15 },
            new DataGridViewTextBoxColumn { Name = "Credit",    HeaderText = "دائن",          FillWeight = 15 },
            new DataGridViewTextBoxColumn { Name = "Balance",   HeaderText = "الرصيد",        FillWeight = 15 }
        );

        var btnClose = new Button
        {
            Text = "إغلاق", Dock = DockStyle.Bottom, Height = 44,
            FlatStyle = FlatStyle.Flat, BackColor = AppTheme.BgContent,
            ForeColor = AppTheme.TextSecondary, Font = AppTheme.FontBody, Cursor = Cursors.Hand
        };
        btnClose.FlatAppearance.BorderSize = 0;
        btnClose.Click += (_, _) => Close();
        Controls.AddRange([btnClose, _dgv, pnlTop]);
    }

    private void LoadStatement()
    {
        Task.Run(async () =>
        {
            var result = await _mediator.Send(new GetCustomerStatementQuery(_customerId));
            Invoke(() =>
            {
                if (!result.IsSuccess || result.Value == null) return;
                var stmt = result.Value;
                _lblBalance.Text = $"الرصيد: {stmt.CurrentBalance:N2} ج.م";
                _dgv.Rows.Clear();
                foreach (var t in stmt.Transactions)
                {
                    _dgv.Rows.Add(
                        t.Date.ToString("dd/MM/yyyy"),
                        t.TypeAr, t.Reference,
                        t.Debit  > 0 ? $"{t.Debit:N2}"  : string.Empty,
                        t.Credit > 0 ? $"{t.Credit:N2}" : string.Empty,
                        $"{t.Balance:N2}"
                    );
                }
            });
        });
    }
}

// ════════════════════════════════════════════════════════════════════
// SUPPLIERS FORM
// ════════════════════════════════════════════════════════════════════
public sealed class SuppliersForm : MasterDataListForm
{
    protected override string ModuleName => Modules.Suppliers;
    protected override string ListTitle  => "الموردين";

    public SuppliersForm(IMediator mediator) : base(mediator) { Text = "الموردين"; }

    protected override void SetupColumns()
    {
        Dgv.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "SupplierId", Visible = false },
            new DataGridViewTextBoxColumn { Name = "Name",       HeaderText = "اسم المورد",  FillWeight = 30 },
            new DataGridViewTextBoxColumn { Name = "Phone",      HeaderText = "الهاتف",       FillWeight = 18 },
            new DataGridViewTextBoxColumn { Name = "Address",    HeaderText = "العنوان",      FillWeight = 20 },
            new DataGridViewTextBoxColumn { Name = "Balance",    HeaderText = "الرصيد",       FillWeight = 14 },
            new DataGridViewTextBoxColumn { Name = "TaxNo",      HeaderText = "الرقم الضريبي",FillWeight = 18 }
        );
    }

    protected override void Reload()
    {
        RunAsync(async () =>
        {
            var result = await _mediator.Send(new GetSuppliersListQuery(TxtSearch.Text.Trim(), 500));
            InvokeOnUI(() =>
            {
                Dgv.Rows.Clear();
                if (!result.IsSuccess || result.Value == null) return;
                foreach (var s in result.Value.Items)
                    Dgv.Rows.Add(s.SupplierId, s.Name, s.Phone, s.Address, $"{s.Balance:N2}", s.TaxNo);
                LblCount.Text = $"عدد الموردين: {result.Value.TotalCount:N0}";
            });
        });
    }

    protected override void AddNew()
    {
        using var dlg = new SupplierEditDialog(_mediator, null);
        if (dlg.ShowDialog() == DialogResult.OK) Reload();
    }

    protected override void EditSelected()
    {
        if (Dgv.CurrentRow == null) return;
        var id = (int)Dgv.CurrentRow.Cells["SupplierId"].Value;
        using var dlg = new SupplierEditDialog(_mediator, id);
        if (dlg.ShowDialog() == DialogResult.OK) Reload();
    }

    protected override void DeleteSelected()
    {
        if (Dgv.CurrentRow == null) return;
        var id = (int)Dgv.CurrentRow.Cells["SupplierId"].Value;
        if (!Confirm("هل تريد حذف هذا المورد؟")) return;
        RunAsync(async () =>
        {
            var result = await _mediator.Send(new DeleteSupplierCommand(id, UserSession.Current.UserId));
            InvokeOnUI(() => { if (result.IsSuccess) Reload(); else ShowError(result.Error); });
        });
    }
}

// ── Supplier Edit Dialog (simple) ─────────────────────────────────
public sealed class SupplierEditDialog : Form
{
    private readonly IMediator _mediator;
    private readonly int?      _supplierId;
    private TextBox _txtName = null!, _txtPhone = null!, _txtAddress = null!, _txtTaxNo = null!, _txtEmail = null!;

    public SupplierEditDialog(IMediator mediator, int? supplierId)
    {
        _mediator   = mediator;
        _supplierId = supplierId;
        InitializeComponent();
        if (supplierId.HasValue) LoadSupplier(supplierId.Value);
    }

    private void InitializeComponent()
    {
        Text = _supplierId.HasValue ? "تعديل مورد" : "إضافة مورد";
        Size = new Size(480, 380); FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false; StartPosition = FormStartPosition.CenterParent;
        BackColor = AppTheme.BgCard; RightToLeft = RightToLeft.Yes; RightToLeftLayout = true;

        var pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16) };
        int y = 12;
        void Row(string lbl, out TextBox tb)
        {
            pnl.Controls.Add(new Label { Text = lbl, Location = new Point(16, y), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel });
            y += 20;
            tb = new TextBox { Location = new Point(16, y), Width = 420, Height = AppTheme.InputHeight, Font = AppTheme.FontBody, BorderStyle = BorderStyle.FixedSingle };
            pnl.Controls.Add(tb);
            y += 44;
        }
        Row("اسم المورد (مطلوب):", out _txtName);
        Row("الهاتف:",              out _txtPhone);
        Row("العنوان:",             out _txtAddress);
        Row("الرقم الضريبي:",      out _txtTaxNo);
        Row("البريد الإلكتروني:", out _txtEmail);

        var pnlBtns = new Panel { Dock = DockStyle.Bottom, Height = 52, BackColor = AppTheme.BgCard, Padding = new Padding(12, 8, 12, 8) };
        var btnSave = new Button { Text = "💾 حفظ", Dock = DockStyle.Right, Width = 110, Height = 34, FlatStyle = FlatStyle.Flat, BackColor = AppTheme.AccentGreen, ForeColor = Color.White, Font = AppTheme.FontBodyBold, Cursor = Cursors.Hand };
        btnSave.FlatAppearance.BorderSize = 0; btnSave.Click += (_, _) => Save();
        var btnCancel = new Button { Text = "إلغاء", Dock = DockStyle.Left, Width = 90, Height = 34, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = AppTheme.TextSecondary, Font = AppTheme.FontBody, Cursor = Cursors.Hand };
        btnCancel.FlatAppearance.BorderColor = AppTheme.Border;
        btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        pnlBtns.Controls.AddRange([btnSave, btnCancel]);
        Controls.AddRange([pnlBtns, pnl]);
    }

    private void LoadSupplier(int id)
    {
        Task.Run(async () =>
        {
            var result = await _mediator.Send(new GetSupplierByIdQuery(id));
            Invoke(() =>
            {
                if (!result.IsSuccess || result.Value == null) return;
                _txtName.Text    = result.Value.Name;
                _txtPhone.Text   = result.Value.Phone;
                _txtAddress.Text = result.Value.Address;
                _txtTaxNo.Text   = result.Value.TaxNo;
                _txtEmail.Text   = result.Value.Email;
            });
        });
    }

    private void Save()
    {
        if (string.IsNullOrWhiteSpace(_txtName.Text)) { MessageBox.Show("اسم المورد مطلوب", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        Task.Run(async () =>
        {
            object cmd = _supplierId.HasValue
                ? new UpdateSupplierCommand(_supplierId.Value, _txtName.Text.Trim(), _txtPhone.Text.Trim(), _txtAddress.Text.Trim(), _txtTaxNo.Text.Trim(), _txtEmail.Text.Trim(), UserSession.Current.UserId)
                : new CreateSupplierCommand(_txtName.Text.Trim(), _txtPhone.Text.Trim(), _txtAddress.Text.Trim(), _txtTaxNo.Text.Trim(), _txtEmail.Text.Trim(), UserSession.Current.UserId);
            var result = cmd is UpdateSupplierCommand uc ? await _mediator.Send(uc) : await _mediator.Send((CreateSupplierCommand)cmd);
            Invoke(() => { if (result.IsSuccess) { DialogResult = DialogResult.OK; Close(); } else MessageBox.Show(result.Error, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error); });
        });
    }
}

// ════════════════════════════════════════════════════════════════════
// EMPLOYEES FORM
// ════════════════════════════════════════════════════════════════════
public sealed class EmployeesForm : BaseForm
{
    private DataGridView _dgv   = null!;
    private TextBox _txtSearch  = null!;
    private Label   _lblCount   = null!;

    public EmployeesForm(IMediator mediator) : base(mediator)
    {
        Text      = "الموظفين";
        BackColor = AppTheme.BgContent;
        InitializeComponent();
        Load();
    }

    private void InitializeComponent()
    {
        var pnlToolbar = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = AppTheme.BgCard, Padding = new Padding(8) };
        _txtSearch = new TextBox { Dock = DockStyle.Right, Width = 240, Font = AppTheme.FontBody, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "🔍 بحث باسم الموظف..." };
        var debounce = new System.Windows.Forms.Timer { Interval = 350 };
        debounce.Tick += (_, _) => { debounce.Stop(); Load(); };
        _txtSearch.TextChanged += (_, _) => { debounce.Stop(); debounce.Start(); };

        var flowBtns = new FlowLayoutPanel { Dock = DockStyle.Left, Width = 320, FlowDirection = FlowDirection.LeftToRight, BackColor = AppTheme.BgCard, WrapContents = false };
        Button Btn(string t, Color c, EventHandler h) { var b = new Button { Text = t, Width = 100, Height = 34, FlatStyle = FlatStyle.Flat, BackColor = c, ForeColor = Color.White, Font = AppTheme.FontSmall, Cursor = Cursors.Hand, Margin = new Padding(0, 0, 6, 0) }; b.FlatAppearance.BorderSize = 0; b.Click += h; return b; }
        flowBtns.Controls.AddRange([
            Btn("➕ إضافة", AppTheme.AccentGreen,  (_, _) => EditEmployee(null)),
            Btn("✏ تعديل",  AppTheme.AccentBlue,   (_, _) => EditEmployeeSelected()),
            Btn("🗑 حذف",    AppTheme.AccentRed,    (_, _) => DeleteEmployee())
        ]);
        pnlToolbar.Controls.AddRange([_txtSearch, flowBtns]);

        var pnlSummary = new Panel { Dock = DockStyle.Bottom, Height = 34, BackColor = AppTheme.BgCard, Padding = new Padding(8, 0, 8, 0) };
        _lblCount = new Label { Dock = DockStyle.Right, AutoSize = false, Width = 200, TextAlign = ContentAlignment.MiddleRight, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextSecondary };
        pnlSummary.Controls.Add(_lblCount);

        _dgv = new DataGridView { Dock = DockStyle.Fill };
        AppTheme.StyleDataGrid(_dgv);
        _dgv.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "EmployeeId", Visible = false },
            new DataGridViewTextBoxColumn { Name = "Name",    HeaderText = "الاسم",        FillWeight = 30 },
            new DataGridViewTextBoxColumn { Name = "JobTitle", HeaderText = "الوظيفة",     FillWeight = 20 },
            new DataGridViewTextBoxColumn { Name = "Phone",   HeaderText = "الهاتف",        FillWeight = 18 },
            new DataGridViewTextBoxColumn { Name = "Salary",  HeaderText = "الراتب",        FillWeight = 12 },
            new DataGridViewTextBoxColumn { Name = "HireDate",HeaderText = "تاريخ التعيين", FillWeight = 15 },
            new DataGridViewTextBoxColumn { Name = "Status",  HeaderText = "الحالة",         FillWeight = 10 }
        );
        _dgv.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) EditEmployeeSelected(); };

        Controls.AddRange([pnlSummary, _dgv, pnlToolbar]);
    }

    private new void Load()
    {
        RunAsync(async () =>
        {
            var result = await _mediator.Send(new GetEmployeesListQuery(_txtSearch.Text.Trim()));
            InvokeOnUI(() =>
            {
                _dgv.Rows.Clear();
                if (!result.IsSuccess || result.Value == null) return;
                foreach (var e in result.Value)
                    _dgv.Rows.Add(e.EmployeeId, e.Name, e.JobTitle, e.Phone,
                        $"{e.Salary:N2}", e.HireDate.ToString("dd/MM/yyyy"), e.IsActive ? "نشط" : "موقوف");
                _lblCount.Text = $"عدد الموظفين: {result.Value.Count:N0}";
            });
        });
    }

    private void EditEmployee(int? id)
    {
        using var dlg = new EmployeeEditDialog(_mediator, id);
        if (dlg.ShowDialog() == DialogResult.OK) Load();
    }

    private void EditEmployeeSelected()
    {
        if (_dgv.CurrentRow == null) return;
        EditEmployee((int)_dgv.CurrentRow.Cells["EmployeeId"].Value);
    }

    private void DeleteEmployee()
    {
        if (_dgv.CurrentRow == null) return;
        var id = (int)_dgv.CurrentRow.Cells["EmployeeId"].Value;
        if (!Confirm("هل تريد حذف هذا الموظف؟")) return;
        RunAsync(async () =>
        {
            var result = await _mediator.Send(new DeleteEmployeeCommand(id, UserSession.Current.UserId));
            InvokeOnUI(() => { if (result.IsSuccess) Load(); else ShowError(result.Error); });
        });
    }
}

// ── Employee Edit Dialog ──────────────────────────────────────────
public sealed class EmployeeEditDialog : Form
{
    private readonly IMediator _mediator;
    private readonly int?      _employeeId;
    private TextBox _txtName = null!, _txtJob = null!, _txtPhone = null!, _txtSalary = null!;
    private DateTimePicker _dtHire = null!;
    private CheckBox _chkActive    = null!;

    public EmployeeEditDialog(IMediator mediator, int? employeeId)
    {
        _mediator   = mediator;
        _employeeId = employeeId;
        InitializeComponent();
        if (employeeId.HasValue) LoadEmployee(employeeId.Value);
    }

    private void InitializeComponent()
    {
        Text = _employeeId.HasValue ? "تعديل موظف" : "إضافة موظف";
        Size = new Size(460, 360); FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false; StartPosition = FormStartPosition.CenterParent;
        BackColor = AppTheme.BgCard; RightToLeft = RightToLeft.Yes; RightToLeftLayout = true;

        var pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16) };
        int y = 12;
        void Row(string lbl, out TextBox tb)
        {
            pnl.Controls.Add(new Label { Text = lbl, Location = new Point(16, y), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel });
            y += 20; tb = new TextBox { Location = new Point(16, y), Width = 400, Height = AppTheme.InputHeight, Font = AppTheme.FontBody, BorderStyle = BorderStyle.FixedSingle };
            pnl.Controls.Add(tb); y += 44;
        }
        Row("الاسم (مطلوب):", out _txtName);
        Row("الوظيفة:",        out _txtJob);
        Row("الهاتف:",         out _txtPhone);
        Row("الراتب:",         out _txtSalary);
        pnl.Controls.Add(new Label { Text = "تاريخ التعيين:", Location = new Point(16, y), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel });
        y += 20;
        _dtHire = new DateTimePicker { Location = new Point(16, y), Width = 200, Format = DateTimePickerFormat.Short };
        _chkActive = new CheckBox { Text = "موظف نشط", Location = new Point(240, y + 4), AutoSize = true, Font = AppTheme.FontBody, Checked = true };
        pnl.Controls.AddRange([_dtHire, _chkActive]);

        var pnlBtns = new Panel { Dock = DockStyle.Bottom, Height = 52, BackColor = AppTheme.BgCard, Padding = new Padding(12, 8, 12, 8) };
        var btnSave = new Button { Text = "💾 حفظ", Dock = DockStyle.Right, Width = 110, Height = 34, FlatStyle = FlatStyle.Flat, BackColor = AppTheme.AccentGreen, ForeColor = Color.White, Font = AppTheme.FontBodyBold, Cursor = Cursors.Hand };
        btnSave.FlatAppearance.BorderSize = 0; btnSave.Click += (_, _) => Save();
        var btnCancel = new Button { Text = "إلغاء", Dock = DockStyle.Left, Width = 90, Height = 34, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = AppTheme.TextSecondary, Font = AppTheme.FontBody, Cursor = Cursors.Hand };
        btnCancel.FlatAppearance.BorderColor = AppTheme.Border;
        btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        pnlBtns.Controls.AddRange([btnSave, btnCancel]);
        Controls.AddRange([pnlBtns, pnl]);
    }

    private void LoadEmployee(int id)
    {
        Task.Run(async () =>
        {
            var result = await _mediator.Send(new GetEmployeeByIdQuery(id));
            Invoke(() =>
            {
                if (!result.IsSuccess || result.Value == null) return;
                var e = result.Value;
                _txtName.Text   = e.Name;
                _txtJob.Text    = e.JobTitle;
                _txtPhone.Text  = e.Phone;
                _txtSalary.Text = e.Salary.ToString("N2");
                _dtHire.Value   = e.HireDate;
                _chkActive.Checked = e.IsActive;
            });
        });
    }

    private void Save()
    {
        if (string.IsNullOrWhiteSpace(_txtName.Text)) { MessageBox.Show("اسم الموظف مطلوب", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        decimal.TryParse(_txtSalary.Text, out var salary);
        Task.Run(async () =>
        {
            object cmd = _employeeId.HasValue
                ? new UpdateEmployeeCommand(_employeeId.Value, _txtName.Text.Trim(), _txtJob.Text.Trim(), _txtPhone.Text.Trim(), salary, _dtHire.Value, _chkActive.Checked, UserSession.Current.UserId)
                : new CreateEmployeeCommand(_txtName.Text.Trim(), _txtJob.Text.Trim(), _txtPhone.Text.Trim(), salary, _dtHire.Value, UserSession.Current.UserId);
            var result = cmd is UpdateEmployeeCommand uc ? await _mediator.Send(uc) : await _mediator.Send((CreateEmployeeCommand)cmd);
            Invoke(() => { if (result.IsSuccess) { DialogResult = DialogResult.OK; Close(); } else MessageBox.Show(result.Error, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error); });
        });
    }
}
