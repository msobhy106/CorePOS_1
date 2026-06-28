using CorePOS.Domain.Common;

namespace CorePOS.Domain.Entities;

public class RolePermission : BaseEntity
{
    public int RoleId       { get; private set; }
    public int PermissionId { get; private set; }

    public Role?       Role       { get; private set; }
    public Permission? Permission { get; private set; }

    protected RolePermission() { }

    public static RolePermission Create(int roleId, int permissionId)
        => new() { RoleId = roleId, PermissionId = permissionId };
}
