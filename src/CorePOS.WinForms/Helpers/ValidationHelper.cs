using System.Windows.Forms;

namespace CorePOS.WinForms.Helpers;

public static class ValidationHelper
{
    public static bool IsNotEmpty(TextBox txt, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(txt.Text))
        {
            MessageHelper.ShowWarning($"{fieldName} مطلوب");
            txt.Focus();
            return false;
        }
        return true;
    }

    public static bool IsValidDecimal(TextBox txt, string fieldName, out decimal value)
    {
        if (!decimal.TryParse(txt.Text, out value) || value < 0)
        {
            MessageHelper.ShowWarning($"{fieldName} يجب أن يكون رقماً صحيحاً");
            txt.Focus();
            return false;
        }
        return true;
    }

    public static bool IsValidInt(TextBox txt, string fieldName, out int value)
    {
        if (!int.TryParse(txt.Text, out value) || value < 0)
        {
            MessageHelper.ShowWarning($"{fieldName} يجب أن يكون رقماً صحيحاً");
            txt.Focus();
            return false;
        }
        return true;
    }

    public static bool ComboHasSelection(ComboBox cbo, string fieldName)
    {
        if (cbo.SelectedIndex < 0 || cbo.SelectedValue is null)
        {
            MessageHelper.ShowWarning($"يرجى اختيار {fieldName}");
            cbo.Focus();
            return false;
        }
        return true;
    }
}
