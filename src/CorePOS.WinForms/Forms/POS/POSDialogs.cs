using MediatR;
using CorePOS.WinForms.Theme;
using CorePOS.WinForms.Infrastructure;
using CorePOS.Application.Features.Shifts.Commands;
using CorePOS.Application.Features.Customers.Queries;

namespace CorePOS.WinForms.Forms.POS;

// ════════════════════════════════════════════════════════════════════
// SHIFT OPEN DIALOG
// ════════════════════════════════════════════════════════════════════
public sealed class ShiftOpenDialog : Form
{
    private readonly IMediator _mediator;
    private TextBox _txtOpeningBalance = null!;

    public ShiftOpenDialog(IMediator mediator)
    {
        _mediator = mediator;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text            = "فتح وردية جديدة";
        Size            = new Size(400, 280);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        MinimizeBox     = false;
        StartPosition   = FormStartPosition.CenterParent;
        BackColor       = AppTheme.BgCard;
        RightToLeft     = RightToLeft.Yes;
        RightToLeftLayout = true;

        var lblTitle = new Label
        {
            Text      = "فتح وردية",
            Font      = AppTheme.FontH1,
            ForeColor = AppTheme.TextPrimary,
            AutoSize  = true,
            Location  = new Point(20, 20)
        };

        var lblDate = new Label
        {
            Text      = $"التاريخ: {DateTime.Now:dd/MM/yyyy  HH:mm}",
            Font      = AppTheme.FontBody,
            ForeColor = AppTheme.TextSecondary,
            AutoSize  = true,
            Location  = new Point(20, 56)
        };

        var lblUser = new Label
        {
            Text      = $"الكاشير: {(UserSession.IsLoggedIn ? UserSession.Current.DisplayName : "-")}",
            Font      = AppTheme.FontBody,
            ForeColor = AppTheme.TextSecondary,
            AutoSize  = true,
            Location  = new Point(20, 78)
        };

        var lblBalance = new Label
        {
            Text      = "رصيد البداية (ج.م):",
            Font      = AppTheme.FontBodyBold,
            ForeColor = AppTheme.TextLabel,
            AutoSize  = true,
            Location  = new Point(20, 114)
        };

        _txtOpeningBalance = new TextBox
        {
            Location    = new Point(20, 138),
            Width       = 340,
            Height      = AppTheme.InputHeight,
            Font        = new Font("Segoe UI", 13f),
            BorderStyle = BorderStyle.FixedSingle,
            Text        = "0.00"
        };

        var btnOpen = new Button
        {
            Text      = "فتح الوردية",
            Location  = new Point(20, 190),
            Width     = 160,
            Height    = AppTheme.ButtonHeight + 4,
            FlatStyle = FlatStyle.Flat,
            BackColor = AppTheme.AccentGreen,
            ForeColor = Color.White,
            Font      = AppTheme.FontBodyBold,
            Cursor    = Cursors.Hand
        };
        btnOpen.FlatAppearance.BorderSize = 0;
        btnOpen.Click += (_, _) => DoOpen();

        var btnCancel = new Button
        {
            Text      = "إلغاء",
            Location  = new Point(200, 190),
            Width     = 100,
            Height    = AppTheme.ButtonHeight + 4,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = AppTheme.TextSecondary,
            Font      = AppTheme.FontBody,
            Cursor    = Cursors.Hand
        };
        btnCancel.FlatAppearance.BorderColor = AppTheme.Border;
        btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

        Controls.AddRange([lblTitle, lblDate, lblUser, lblBalance, _txtOpeningBalance, btnOpen, btnCancel]);
    }

