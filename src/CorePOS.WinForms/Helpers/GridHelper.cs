using System.Windows.Forms;

namespace CorePOS.WinForms.Helpers;

public static class GridHelper
{
    public static int? GetSelectedId(DataGridView dgv, int idColumnIndex = 0)
    {
        if (dgv.SelectedRows.Count == 0) return null;
        var val = dgv.SelectedRows[0].Cells[idColumnIndex].Value;
        return val is int id ? id : null;
    }

    public static T? GetSelectedValue<T>(DataGridView dgv, string columnName)
    {
        if (dgv.SelectedRows.Count == 0) return default;
        var val = dgv.SelectedRows[0].Cells[columnName].Value;
        return val is T t ? t : default;
    }

    public static void HideColumn(DataGridView dgv, int index)
    {
        if (dgv.Columns.Count > index)
            dgv.Columns[index].Visible = false;
    }

    public static void HideColumn(DataGridView dgv, string name)
    {
        if (dgv.Columns.Contains(name))
            dgv.Columns[name].Visible = false;
    }

    public static void SetColumnWidth(DataGridView dgv, string name, int width)
    {
        if (dgv.Columns.Contains(name))
        {
            dgv.Columns[name].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dgv.Columns[name].Width        = width;
        }
    }

    public static void HighlightLowStock(DataGridView dgv, string stockColumn, string minStockColumn)
    {
        foreach (DataGridViewRow row in dgv.Rows)
        {
            if (!row.IsNewRow)
            {
                decimal stock    = row.Cells[stockColumn].Value is decimal s ? s : 0;
                decimal minStock = row.Cells[minStockColumn].Value is decimal m ? m : 0;
                if (minStock > 0 && stock <= minStock)
                    row.DefaultCellStyle.ForeColor = ThemeManager.AccentRed;
            }
        }
    }
}
