namespace CorePOS.Application.Features.Branches.DTOs;
public class BranchDto
{
    public int     Id          { get; set; }
    public string  Code        { get; set; } = string.Empty;
    public string  NameAr      { get; set; } = string.Empty;
    public string? Address     { get; set; }
    public string? Phone       { get; set; }
    public string? ManagerName { get; set; }
    public bool    IsMain      { get; set; }
    public bool    IsActive    { get; set; }
}
