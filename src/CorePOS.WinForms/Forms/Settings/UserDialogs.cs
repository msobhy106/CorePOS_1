using MediatR;
using CorePOS.WinForms.Theme;
using CorePOS.WinForms.Infrastructure;
using CorePOS.Application.Features.Users.Queries;
using CorePOS.Application.Features.Users.Commands;

namespace CorePOS.WinForms.Forms.Settings;

// ════════════════════════════════════════════════════════════════════
// USER EDIT DIALOG
// ════════════════════════════════════════════════════════════════════
public sealed class UserEditDialog : Form
{
    private readonly IMediator _mediator;
    private readonly int?      _userId;

    private TextBox  _txtUsername   = null!;
    private TextBox  _txtFullName   = null!;
    private TextBox  _txtFullNameAr = null!;
    private TextBox  _txtPassword   = null!;
    private TextBox  _txtConfirm    = null!;
    private ComboBox _cmbRole       = null!;
    private ComboBox _cmbBranch     = null!;
    private CheckBox _chkActive     = null!;

    public UserEditDialog(IMediator mediator, int? userId)
    {
        _mediator = mediator;
        _userId   = userId;
        InitializeComponent();
        LoadCombos();
        if (userId.HasValue) LoadUser(userId.Value);
    }

    private void InitializeComponent()
    {
        Text            = _userId.HasValue ? "تعديل مستخدم" : "مستخدم جديد";
        Size            = new Size(500, 480);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        StartPosition   = FormStartPosition.CenterParent;
        BackColor       = AppTheme.BgCard;
        RightToLeft     = RightToLeft.Yes;
        RightToLeftLayout = true;

        var pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
        int y   = 12;

        void Row(string lbl, out TextBox tb, bool password = false, string ph = "")
        {
            pnl.Controls.Add(new Label { Text = lbl, Location = new Point(16, y), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel });
            y += 22;
            tb = new TextBox
            {
                Location        = new Point(16, y),
                Width           = 420,
                Height          = AppTheme.InputHeight,
                Font            = AppTheme.FontBody,
                BorderStyle     = BorderStyle.FixedSingle,
                PlaceholderText = ph
            };
            if (password) tb.PasswordChar = '●';
            pnl.Controls.Add(tb);
            y += 46;
        }

        void RowCombo(string lbl, out ComboBox cb)
        {
            pnl.Controls.Add(new Label { Text = lbl, Location = new Point(16, y), AutoSize = true, Font = AppTheme.FontSmall, ForeColor = AppTheme.TextLabel });
            y += 22;
            cb = new ComboBox
            {
                Location      = new Point(16, y),
                Width         = 420,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font          = AppTheme.FontBody
            };
            pnl.Controls.Add(cb);
            y += 46;
        }

        Row("اسم المستخدم (للدخول):",  out _txtUsername,   ph: "admin");
        Row("الاسم الكامل (عربي):",     out _txtFullNameAr, ph: "محمد أحمد");
        Row("الاسم الكامل (إنجليزي):", out _txtFullName,   ph: "Mohamed Ahmed");
        Row("كلمة المرور:",              out _txtPassword,   password: true, ph: _userId.HasValue ? "اتركه فارغاً للإبقاء على الحالي" : "مطلوب");
        Row("تأكيد كلمة المرور:",        out _txtConfirm,    password: true);
        RowCombo("الدور:",               out _cmbRole);
        RowCombo("الفرع:",               out _cmbBranch);

        _chkActive = new CheckBox
        {
            Text     = "حساب نشط",
            Location = new Point(16, y),
            AutoSize = true,
            Font     = AppTheme.FontBody,
            Checked  = true
        };
        pnl.Controls.Add(_chkActive);

        var pnlBtns = new Panel
        {
            Dock      = DockStyle.Bottom,
            Height    = 52,
            BackColor = AppTheme.BgCard,
            Padding   = new Padding(12, 8, 12, 8)
        };

        var btnSave = new Button
        {
            Text      = "💾 حفظ",
            Dock      = DockStyle.Right,
            Width     = 110,
            Height    = 34,
            FlatStyle = FlatStyle.Flat,
            BackColor = AppTheme.AccentGreen,
            ForeColor = Color.White,
            Font      = AppTheme.FontBodyBold,
            Cursor    = Cursors.Hand
        };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += (_, _) => Save();

        var btnCancel = new Button
        {
            Text      = "إلغاء",
            Dock      = DockStyle.Left,
            Width     = 90,
            Height    = 34,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = AppTheme.TextSecondary,
            Font      = AppTheme.FontBody,
            Cursor    = Cursors.Hand
        };
        btnCancel.FlatAppearance.BorderColor = AppTheme.Border;
        btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

        pnlBtns.Controls.AddRange([btnSave, btnCancel]);
        Controls.AddRange([pnlBtns, pnl]);
    }

