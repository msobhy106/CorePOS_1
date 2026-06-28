using CorePOS.Domain.Common;

namespace CorePOS.Domain.Entities;

public class Setting : BaseEntity
{
    public string  SettingKey   { get; private set; } = string.Empty;
    public string? SettingValue { get; private set; }
    public string? SettingGroup { get; private set; }
    public string? DataType     { get; private set; }
    public string? Description  { get; private set; }

    protected Setting() { }

    public static Setting Create(string key, string? value, string? group = null,
        string? dataType = "string", string? description = null)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Setting key is required.");
        return new Setting { SettingKey = key.Trim(), SettingValue = value,
                             SettingGroup = group, DataType = dataType, Description = description };
    }

    public void UpdateValue(string? value) => SettingValue = value;

    // Typed getters
    public string  AsString(string defaultValue = "")    => SettingValue ?? defaultValue;
    public int     AsInt(int defaultValue = 0)            => int.TryParse(SettingValue, out var v) ? v : defaultValue;
    public bool    AsBool(bool defaultValue = false)      => bool.TryParse(SettingValue, out var v) ? v : defaultValue;
    public decimal AsDecimal(decimal defaultValue = 0)    => decimal.TryParse(SettingValue, out var v) ? v : defaultValue;
}
