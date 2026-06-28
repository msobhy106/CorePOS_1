namespace CorePOS.Application.Features.Users.DTOs;

public class UserDto
{
    public int     Id           { get; set; }
    public string  Username     { get; set; } = string.Empty;
    public string  FullName     { get; set; } = string.Empty;
    public string? FullNameAr   { get; set; }
    public string? Email        { get; set; }
    public string? Phone        { get; set; }
    public string? PhotoPath    { get; set; }
    public int     RoleId       { get; set; }
    public string  RoleName     { get; set; } = string.Empty;
    public string  RoleNameAr   { get; set; } = string.Empty;
    public int?    BranchId     { get; set; }
    public string? BranchName   { get; set; }
    public int?    WarehouseId  { get; set; }
    public string? WarehouseName{ get; set; }
    public bool    IsActive     { get; set; }
    public DateTime? LastLogin  { get; set; }
}

public class LoginResultDto
{
    public bool   Success       { get; set; }
    public string? ErrorMessage { get; set; }
    public UserDto? User        { get; set; }
    public IReadOnlyList<string> Permissions { get; set; } = [];
}