    private void LoadCombos()
    {
        Task.Run(async () =>
        {
            var rolesTask   = _mediator.Send(new GetRolesQuery());
            var branchTask  = _mediator.Send(new GetBranchesQuery());
            await Task.WhenAll(rolesTask, branchTask);

            Invoke(() =>
            {
                if (rolesTask.Result.IsSuccess && rolesTask.Result.Value != null)
                    foreach (var r in rolesTask.Result.Value)
                        _cmbRole.Items.Add(new ComboItem(r.RoleId, r.NameAr));
                if (_cmbRole.Items.Count > 0) _cmbRole.SelectedIndex = 0;

                if (branchTask.Result.IsSuccess && branchTask.Result.Value != null)
                    foreach (var b in branchTask.Result.Value)
                        _cmbBranch.Items.Add(new ComboItem(b.BranchId, b.Name));
                if (_cmbBranch.Items.Count > 0) _cmbBranch.SelectedIndex = 0;
            });
        });
    }

    private void LoadUser(int id)
    {
        Task.Run(async () =>
        {
            var result = await _mediator.Send(new GetUserByIdQuery(id));
            Invoke(() =>
            {
                if (!result.IsSuccess || result.Value == null) return;
                var u = result.Value;
                _txtUsername.Text   = u.Username;
                _txtFullNameAr.Text = u.FullNameAr;
                _txtFullName.Text   = u.FullName;
                _chkActive.Checked  = u.IsActive;

                SelectComboById(_cmbRole,   u.RoleId);
                SelectComboById(_cmbBranch, u.BranchId);
            });
        });
    }

    private static void SelectComboById(ComboBox cmb, int id)
    {
        foreach (var item in cmb.Items)
            if (item is ComboItem ci && ci.Id == id) { cmb.SelectedItem = item; return; }
    }

