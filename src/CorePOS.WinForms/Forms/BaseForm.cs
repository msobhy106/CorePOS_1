using MediatR;
using CorePOS.WinForms.Theme;
using CorePOS.WinForms.Infrastructure;

namespace CorePOS.WinForms.Forms;

/// <summary>
/// Base class for all Core POS forms.
/// Handles: RTL, theming, permission checking, loading overlay, error display.
/// </summary>
public class BaseForm : Form
{
    protected readonly IMediator _mediator;

    public BaseForm(IMediator mediator)
    {
        _mediator = mediator;
        InitializeBaseForm();
    }

    // Parameterless constructor for designer
    protected BaseForm()
    {
        _mediator = null!;
        InitializeBaseForm();
    }

    private void InitializeBaseForm()
    {
        // RTL Arabic
        RightToLeft       = RightToLeft.Yes;
        RightToLeftLayout = true;

        // Base appearance
        BackColor = AppTheme.BgContent;
        Font      = AppTheme.FontBody;

        // Default size
        StartPosition = FormStartPosition.CenterScreen;
    }

    // ── Permission Helpers ────────────────────────────────────────
    protected bool CanAccess(string module, string action)
        => UserSession.IsLoggedIn && UserSession.Current.HasPermission(module, action);

    protected bool CanAdd(string module)    => CanAccess(module, "Add");
    protected bool CanEdit(string module)   => CanAccess(module, "Edit");
    protected bool CanDelete(string module) => CanAccess(module, "Delete");
    protected bool CanView(string module)   => CanAccess(module, "View");
    protected bool CanPrint(string module)  => CanAccess(module, "Print");
    protected bool CanExport(string module) => CanAccess(module, "Export");

    protected void ApplyButtonPermissions(string module,
        Button? btnAdd = null, Button? btnEdit = null,
        Button? btnDelete = null, Button? btnPrint = null,
        Button? btnExport = null)
    {
        if (btnAdd    != null) btnAdd.Visible    = CanAdd(module);
        if (btnEdit   != null) btnEdit.Visible   = CanEdit(module);
        if (btnDelete != null) btnDelete.Visible = CanDelete(module);
        if (btnPrint  != null) btnPrint.Visible  = CanPrint(module);
        if (btnExport != null) btnExport.Visible = CanExport(module);
    }

    // ── Loading Overlay ───────────────────────────────────────────
    private Panel? _loadingPanel;

    protected void ShowLoading(string message = "جاري التحميل...")
    {
        if (_loadingPanel != null) return;

        _loadingPanel = new Panel
        {
            BackColor = Color.FromArgb(180, 0, 0, 0),
            Dock      = DockStyle.Fill
        };
        var lbl = new Label
        {
            Text      = message,
            ForeColor = Color.White,
            Font      = AppTheme.FontH2,
            AutoSize  = true
        };
        _loadingPanel.Controls.Add(lbl);
        _loadingPanel.Resize += (_, _) =>
        {
            lbl.Left = (_loadingPanel.Width  - lbl.Width)  / 2;
            lbl.Top  = (_loadingPanel.Height - lbl.Height) / 2;
        };

        Controls.Add(_loadingPanel);
        _loadingPanel.BringToFront();
        Refresh();
    }

    protected void HideLoading()
    {
        if (_loadingPanel == null) return;
        Controls.Remove(_loadingPanel);
        _loadingPanel.Dispose();
        _loadingPanel = null;
    }

    // ── Async wrapper for event handlers ─────────────────────────
    protected void RunAsync(Func<Task> action, string loadingMsg = "جاري التنفيذ...")
    {
        ShowLoading(loadingMsg);
        Task.Run(async () =>
        {
            try   { await action(); }
            catch (Exception ex) { InvokeOnUI(() => ShowError(ex.Message)); }
            finally { InvokeOnUI(HideLoading); }
        });
    }

    protected void InvokeOnUI(Action action)
    {
        if (InvokeRequired) Invoke(action);
        else action();
    }

    // ── Message helpers ───────────────────────────────────────────
    protected void ShowError(string message, string title = "خطأ")
        => MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);

    protected void ShowSuccess(string message, string title = "نجاح")
        => MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);

    protected bool Confirm(string message, string title = "تأكيد")
        => MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question)
           == DialogResult.Yes;

    // ── Style helpers ─────────────────────────────────────────────
    protected Panel CreateCard(int x, int y, int w, int h, string? title = null)
    {
        var card = new Panel
        {
            Location  = new Point(x, y),
            Size      = new Size(w, h),
            BackColor = AppTheme.BgCard,
            Padding   = new Padding(AppTheme.CardPadding)
        };

        if (title != null)
        {
            var lbl = new Label
            {
                Text     = title,
                Font     = AppTheme.FontH2,
                ForeColor= AppTheme.TextPrimary,
                AutoSize = true,
                Location = new Point(AppTheme.CardPadding, AppTheme.CardPadding)
            };
            card.Controls.Add(lbl);
        }

        return card;
    }

    protected Label CreateLabel(string text, int x, int y, bool bold = false)
        => new()
        {
            Text      = text,
            Location  = new Point(x, y),
            AutoSize  = true,
            Font      = bold ? AppTheme.FontBodyBold : AppTheme.FontBody,
            ForeColor = AppTheme.TextLabel
        };
}
