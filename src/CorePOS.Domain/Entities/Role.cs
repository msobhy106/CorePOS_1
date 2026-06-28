using CorePOS.Domain.Common;

namespace CorePOS.Domain.Entities;

public class Role : BaseEntity
{
    public string  Name        { get; private set; } = string.Empty;
    public string  NameAr      { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool    IsSystem    { get; private set; }
    public bool    IsActive    { get; private set; } = true;
    public DateTime CreatedAt  { get; private set; } = DateTime.UtcNow;

    private readonly List<RolePermission> _rolePermissions = [];
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    protected Role() { }

    public static Role Create(string name, string nameAr, string? description = null, bool isSystem = false)
    {
        if (string.IsNullOrWhiteSpace(name))   throw new ArgumentException("Role name is required.");
        if (string.IsNullOrWhiteSpace(nameAr)) throw new ArgumentException("Arabic role name is required.");
        return new Role { Name = name.Trim(), NameAr = nameAr.Trim(), Description = description, IsSystem = isSystem };
    }

    public void Update(string name, string nameAr, string? description)
    {
        if (IsSystem) throw new InvalidOperationException("System roles cannot be modified.");
        Name        = name.Trim();
        NameAr      = nameAr.Trim();
        Description = description;
    }

    public void Deactivate() => IsActive = false;
    public void Activate()   => IsActive = true;
}