    private void Save()
    {
        // Validation
        if (string.IsNullOrWhiteSpace(_txtUsername.Text))
        { MessageBox.Show("اسم المستخدم مطلوب", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

        if (!_userId.HasValue && string.IsNullOrWhiteSpace(_txtPassword.Text))
        { MessageBox.Show("كلمة المرور مطلوبة للمستخدم الجديد", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

        if (!string.IsNullOrEmpty(_txtPassword.Text) && _txtPassword.Text != _txtConfirm.Text)
        { MessageBox.Show("كلمة المرور وتأكيدها غير متطابقتين", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

        var role   = _cmbRole.SelectedItem   as ComboItem;
        var branch = _cmbBranch.SelectedItem as ComboItem;

        Task.Run(async () =>
        {
            object cmd = _userId.HasValue
                ? new UpdateUserCommand(
                    UserId:      _userId.Value,
                    Username:    _txtUsername.Text.Trim(),
                    FullName:    _txtFullName.Text.Trim(),
                    FullNameAr:  _txtFullNameAr.Text.Trim(),
                    NewPassword: string.IsNullOrWhiteSpace(_txtPassword.Text) ? null : _txtPassword.Text,
                    RoleId:      role?.Id ?? 0,
                    BranchId:    branch?.Id ?? 0,
                    IsActive:    _chkActive.Checked,
                    ModifiedBy:  UserSession.Current.UserId)
                : new CreateUserCommand(
                    Username:    _txtUsername.Text.Trim(),
                    FullName:    _txtFullName.Text.Trim(),
                    FullNameAr:  _txtFullNameAr.Text.Trim(),
                    Password:    _txtPassword.Text,
                    RoleId:      role?.Id ?? 0,
                    BranchId:    branch?.Id ?? 0,
                    CreatedBy:   UserSession.Current.UserId);

            var result = cmd is UpdateUserCommand uc
                ? await _mediator.Send(uc)
                : await _mediator.Send((CreateUserCommand)cmd);

            Invoke(() =>
            {
                if (result.IsSuccess) { DialogResult = DialogResult.OK; Close(); }
                else MessageBox.Show(result.Error, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        });
    }
}

// ════════════════════════════════════════════════════════════════════
// USER PERMISSIONS DIALOG
// ════════════════════════════════════════════════════════════════════
public sealed class UserPermissionsDialog : Form
{
    private readonly IMediator _mediator;
    private readonly int       _userId;
    private readonly string    _userName;

    // Permission checkboxes: key = "Module:Action"
    private readonly Dictionary<string, CheckBox> _permChecks = new();

    public UserPermissionsDialog(IMediator mediator, int userId, string userName)
    {
        _mediator = mediator;
        _userId   = userId;
        _userName = userName;
        InitializeComponent();
        LoadPermissions();
    }

    private void InitializeComponent()
    {
        Text            = $"صلاحيات: {_userName}";
        Size            = new Size(700, 580);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        StartPosition   = FormStartPosition.CenterParent;
        BackColor       = AppTheme.BgContent;
        RightToLeft     = RightToLeft.Yes;
        RightToLeftLayout = true;

        var pnlTitle = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = AppTheme.BgCard, Padding = new Padding(16, 0, 0, 0) };
        pnlTitle.Controls.Add(new Label { Text = $"تحديد الصلاحيات للمستخدم: {_userName}", Font = AppTheme.FontH2, ForeColor = AppTheme.TextPrimary, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight });

        // Scrollable permissions grid
        var scrollPnl = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = AppTheme.BgContent, Padding = new Padding(16) };

        // Define modules and actions
        var modules = new[]
        {
            (Modules.Sales,       "المبيعات",     new[] { "Add", "Edit", "Delete", "View", "Print" }),
            (Modules.Purchases,   "المشتريات",    new[] { "Add", "Edit", "Delete", "View" }),
            (Modules.Inventory,   "المخزون",      new[] { "View", "Edit" }),
            (Modules.Reports,     "التقارير",     new[] { "View", "Print", "Export" }),
            (Modules.Customers,   "العملاء",      new[] { "Add", "Edit", "Delete", "View" }),
            (Modules.Suppliers,   "الموردين",     new[] { "Add", "Edit", "Delete", "View" }),
            (Modules.Products,    "الأصناف",      new[] { "Add", "Edit", "Delete", "View" }),
            (Modules.Finance,     "الخزنة",       new[] { "Add", "Edit", "Delete", "View" }),
            (Modules.Employees,   "الموظفين",     new[] { "Add", "Edit", "Delete", "View" }),
            (Modules.Users,       "المستخدمين",   new[] { "Add", "Edit", "Delete", "View" }),
            (Modules.Settings,    "الإعدادات",    new[] { "View", "Edit" }),
            (Modules.Backup,      "النسخ الاحتياطي", new[] { "Add", "View" }),
        };

        var actionLabels = new Dictionary<string, string>
        {
            ["Add"]    = "إضافة",
            ["Edit"]   = "تعديل",
            ["Delete"] = "حذف",
            ["View"]   = "عرض",
            ["Print"]  = "طباعة",
            ["Export"] = "تصدير"
        };

        // Build header row
        var headerPnl = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = AppTheme.BgCard };
        headerPnl.Controls.Add(new Label { Text = "الوحدة", Location = new Point(300, 8), AutoSize = true, Font = AppTheme.FontSmallBold, ForeColor = AppTheme.TextSecondary });
        int[] actionX = [16, 80, 154, 228, 302, 376];
        string[] actionOrder = ["Add", "Edit", "Delete", "View", "Print", "Export"];
        for (int i = 0; i < actionOrder.Length; i++)
            headerPnl.Controls.Add(new Label { Text = actionLabels[actionOrder[i]], Location = new Point(actionX[i], 8), AutoSize = true, Font = AppTheme.FontSmallBold, ForeColor = AppTheme.TextSecondary });
        scrollPnl.Controls.Add(headerPnl);

        int y = 44;
        foreach (var (module, moduleLabelAr, actions) in modules)
        {
            var row = new Panel { Location = new Point(0, y), Height = 38, Width = 640, BackColor = y % 76 == 44 ? AppTheme.BgCard : AppTheme.BgContent };
            row.Controls.Add(new Label { Text = moduleLabelAr, Location = new Point(300, 9), AutoSize = true, Font = AppTheme.FontBody, ForeColor = AppTheme.TextPrimary });

            for (int i = 0; i < actionOrder.Length; i++)
            {
                var action = actionOrder[i];
                var chk = new CheckBox
                {
                    Location = new Point(actionX[i] + 6, 9),
                    AutoSize = true,
                    Enabled  = actions.Contains(action),
                    Tag      = $"{module}:{action}"
                };
                if (actions.Contains(action))
                    _permChecks[$"{module}:{action}"] = chk;
                else
                    chk.ForeColor = AppTheme.Border;

                row.Controls.Add(chk);
            }

            // "Select all" for this module
            var btnAll = new Button
            {
                Text      = "الكل",
                Location  = new Point(460, 6),
                Width     = 50,
                Height    = 24,
                FlatStyle = FlatStyle.Flat,
                BackColor = AppTheme.BgContent,
                ForeColor = AppTheme.AccentBlue,
                Font      = AppTheme.FontSmall,
                Cursor    = Cursors.Hand,
                Tag       = module
            };
            btnAll.FlatAppearance.BorderColor = AppTheme.AccentBlue;
            btnAll.Click += (s, _) =>
            {
                if (s is Button b && b.Tag is string mod)
                    foreach (var kv in _permChecks.Where(k => k.Key.StartsWith(mod + ":")))
                        kv.Value.Checked = true;
            };
            row.Controls.Add(btnAll);

            scrollPnl.Controls.Add(row);
            y += 38;
        }

        // Select/Deselect all buttons
        var pnlSelectAll = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = AppTheme.BgCard, Padding = new Padding(8) };
        var btnSelectAll = new Button { Text = "تحديد الكل", Dock = DockStyle.Right, Width = 120, Height = 28, FlatStyle = FlatStyle.Flat, BackColor = AppTheme.AccentBlue, ForeColor = Color.White, Font = AppTheme.FontSmall, Cursor = Cursors.Hand };
        btnSelectAll.FlatAppearance.BorderSize = 0;
        btnSelectAll.Click += (_, _) => { foreach (var chk in _permChecks.Values) chk.Checked = true; };
        var btnClearAll = new Button { Text = "إلغاء الكل", Dock = DockStyle.Right, Width = 120, Height = 28, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = AppTheme.AccentRed, Font = AppTheme.FontSmall, Cursor = Cursors.Hand, Margin = new Padding(0, 0, 6, 0) };
        btnClearAll.FlatAppearance.BorderColor = AppTheme.AccentRed;
        btnClearAll.Click += (_, _) => { foreach (var chk in _permChecks.Values) chk.Checked = false; };
        pnlSelectAll.Controls.AddRange([btnSelectAll, btnClearAll]);

        // Bottom save/cancel
        var pnlBtns = new Panel { Dock = DockStyle.Bottom, Height = 52, BackColor = AppTheme.BgCard, Padding = new Padding(12, 8, 12, 8) };
        var btnSave = new Button { Text = "💾 حفظ الصلاحيات", Dock = DockStyle.Right, Width = 160, Height = 34, FlatStyle = FlatStyle.Flat, BackColor = AppTheme.AccentGreen, ForeColor = Color.White, Font = AppTheme.FontBodyBold, Cursor = Cursors.Hand };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += (_, _) => SavePermissions();
        var btnClose = new Button { Text = "إغلاق", Dock = DockStyle.Left, Width = 90, Height = 34, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = AppTheme.TextSecondary, Font = AppTheme.FontBody, Cursor = Cursors.Hand };
        btnClose.FlatAppearance.BorderColor = AppTheme.Border;
        btnClose.Click += (_, _) => Close();
        pnlBtns.Controls.AddRange([btnSave, btnClose]);

        Controls.AddRange([pnlBtns, scrollPnl, pnlSelectAll, pnlTitle]);
    }

    private void LoadPermissions()
    {
        Task.Run(async () =>
        {
            var result = await _mediator.Send(new GetUserPermissionsQuery(_userId));
            Invoke(() =>
            {
                if (!result.IsSuccess || result.Value == null) return;
                var granted = new HashSet<string>(result.Value.Select(p => $"{p.Module}:{p.Action}"), StringComparer.OrdinalIgnoreCase);
                foreach (var kv in _permChecks)
                    kv.Value.Checked = granted.Contains(kv.Key);
            });
        });
    }

    private void SavePermissions()
    {
        var permissions = _permChecks
            .Where(kv => kv.Value.Checked)
            .Select(kv => kv.Key)   // "Module:Action"
            .ToList();

        Task.Run(async () =>
        {
            var cmd    = new SetUserPermissionsCommand(_userId, permissions, UserSession.Current.UserId);
            var result = await _mediator.Send(cmd);
            Invoke(() =>
            {
                if (result.IsSuccess) { MessageBox.Show("تم حفظ الصلاحيات", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information); Close(); }
                else MessageBox.Show(result.Error, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
        });
    }
}
