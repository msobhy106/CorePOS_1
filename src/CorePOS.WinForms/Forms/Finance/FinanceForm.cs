using MediatR;
using CorePOS.WinForms.Theme;
using CorePOS.WinForms.Infrastructure;
using CorePOS.Application.Features.Finance.Queries;
using CorePOS.Application.Features.Finance.Commands;

namespace CorePOS.WinForms.Forms.Finance;

/// <summary>
/// Finance/Treasury management screen.
/// Tabs: 1.الخزائن  2.عمليات  3.مصروفات  4.إقفال يومي
/// </summary>
public sealed class FinanceForm : BaseForm
{
    private TabControl _tabs = null!;

    // Tab 1 — Cashboxes
    private DataGridView _dgvCashboxes = null!;

    // Tab 2 — Operations
    private ComboBox  _cmbCashbox   = null!;
    private ComboBox  _cmbOpType    = null!;
    private TextBox   _txtAmount    = null!;
    private TextBox   _txtOpNotes   = null!;
    private ComboBox  _cmbTargetBox = null!;
    private Panel     _pnlTarget    = null!;
    private DataGridView _dgvOps    = null!;

    // Tab 3 — Expenses
    private DataGridView _dgvExpenses = null!;
    private ComboBox     _cmbExpType  = null!;
    private TextBox      _txtExpAmt   = null!;
    private TextBox      _txtExpDesc  = null!;

    // Tab 4 — Daily close
    private Label _lblCloseDate    = null!;
    private Label _lblOpenBalance  = null!;
    private Label _lblTotalSales   = null!;
    private Label _lblTotalExpenses= null!;
    private Label _lblCloseBalance = null!;

    public FinanceForm(IMediator mediator) : base(mediator)
    {
        Text      = "الخزنة والمالية";
        BackColor = AppTheme.BgContent;
        InitializeComponent();
        LoadCashboxes();
    }

    private void InitializeComponent()
    {
        _tabs = new TabControl { Dock = DockStyle.Fill, Font = AppTheme.FontBody };
        _tabs.TabPages.Add(BuildCashboxesTab());
        _tabs.TabPages.Add(BuildOperationsTab());
        _tabs.TabPages.Add(BuildExpensesTab());
        _tabs.TabPages.Add(BuildDailyCloseTab());
        _tabs.SelectedIndexChanged += (_, _) =>
        {
            switch (_tabs.SelectedIndex)
            {
                case 0: LoadCashboxes();   break;
                case 1: LoadOperations();  break;
                case 2: LoadExpenses();    break;
                case 3: LoadCloseData();   break;
            }
        };
        Controls.Add(_tabs);
    }

    // ══════════════════════════════════════════════════════════════
    // TAB 1 — CASHBOXES
    // ══════════════════════════════════════════════════════════════
    private TabPage BuildCashboxesTab()
    {
        var tab = new TabPage("الخزائن") { BackColor = AppTheme.BgContent };

        var pnlBtns = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = AppTheme.BgCard, Padding = new Padding(8) };
        var flow    = new FlowLayoutPanel { Dock = DockStyle.Left, Width = 340, FlowDirection = FlowDirection.LeftToRight, BackColor = AppTheme.BgCard, WrapContents = false };

        Button Btn(string t, Color c, EventHandler h) { var b = new Button { Text = t, Width = 110, Height = 34, FlatStyle = FlatStyle.Flat, BackColor = c, ForeColor = Color.White, Font = AppTheme.FontSmall, Cursor = Cursors.Hand, Margin = new Padding(0, 0, 6, 0) }; b.FlatAppearance.BorderSize = 0; b.Click += h; return b; }
        flow.Controls.AddRange([
            Btn("➕ خزنة جديدة", AppTheme.AccentGreen, (_, _) => AddCashbox()),
            Btn("✏ تعديل",        AppTheme.AccentBlue,  (_, _) => EditCashbox()),
            Btn("🔄 تحديث",       AppTheme.AccentPurple,(_, _) => LoadCashboxes())
        ]);
        pnlBtns.Controls.Add(flow);

