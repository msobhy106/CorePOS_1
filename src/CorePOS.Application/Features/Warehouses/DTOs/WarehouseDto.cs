namespace CorePOS.Application.Features.Warehouses.DTOs;

public class WarehouseDto
{
    public int     Id          { get; set; }
    public string  Code        { get; set; } = string.Empty;
    public string  NameAr      { get; set; } = string.Empty;
    public int     BranchId    { get; set; }
    public string  BranchName  { get; set; } = string.Empty;
    public string? Address     { get; set; }
    public string? ManagerName { get; set; }
    public bool    IsMain      { get; set; }
    public bool    IsActive    { get; set; }
}
