namespace CorePOS.Application.Features.Employees.DTOs;

public class EmployeeDto
{
    public int      Id         { get; set; }
    public string   Code       { get; set; } = string.Empty;
    public string   Name       { get; set; } = string.Empty;
    public string?  JobTitle   { get; set; }
    public string?  Phone      { get; set; }
    public string?  Address    { get; set; }
    public decimal  Salary     { get; set; }
    public DateOnly? HireDate  { get; set; }
    public string?  BranchName { get; set; }
    public bool     IsActive   { get; set; }
    public decimal  TotalAdvances   { get; set; }
    public decimal  TotalDeductions { get; set; }
    public decimal  TotalBonuses    { get; set; }
}
