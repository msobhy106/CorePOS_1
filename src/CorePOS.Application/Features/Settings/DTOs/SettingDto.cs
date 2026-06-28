namespace CorePOS.Application.Features.Settings.DTOs;

public class SettingDto
{
    public string  Key         { get; set; } = string.Empty;
    public string? Value       { get; set; }
    public string  Group       { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string  DataType    { get; set; } = "string";
    public bool    IsSystem    { get; set; }
}
