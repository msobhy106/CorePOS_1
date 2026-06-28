namespace CorePOS.WinForms.Helpers;

public static class CurrencyFormatter
{
    private static string _symbol  = "ج.م";
    private static int    _decimals = 2;

    public static void Configure(string symbol, int decimals)
    {
        _symbol   = symbol;
        _decimals = decimals;
    }

    public static string Format(decimal value)
        => $"{value.ToString($"N{_decimals}")} {_symbol}";

    public static string FormatNoSymbol(decimal value)
        => value.ToString($"N{_decimals}");

    public static string FormatQty(decimal value)
        => value % 1 == 0 ? value.ToString("N0") : value.ToString("N3");
}