        _dgvCashboxes = new DataGridView { Dock = DockStyle.Fill };
        AppTheme.StyleDataGrid(_dgvCashboxes);
        _dgvCashboxes.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "CashboxId",  Visible = false },
            new DataGridViewTextBoxColumn { Name = "Name",        HeaderText = "اسم الخزنة",     FillWeight = 30 },
            new DataGridViewTextBoxColumn { Name = "Type",        HeaderText = "النوع",            FillWeight = 15 },
            new DataGridViewTextBoxColumn { Name = "Balance",     HeaderText = "الرصيد الحالي",   FillWeight = 20 },
            new DataGridViewTextBoxColumn { Name = "OpenBalance", HeaderText = "رصيد البداية",    FillWeight = 20 },
            new DataGridViewTextBoxColumn { Name = "Branch",      HeaderText = "الفرع",            FillWeight = 15 }
        );

        tab.Controls.AddRange([_dgvCashboxes, pnlBtns]);
        return tab;
    }

    // ══════════════════════════════════════════════════════════════
    // TAB 2 — OPERATIONS
    // ══════════════════════════════════════════════════════════════
    private TabPage BuildOperationsTab()
    {
        var tab = new TabPage("العمليات") { BackColor = AppTheme.BgContent };

        // Form area
        var pnlForm = new Panel { Dock = DockStyle.Top, Height = 160, BackColor = AppTheme.BgCard, Padding = new Padding(16, 12, 16, 8) };

        int y = 10;
        void AddField(string lbl, Control ctrl, int x, int iy)
        {
            pnlForm.Controls.Add(new Label { Text = lbl, Location = new Point(x, iy), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel });
            ctrl.Location = new Point(x, iy + 20);
            pnlForm.Controls.Add(ctrl);
        }

        _cmbCashbox = new ComboBox { Width = 200, DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody };
        AddField("الخزنة:", _cmbCashbox, 16, y);

        _cmbOpType = new ComboBox { Width = 160, DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody };
        _cmbOpType.Items.AddRange(["إيداع", "سحب", "تحويل"]);
        _cmbOpType.SelectedIndex = 0;
        _cmbOpType.SelectedIndexChanged += (_, _) => _pnlTarget.Visible = _cmbOpType.SelectedIndex == 2;
        AddField("نوع العملية:", _cmbOpType, 240, y);

        _txtAmount = new TextBox { Width = 160, Height = AppTheme.InputHeight, Font = AppTheme.FontBody, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "0.00" };
        AddField("المبلغ:", _txtAmount, 430, y);

        y += 68;
        _pnlTarget = new Panel { Location = new Point(16, y), Size = new Size(220, 60), BackColor = AppTheme.BgCard, Visible = false };
        _cmbTargetBox = new ComboBox { Width = 200, DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody, Location = new Point(0, 24) };
        _pnlTarget.Controls.AddRange([new Label { Text = "إلى الخزنة:", AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel }, _cmbTargetBox]);
        pnlForm.Controls.Add(_pnlTarget);

        _txtOpNotes = new TextBox { Location = new Point(260, y), Width = 360, Height = AppTheme.InputHeight, Font = AppTheme.FontBody, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "ملاحظات..." };
        pnlForm.Controls.Add(new Label { Text = "ملاحظات:", Location = new Point(260, y - 18), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel });
        pnlForm.Controls.Add(_txtOpNotes);

        var btnSave = new Button
        {
            Text = "💾 حفظ العملية", Location = new Point(16, y + 44), Width = 160, Height = AppTheme.ButtonHeight,
            FlatStyle = FlatStyle.Flat, BackColor = AppTheme.AccentGreen, ForeColor = Color.White,
            Font = AppTheme.FontBodyBold, Cursor = Cursors.Hand
        };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += (_, _) => SaveOperation();
        pnlForm.Controls.Add(btnSave);

        // Operations history grid
        _dgvOps = new DataGridView { Dock = DockStyle.Fill };
        AppTheme.StyleDataGrid(_dgvOps);
        _dgvOps.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "Date",     HeaderText = "التاريخ",    FillWeight = 18 },
            new DataGridViewTextBoxColumn { Name = "Type",     HeaderText = "النوع",       FillWeight = 15 },
            new DataGridViewTextBoxColumn { Name = "Cashbox",  HeaderText = "الخزنة",      FillWeight = 18 },
            new DataGridViewTextBoxColumn { Name = "Amount",   HeaderText = "المبلغ",      FillWeight = 15 },
            new DataGridViewTextBoxColumn { Name = "Balance",  HeaderText = "الرصيد بعد", FillWeight = 15 },
            new DataGridViewTextBoxColumn { Name = "Notes",    HeaderText = "ملاحظات",     FillWeight = 20 },
            new DataGridViewTextBoxColumn { Name = "ByUser",   HeaderText = "بواسطة",      FillWeight = 12 }
        );

        tab.Controls.AddRange([_dgvOps, pnlForm]);
        return tab;
    }

    // ══════════════════════════════════════════════════════════════
    // TAB 3 — EXPENSES
    // ══════════════════════════════════════════════════════════════
    private TabPage BuildExpensesTab()
    {
        var tab = new TabPage("المصروفات") { BackColor = AppTheme.BgContent };

        var pnlForm = new Panel { Dock = DockStyle.Top, Height = 96, BackColor = AppTheme.BgCard, Padding = new Padding(16, 10, 16, 8) };

        _cmbExpType = new ComboBox { Location = new Point(16, 26), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList, Font = AppTheme.FontBody };
        _cmbExpType.Items.AddRange(["إيجار", "كهرباء", "مياه", "إنترنت", "مرتبات", "نقل", "أخرى"]);
        _cmbExpType.SelectedIndex = 0;
        pnlForm.Controls.Add(new Label { Text = "نوع المصروف:", Location = new Point(16, 8), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel });
        pnlForm.Controls.Add(_cmbExpType);

        _txtExpAmt = new TextBox { Location = new Point(240, 26), Width = 140, Height = AppTheme.InputHeight, Font = AppTheme.FontBody, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "0.00" };
        pnlForm.Controls.Add(new Label { Text = "المبلغ:", Location = new Point(240, 8), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel });
        pnlForm.Controls.Add(_txtExpAmt);

        _txtExpDesc = new TextBox { Location = new Point(400, 26), Width = 300, Height = AppTheme.InputHeight, Font = AppTheme.FontBody, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "وصف..." };
        pnlForm.Controls.Add(new Label { Text = "الوصف:", Location = new Point(400, 8), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel });
        pnlForm.Controls.Add(_txtExpDesc);

        var btnAddExp = new Button { Text = "➕ إضافة مصروف", Location = new Point(16, 58), Width = 140, Height = 32, FlatStyle = FlatStyle.Flat, BackColor = AppTheme.AccentOrange, ForeColor = Color.White, Font = AppTheme.FontSmallBold, Cursor = Cursors.Hand };
        btnAddExp.FlatAppearance.BorderSize = 0;
        btnAddExp.Click += (_, _) => AddExpense();
        pnlForm.Controls.Add(btnAddExp);

        _dgvExpenses = new DataGridView { Dock = DockStyle.Fill };
        AppTheme.StyleDataGrid(_dgvExpenses);
        _dgvExpenses.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "ExpId",   Visible = false },
            new DataGridViewTextBoxColumn { Name = "Date",    HeaderText = "التاريخ",      FillWeight = 18 },
            new DataGridViewTextBoxColumn { Name = "Type",    HeaderText = "النوع",         FillWeight = 20 },
            new DataGridViewTextBoxColumn { Name = "Amount",  HeaderText = "المبلغ",        FillWeight = 15 },
            new DataGridViewTextBoxColumn { Name = "Desc",    HeaderText = "الوصف",         FillWeight = 30 },
            new DataGridViewTextBoxColumn { Name = "ByUser",  HeaderText = "بواسطة",        FillWeight = 15 }
        );

        var btnDelete = new Button { Text = "🗑 حذف", Dock = DockStyle.Bottom, Height = 38, FlatStyle = FlatStyle.Flat, BackColor = AppTheme.AccentRed, ForeColor = Color.White, Font = AppTheme.FontSmall, Cursor = Cursors.Hand };
        btnDelete.FlatAppearance.BorderSize = 0;
        btnDelete.Visible = CanDelete(Modules.Finance);
        btnDelete.Click += (_, _) => DeleteExpense();

        tab.Controls.AddRange([btnDelete, _dgvExpenses, pnlForm]);
        return tab;
    }

    // ══════════════════════════════════════════════════════════════
    // TAB 4 — DAILY CLOSE
    // ══════════════════════════════════════════════════════════════
    private TabPage BuildDailyCloseTab()
    {
        var tab = new TabPage("الإقفال اليومي") { BackColor = AppTheme.BgContent };

        var pnlCard = new Panel { Width = 460, BackColor = AppTheme.BgCard, Padding = new Padding(24), Anchor = AnchorStyles.None };
        tab.Resize += (_, _) =>
        {
            pnlCard.Location = new Point((tab.Width - pnlCard.Width) / 2, (tab.Height - pnlCard.Height) / 2);
        };

        _lblCloseDate    = new Label { Dock = DockStyle.Top, Height = 34, Font = AppTheme.FontH1, ForeColor = AppTheme.TextPrimary, TextAlign = ContentAlignment.MiddleCenter };
        var sep          = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = AppTheme.Border };

        Label MakeSumRow(string lbl, out Label val, Color? c = null)
        {
            var row  = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = AppTheme.BgCard };
            var lblL = new Label { Text = lbl, Dock = DockStyle.Right, AutoSize = false, Width = 180, Font = AppTheme.FontBody, ForeColor = AppTheme.TextLabel, TextAlign = ContentAlignment.MiddleRight };
            val = new Label { Dock = DockStyle.Fill, Font = AppTheme.FontBodyBold, ForeColor = c ?? AppTheme.TextPrimary, TextAlign = ContentAlignment.MiddleLeft };
            row.Controls.AddRange([val, lblL]);
            return row;
        }

        pnlCard.Controls.Add(MakeSumRow("رصيد البداية:",      out _lblOpenBalance));
        pnlCard.Controls.Add(MakeSumRow("إجمالي المبيعات:",   out _lblTotalSales,    AppTheme.AccentGreen));
        pnlCard.Controls.Add(MakeSumRow("إجمالي المصروفات:",  out _lblTotalExpenses, AppTheme.AccentRed));
        pnlCard.Controls.Add(MakeSumRow("رصيد نهاية اليوم:",  out _lblCloseBalance,  AppTheme.AccentBlue));
        pnlCard.Controls.Add(sep);
        pnlCard.Controls.Add(_lblCloseDate);

        _lblCloseBalance.Font = AppTheme.FontPOSLarge;
        pnlCard.Height = 260;

        var btnClose = new Button { Text = "🔒 إقفال يومي", Dock = DockStyle.Bottom, Height = 48, FlatStyle = FlatStyle.Flat, BackColor = AppTheme.AccentBlue, ForeColor = Color.White, Font = new Font("Segoe UI", 13f, FontStyle.Bold), Cursor = Cursors.Hand };
        btnClose.FlatAppearance.BorderSize = 0;
        btnClose.Click += (_, _) => DailyClose();
        pnlCard.Controls.Add(btnClose);

        tab.Controls.Add(pnlCard);
        return tab;
    }

    // ══════════════════════════════════════════════════════════════
    // DATA LOADING
    // ══════════════════════════════════════════════════════════════
    private void LoadCashboxes()
    {
        RunAsync(async () =>
        {
            var result = await _mediator.Send(new GetCashboxesQuery(UserSession.Current.BranchId));
            InvokeOnUI(() =>
            {
                _dgvCashboxes.Rows.Clear();
                if (!result.IsSuccess || result.Value == null) return;

                // Also refresh combos
                _cmbCashbox.Items.Clear();
                _cmbTargetBox.Items.Clear();

                foreach (var box in result.Value)
                {
                    _dgvCashboxes.Rows.Add(box.CashboxId, box.Name, box.IsMain ? "رئيسية" : "فرعية", $"{box.CurrentBalance:N2}", $"{box.OpeningBalance:N2}", box.BranchName);
                    _cmbCashbox.Items.Add(new ComboItem(box.CashboxId, box.Name));
                    _cmbTargetBox.Items.Add(new ComboItem(box.CashboxId, box.Name));
                }
                if (_cmbCashbox.Items.Count > 0) _cmbCashbox.SelectedIndex = 0;
                if (_cmbTargetBox.Items.Count > 1) _cmbTargetBox.SelectedIndex = 1;
            });
        });
    }

    private void LoadOperations()
    {
        RunAsync(async () =>
        {
            var result = await _mediator.Send(new GetCashboxOperationsQuery(UserSession.Current.BranchId, DateTime.Today));
            InvokeOnUI(() =>
            {
                _dgvOps.Rows.Clear();
                if (!result.IsSuccess || result.Value == null) return;
                foreach (var op in result.Value)
                {
                    var rowIdx = _dgvOps.Rows.Add(
                        op.OperationDate.ToString("dd/MM/yyyy HH:mm"),
                        op.TypeAr, op.CashboxName, $"{op.Amount:N2}", $"{op.BalanceAfter:N2}", op.Notes, op.ByUserName);
                    _dgvOps.Rows[rowIdx].Cells["Amount"].Style.ForeColor = op.Type == "Withdraw" ? AppTheme.AccentRed : AppTheme.AccentGreen;
                }
            });
        });
    }

    private void LoadExpenses()
    {
        RunAsync(async () =>
        {
            var result = await _mediator.Send(new GetExpensesQuery(UserSession.Current.BranchId, DateTime.Today));
            InvokeOnUI(() =>
            {
                _dgvExpenses.Rows.Clear();
                if (!result.IsSuccess || result.Value == null) return;
                foreach (var e in result.Value)
                    _dgvExpenses.Rows.Add(e.ExpenseId, e.Date.ToString("dd/MM/yyyy HH:mm"), e.TypeAr, $"{e.Amount:N2}", e.Description, e.CreatedByName);
            });
        });
    }

    private void LoadCloseData()
    {
        RunAsync(async () =>
        {
            var result = await _mediator.Send(new GetDailyCloseSummaryQuery(UserSession.Current.BranchId, DateTime.Today));
            InvokeOnUI(() =>
            {
                if (!result.IsSuccess || result.Value == null) return;
                var s = result.Value;
                _lblCloseDate.Text     = $"إقفال يوم: {DateTime.Today:dddd، d MMMM yyyy}";
                _lblOpenBalance.Text   = $"{s.OpeningBalance:N2} ج.م";
                _lblTotalSales.Text    = $"+{s.TotalSales:N2} ج.م";
                _lblTotalExpenses.Text = $"-{s.TotalExpenses:N2} ج.م";
                _lblCloseBalance.Text  = $"{s.ClosingBalance:N2} ج.م";
            });
        });
    }

    // ══════════════════════════════════════════════════════════════
    // ACTIONS
    // ══════════════════════════════════════════════════════════════
    private void AddCashbox()
    {
        using var dlg = new CashboxEditDialog(_mediator);
        if (dlg.ShowDialog() == DialogResult.OK) LoadCashboxes();
    }

    private void EditCashbox()
    {
        if (_dgvCashboxes.CurrentRow == null) return;
        var id = (int)_dgvCashboxes.CurrentRow.Cells["CashboxId"].Value;
        using var dlg = new CashboxEditDialog(_mediator, id);
        if (dlg.ShowDialog() == DialogResult.OK) LoadCashboxes();
    }

    private void SaveOperation()
    {
        var cashbox = _cmbCashbox.SelectedItem as ComboItem;
        if (cashbox == null) { ShowError("اختر الخزنة"); return; }
        if (!decimal.TryParse(_txtAmount.Text, out var amount) || amount <= 0)
        { ShowError("أدخل مبلغاً صحيحاً"); return; }

        var opType = _cmbOpType.SelectedIndex switch { 0 => "Deposit", 1 => "Withdraw", _ => "Transfer" };
        int? targetId = null;
        if (opType == "Transfer")
        {
            var target = _cmbTargetBox.SelectedItem as ComboItem;
            if (target == null || target.Id == cashbox.Id) { ShowError("اختر خزنة مختلفة للتحويل"); return; }
            targetId = target.Id;
        }

        RunAsync(async () =>
        {
            var cmd    = new CreateCashboxOperationCommand(cashbox.Id, opType, amount, targetId, _txtOpNotes.Text.Trim(), UserSession.Current.UserId);
            var result = await _mediator.Send(cmd);
            InvokeOnUI(() =>
            {
                if (result.IsSuccess)
                {
                    _txtAmount.Clear(); _txtOpNotes.Clear();
                    LoadOperations(); LoadCashboxes();
                }
                else ShowError(result.Error);
            });
        }, "جاري حفظ العملية...");
    }

    private void AddExpense()
    {
        if (!decimal.TryParse(_txtExpAmt.Text, out var amount) || amount <= 0)
        { ShowError("أدخل مبلغاً صحيحاً"); return; }

        RunAsync(async () =>
        {
            var cmd    = new CreateExpenseCommand(UserSession.Current.BranchId, _cmbExpType.Text, amount, _txtExpDesc.Text.Trim(), UserSession.Current.UserId);
            var result = await _mediator.Send(cmd);
            InvokeOnUI(() =>
            {
                if (result.IsSuccess) { _txtExpAmt.Clear(); _txtExpDesc.Clear(); LoadExpenses(); }
                else ShowError(result.Error);
            });
        });
    }

    private void DeleteExpense()
    {
        if (_dgvExpenses.CurrentRow == null) return;
        var id = (int)_dgvExpenses.CurrentRow.Cells["ExpId"].Value;
        if (!Confirm("هل تريد حذف هذا المصروف؟")) return;
        RunAsync(async () =>
        {
            var result = await _mediator.Send(new DeleteExpenseCommand(id, UserSession.Current.UserId));
            InvokeOnUI(() => { if (result.IsSuccess) LoadExpenses(); else ShowError(result.Error); });
        });
    }

    private void DailyClose()
    {
        if (!Confirm("هل تريد إقفال اليوم؟ لن يمكن إجراء معاملات على هذا اليوم بعدها.")) return;
        RunAsync(async () =>
        {
            var cmd    = new DailyCloseCommand(UserSession.Current.BranchId, DateTime.Today, UserSession.Current.UserId);
            var result = await _mediator.Send(cmd);
            InvokeOnUI(() =>
            {
                if (result.IsSuccess) ShowSuccess("تم الإقفال اليومي بنجاح");
                else ShowError(result.Error);
            });
        }, "جاري الإقفال...");
    }
}