    private void DoOpen()
    {
        if (!decimal.TryParse(_txtOpeningBalance.Text, out var balance) || balance < 0)
        {
            MessageBox.Show("يرجى إدخال رصيد صحيح", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Task.Run(async () =>
        {
            var cmd    = new OpenShiftCommand(UserSession.Current.UserId,
                                              UserSession.Current.BranchId,
                                              balance);
            var result = await _mediator.Send(cmd);
            Invoke(() =>
            {
                if (result.IsSuccess)
                {
                    UserSession.Current.ActiveShiftId = result.Value.ShiftId;
                    UserSession.Current.ActiveShiftNo = result.Value.ShiftNo;
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

// ════════════════════════════════════════════════════════════════════
// CUSTOMER PICKER DIALOG
// ════════════════════════════════════════════════════════════════════
public sealed class CustomerPickerDialog : Form
{
    private readonly IMediator _mediator;
    private DataGridView _dgv = null!;
    private TextBox _txtSearch = null!;
    private System.Windows.Forms.Timer _debounce = null!;

    public int?   SelectedCustomerId   { get; private set; }
    public string SelectedCustomerName { get; private set; } = string.Empty;

    public CustomerPickerDialog(IMediator mediator)
    {
        _mediator = mediator;
        InitializeComponent();
        LoadCustomers(string.Empty);
    }

    private void InitializeComponent()
    {
        Text            = "اختر عميل";
        Size            = new Size(600, 480);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        StartPosition   = FormStartPosition.CenterParent;
        BackColor       = AppTheme.BgContent;
        RightToLeft     = RightToLeft.Yes;
        RightToLeftLayout = true;

        _txtSearch = new TextBox
        {
            Dock            = DockStyle.Top,
            Height          = AppTheme.InputHeight,
            Font            = AppTheme.FontBody,
            BorderStyle     = BorderStyle.FixedSingle,
            PlaceholderText = "ابحث باسم العميل أو رقم الهاتف..."
        };
        _debounce = new System.Windows.Forms.Timer { Interval = 300 };
        _debounce.Tick += (_, _) => { _debounce.Stop(); LoadCustomers(_txtSearch.Text.Trim()); };
        _txtSearch.TextChanged += (_, _) => { _debounce.Stop(); _debounce.Start(); };

        _dgv = new DataGridView { Dock = DockStyle.Fill };
        AppTheme.StyleDataGrid(_dgv);
        _dgv.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "Id",      HeaderText = "ID",    Width = 50,  Visible = false },
            new DataGridViewTextBoxColumn { Name = "Name",    HeaderText = "الاسم", FillWeight = 40 },
            new DataGridViewTextBoxColumn { Name = "Phone",   HeaderText = "الهاتف",FillWeight = 30 },
            new DataGridViewTextBoxColumn { Name = "Balance", HeaderText = "الرصيد",FillWeight = 30 }
        );
        _dgv.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) SelectRow(e.RowIndex); };

        var pnlBottom = new Panel
        {
            Dock      = DockStyle.Bottom,
            Height    = 52,
            BackColor = AppTheme.BgCard,
            Padding   = new Padding(12, 8, 12, 8)
        };
        var btnSelect = new Button
        {
            Text      = "تحديد",
            Dock      = DockStyle.Right,
            Width     = 100,
            Height    = 34,
            FlatStyle = FlatStyle.Flat,
            BackColor = AppTheme.AccentBlue,
            ForeColor = Color.White,
            Font      = AppTheme.FontBodyBold,
            Cursor    = Cursors.Hand
        };
        btnSelect.FlatAppearance.BorderSize = 0;
        btnSelect.Click += (_, _) =>
        {
            if (_dgv.CurrentRow != null) SelectRow(_dgv.CurrentRow.Index);
        };
        var btnCancel = new Button
        {
            Text      = "إلغاء",
            Dock      = DockStyle.Left,
            Width     = 80,
            Height    = 34,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.White,
            ForeColor = AppTheme.TextSecondary,
            Font      = AppTheme.FontBody,
            Cursor    = Cursors.Hand
        };
        btnCancel.FlatAppearance.BorderColor = AppTheme.Border;
        btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        pnlBottom.Controls.AddRange([btnSelect, btnCancel]);

        Controls.AddRange([pnlBottom, _dgv, _txtSearch]);
    }

    private void LoadCustomers(string search)
    {
        Task.Run(async () =>
        {
            var q      = new SearchCustomersQuery(search, 50);
            var result = await _mediator.Send(q);
            Invoke(() =>
            {
                _dgv.Rows.Clear();
                if (result.IsSuccess && result.Value != null)
                    foreach (var c in result.Value)
                        _dgv.Rows.Add(c.CustomerId, c.Name, c.Phone, $"{c.Balance:N2}");
            });
        });
    }

    private void SelectRow(int rowIndex)
    {
        SelectedCustomerId   = (int)_dgv.Rows[rowIndex].Cells["Id"].Value;
        SelectedCustomerName = _dgv.Rows[rowIndex].Cells["Name"].Value?.ToString() ?? string.Empty;
        DialogResult         = DialogResult.OK;
        Close();
    }
}

// ════════════════════════════════════════════════════════════════════
// DELIVERY DIALOG
// ════════════════════════════════════════════════════════════════════
public sealed class DeliveryDialog : Form
{
    private TextBox _txtCost  = null!;
    private TextBox _txtAgent = null!;

    public decimal DeliveryCost  { get; private set; }
    public string  DeliveryAgent { get; private set; } = string.Empty;

    public DeliveryDialog(decimal currentCost, string currentAgent)
    {
        InitializeComponent();
        _txtCost.Text  = currentCost > 0 ? currentCost.ToString("N2") : string.Empty;
        _txtAgent.Text = currentAgent;
    }

    private void InitializeComponent()
    {
        Text            = "خدمة التوصيل";
        Size            = new Size(360, 240);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        StartPosition   = FormStartPosition.CenterParent;
        BackColor       = AppTheme.BgCard;
        RightToLeft     = RightToLeft.Yes;
        RightToLeftLayout = true;

        int y = 20;
        void AddRow(string lbl, out TextBox tb, string placeholder = "")
        {
            Controls.Add(new Label { Text = lbl, Location = new Point(20, y), AutoSize = true, Font = AppTheme.FontBodyBold, ForeColor = AppTheme.TextLabel });
            y += 24;
            tb = new TextBox { Location = new Point(20, y), Width = 300, Height = AppTheme.InputHeight, Font = AppTheme.FontBody, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = placeholder };
            Controls.Add(tb);
            y += 44;
        }

        AddRow("تكلفة التوصيل (ج.م):", out _txtCost,  "0.00");
        AddRow("مندوب التوصيل:",        out _txtAgent, "اسم المندوب");

        var btnOk = new Button
        {
            Text = "تأكيد", Location = new Point(20, y), Width = 140, Height = AppTheme.ButtonHeight,
            FlatStyle = FlatStyle.Flat, BackColor = AppTheme.AccentBlue, ForeColor = Color.White,
            Font = AppTheme.FontBodyBold, Cursor = Cursors.Hand
        };
        btnOk.FlatAppearance.BorderSize = 0;
        btnOk.Click += (_, _) =>
        {
            decimal.TryParse(_txtCost.Text, out var cost);
            DeliveryCost  = cost;
            DeliveryAgent = _txtAgent.Text.Trim();
            DialogResult  = DialogResult.OK;
            Close();
        };

        var btnClear = new Button
        {
            Text = "إزالة التوصيل", Location = new Point(180, y), Width = 140, Height = AppTheme.ButtonHeight,
            FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = AppTheme.AccentRed,
            Font = AppTheme.FontBody, Cursor = Cursors.Hand
        };
        btnClear.FlatAppearance.BorderColor = AppTheme.AccentRed;
        btnClear.Click += (_, _) =>
        {
            DeliveryCost  = 0;
            DeliveryAgent = string.Empty;
            DialogResult  = DialogResult.OK;
            Close();
        };

        Controls.AddRange([btnOk, btnClear]);
        Height = y + 80;
    }
}

// ════════════════════════════════════════════════════════════════════
// HELD INVOICE PICKER DIALOG
// ════════════════════════════════════════════════════════════════════
public sealed class HeldInvoicePickerDialog : Form
{
    private DataGridView _dgv = null!;
    public int SelectedIndex { get; private set; } = -1;

    public HeldInvoicePickerDialog(List<HeldInvoice> heldInvoices)
    {
        InitializeComponent();

        foreach (var (h, i) in heldInvoices.Select((h, i) => (h, i)))
        {
            _dgv.Rows.Add(
                i,
                h.HeldAt.ToString("HH:mm:ss"),
                h.CustomerName.Length > 0 ? h.CustomerName : "بدون عميل",
                h.Items.Count,
                $"{h.Items.Sum(x => x.LineTotal):N2}"
            );
        }
    }

    private void InitializeComponent()
    {
        Text            = "الفواتير المعلقة";
        Size            = new Size(540, 380);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        StartPosition   = FormStartPosition.CenterParent;
        BackColor       = AppTheme.BgContent;
        RightToLeft     = RightToLeft.Yes;
        RightToLeftLayout = true;

        _dgv = new DataGridView { Dock = DockStyle.Fill };
        AppTheme.StyleDataGrid(_dgv);
        _dgv.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "Idx",      HeaderText = "#",       Width = 40, Visible = false },
            new DataGridViewTextBoxColumn { Name = "Time",     HeaderText = "وقت التعليق", FillWeight = 25 },
            new DataGridViewTextBoxColumn { Name = "Customer", HeaderText = "العميل",      FillWeight = 35 },
            new DataGridViewTextBoxColumn { Name = "Items",    HeaderText = "عدد الأصناف", FillWeight = 20 },
            new DataGridViewTextBoxColumn { Name = "Total",    HeaderText = "الإجمالي",    FillWeight = 20 }
        );
        _dgv.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) SelectRow(e.RowIndex); };

        var pnlBtns = new Panel
        {
            Dock = DockStyle.Bottom, Height = 52, BackColor = AppTheme.BgCard, Padding = new Padding(12, 8, 12, 8)
        };
        var btnSelect = new Button
        {
            Text = "استرجاع الفاتورة", Dock = DockStyle.Right, Width = 150, Height = 34,
            FlatStyle = FlatStyle.Flat, BackColor = AppTheme.AccentPurple, ForeColor = Color.White,
            Font = AppTheme.FontBodyBold, Cursor = Cursors.Hand
        };
        btnSelect.FlatAppearance.BorderSize = 0;
        btnSelect.Click += (_, _) => { if (_dgv.CurrentRow != null) SelectRow(_dgv.CurrentRow.Index); };

        var btnCancel = new Button
        {
            Text = "إلغاء", Dock = DockStyle.Left, Width = 80, Height = 34,
            FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = AppTheme.TextSecondary,
            Font = AppTheme.FontBody, Cursor = Cursors.Hand
        };
        btnCancel.FlatAppearance.BorderColor = AppTheme.Border;
        btnCancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        pnlBtns.Controls.AddRange([btnSelect, btnCancel]);

        Controls.AddRange([pnlBtns, _dgv]);
    }

    private void SelectRow(int rowIndex)
    {
        SelectedIndex = (int)_dgv.Rows[rowIndex].Cells["Idx"].Value;
        DialogResult  = DialogResult.OK;
        Close();
    }
}
