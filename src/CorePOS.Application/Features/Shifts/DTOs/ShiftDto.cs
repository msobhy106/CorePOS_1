namespace CorePOS.Application.Features.Shifts.DTOs;

public class ShiftDto
{
    public int      Id             { get; set; }
    public string   ShiftNo        { get; set; } = string.Empty;
    public string   CashierName    { get; set; } = string.Empty;
    public string   BranchName     { get; set; } = string.Empty;
    public string   CashBoxName    { get; set; } = string.Empty;
    public decimal  OpeningBalance { get; set; }
    public decimal  ClosingBalance { get; set; }
    public decimal  ActualBalance  { get; set; }
    public decimal  Difference     => ActualBalance - ClosingBalance;
    public DateTime StartTime      { get; set; }
    public DateTime? EndTime       { get; set; }
    public string   Status         { get; set; } = string.Empty;
    public int      SalesCount     { get; set; }
    public decimal  SalesRevenue   { get; set; }
    public decimal  TotalExpenses  { get; set; }
    public string?  Notes          { get; set; }
}
