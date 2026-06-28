namespace CorePOS.Application.Features.Expenses.DTOs;
public class ExpenseDto
{
    public int     Id           { get; set; }
    public string  ExpenseNo    { get; set; } = string.Empty;
    public DateOnly ExpenseDate { get; set; }
    public string  CategoryName { get; set; } = string.Empty;
    public string  BranchName   { get; set; } = string.Empty;
    public decimal Amount       { get; set; }
    public string? Description  { get; set; }
    public string  CreatedBy    { get; set; } = string.Empty;
}
