using CorePOS.Domain.Common;
using CorePOS.Domain.Events;

namespace CorePOS.Domain.Entities;

public class User : AuditableEntity
{
    public string   Username     { get; private set; } = string.Empty;
    public string   PasswordHash { get; private set; } = string.Empty;
    public string   FullName     { get; private set; } = string.Empty;
    public string?  FullNameAr   { get; private set; }
    public string?  Email        { get; private set; }
    public string?  Phone        { get; private set; }
    public string?  PhotoPath    { get; private set; }
    public int      RoleId       { get; private set; }
    public int?     BranchId     { get; private set; }
    public int?     WarehouseId  { get; private set; }
    public bool     IsActive     { get; private set; } = true;
    public DateTime? LastLogin   { get; private set; }

    // Navigation
    public Role?      Role      { get; private set; }
    public Branch?    Branch    { get; private set; }
    public Warehouse? Warehouse { get; private set; }

    protected User() { }

    public static User Create(string username, string passwordHash, string fullName,
        int roleId, int? branchId = null, int? warehouseId = null,
        string? fullNameAr = null, string? email = null, string? phone = null)
    {
        if (string.IsNullOrWhiteSpace(username))     throw new ArgumentException("Username is required.");
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash is required.");
        if (string.IsNullOrWhiteSpace(fullName))     throw new ArgumentException("Full name is required.");

        return new User
        {
            Username     = username.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            FullName     = fullName.Trim(),
            FullNameAr   = fullNameAr?.Trim(),
            Email        = email?.Trim().ToLowerInvariant(),
            Phone        = phone?.Trim(),
            RoleId       = roleId,
            BranchId     = branchId,
            WarehouseId  = warehouseId
        };
    }

    public void UpdateProfile(string fullName, string? fullNameAr, string? email, string? phone, string? photoPath)
    {
        FullName   = fullName.Trim();
        FullNameAr = fullNameAr?.Trim();
        Email      = email?.Trim().ToLowerInvariant();
        Phone      = phone?.Trim();
        PhotoPath  = photoPath;
        UpdatedAt  = DateTime.UtcNow;
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password hash cannot be empty.");
        PasswordHash = newPasswordHash;
        UpdatedAt    = DateTime.UtcNow;
    }

    public void AssignRole(int roleId)
    {
        RoleId    = roleId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignBranch(int? branchId, int? warehouseId)
    {
        BranchId    = branchId;
        WarehouseId = warehouseId;
        UpdatedAt   = DateTime.UtcNow;
    }

    public void RecordLogin() => LastLogin = DateTime.UtcNow;

    public void Activate()   { IsActive = true;  UpdatedAt = DateTime.UtcNow; }
    public void Deactivate() { IsActive = false; UpdatedAt = DateTime.UtcNow; }
}
