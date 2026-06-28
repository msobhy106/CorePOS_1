namespace CorePOS.Application.Features.CashBoxes.DTOs;

public class CashBoxDto
{
    public int     Id             { get; set; }
    public string  Code           { get; set; } = string.Empty;
    public string  NameAr         { get; set; } = string.Empty;
    public string  BranchName     { get; set; } = string.Empty;
    public bool    IsMain         { get; set; }
    public decimal CurrentBalance { get; set; }
    public bool    IsActive       { get; set; }
    public bool    HasOpenShift   { get; set; }
    public string? CurrentCashier { get; set; }
}