// ── Cashbox edit dialog ────────────────────────────────────────────
public sealed class CashboxEditDialog : Form
{
    private readonly IMediator _mediator;
    private readonly int?      _cashboxId;
    private TextBox  _txtName      = null!;
    private CheckBox _chkMain      = null!;
    private TextBox  _txtOpenBal   = null!;

    public CashboxEditDialog(IMediator mediator, int? cashboxId = null)
    {
        _mediator  = mediator;
        _cashboxId = cashboxId;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = _cashboxId.HasValue ? "تعديل خزنة" : "خزنة جديدة";
        Size = new Size(380, 260); FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false; StartPosition = FormStartPosition.CenterParent;
        BackColor = AppTheme.BgCard; RightToLeft = RightToLeft.Yes; RightToLeftLayout = true;

        var pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16) };
        int y   = 12;
        void Row(string lbl, out TextBox tb, string ph = "")
        {
            pnl.Controls.Add(new Label { Text = lbl, Location = new Point(16, y), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel });
            y += 20;
            tb = new TextBox { Location = new Point(16, y), Width = 320, Height = AppTheme.InputHeight, Font = AppTheme.FontBody, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = ph };
            pnl.Controls.Add(tb); y += 44;
        }
        Row("اسم الخزنة:", out _txtName);
        Row("رصيد البداية:", out _txtOpenBal, "0.00");
        _chkMain = new CheckBox { Text = "خزنة رئيسية", Location = new Point(16, y), AutoSize = true, Font = AppTheme.FontBody };
        pnl.Controls.Add(_chkMain);

        var pnlBtns = new Panel { Dock = DockStyle.Bottom, Height = 52, BackColor = AppTheme.BgCard, Padding = new Padding(12, 8, 12, 8) };
        var btnSave = new Button { Text = "💾 حفظ", Dock = DockStyle.Right, Width = 110, Height = 34, FlatStyle = FlatStyle.Flat, BackColor = AppTheme.AccentGreen, ForeColor = Color.White, Font = AppTheme.FontBodyBold, Cursor = Cursors.Hand };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(_txtName.Text)) { MessageBox.Show("اسم الخزنة مطلوب", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            decimal.TryParse(_txtOpenBal.Text, out var bal);
            Task.Run(async () =>
            {
                object cmd = _cashboxId.HasValue
                    ? new UpdateCashboxCommand(_cashboxId.Value, _txtName.Text.Trim(), _chkMain.Checked, bal, UserSession.Current.UserId)
                    : new CreateCashboxCommand(UserSession.Current.BranchId, _txtName.Text.Trim(), _chkMain.Checked, bal, UserSession.Current.UserId);
                var result = cmd is UpdateCashboxCommand uc ? await _mediator.Send(uc) : await _mediator.Send((CreateCashboxCommand)cmd);
                Invoke(() => { if (result.IsSuccess) { DialogResult = DialogResult.OK; Close(); } else MessageBox.Show(result.Error, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error); });
            });
        };
        var btnCancel = new Button { Text = "إلغاء", Dock = DockStyle.Left, Width = 90, Height = 34, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = AppTheme.TextSecondary, Font = AppTheme.FontBody, Cursor = Cursors.Hand };
        btnCancel.FlatAppearance.BorderColor = AppTheme.Border;
        btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        pnlBtns.Controls.AddRange([btnSave, btnCancel]);
        Controls.AddRange([pnlBtns, pnl]);
    }
}
