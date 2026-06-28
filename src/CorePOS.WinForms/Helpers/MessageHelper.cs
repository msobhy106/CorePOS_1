using System.Windows.Forms;

namespace CorePOS.WinForms.Helpers;

/// <summary>Standardized Arabic message boxes.</summary>
public static class MessageHelper
{
    public static void ShowError(string message, string title = "خطأ")
        => MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);

    public static void ShowSuccess(string message, string title = "Core POS")
        => MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);

    public static void ShowWarning(string message, string title = "تنبيه")
        => MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);

    public static bool Confirm(string message, string title = "تأكيد")
        => MessageBox.Show(message, title,
               MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

    public static bool ConfirmDelete(string itemName = "")
        => Confirm(string.IsNullOrEmpty(itemName)
               ? "هل تريد حذف هذا السجل؟"
               : $"هل تريد حذف: {itemName}؟", "تأكيد الحذف");

    public static void ShowResult(CorePOS.Application.Common.Result result, string successMsg = "تم الحفظ بنجاح ✓")
    {
        if (result.IsSuccess) ShowSuccess(successMsg);
        else ShowError(result.Error);
    }

    public static void ShowResult<T>(CorePOS.Application.Common.Result<T> result, string successMsg = "تم الحفظ بنجاح ✓")
    {
        if (result.IsSuccess) ShowSuccess(successMsg);
        else ShowError(result.Error);
    }
}
