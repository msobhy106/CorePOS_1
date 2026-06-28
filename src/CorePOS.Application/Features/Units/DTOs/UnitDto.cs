namespace CorePOS.Application.Features.Units.DTOs;

public class UnitDto
{
    public int     Id           { get; set; }
    public string  Code         { get; set; } = string.Empty;
    public string  NameAr       { get; set; } = string.Empty;
    public string  NameEn       { get; set; } = string.Empty;
    public string? Abbreviation { get; set; }
    public bool    IsActive     { get; set; }
}
