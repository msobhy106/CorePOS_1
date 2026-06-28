using CorePOS.Application.Interfaces;

namespace CorePOS.Infrastructure.Security;

public class CurrentUserService : ICurrentUserService
{
    private HashSet<string> _permissionKeys = [];

    public int     UserId      { get; private set; }
    public string  Username    { get; private set; } = string.Empty;
    public string  FullName    { get; private set; } = string.Empty;
    public int     RoleId      { get; private set; }
    public int?    BranchId    { get; private set; }
    public int?    WarehouseId { get; private set; }
    public bool    IsAdmin     { get; private set; }
    public bool    IsLoggedIn  => UserId > 0;
    public int?    ShiftId     { get; private set; }

    public bool HasPermission(string moduleKey, string actionKey)
        => IsAdmin || _permissionKeys.Contains($"{moduleKey}.{actionKey}");

    public void SetUser(int userId, string username, string fullName, int roleId,
        int? branchId, int? warehouseId, IEnumerable<string> permissionKeys)
    {
        UserId      = userId;
        Username    = username;
        FullName    = fullName;
        RoleId      = roleId;
        BranchId    = branchId;
        WarehouseId = warehouseId;
        IsAdmin     = roleId == 1;   // Role 1 = Admin (matches seed data)
        _permissionKeys = new HashSet<string>(permissionKeys, StringComparer.OrdinalIgnoreCase);
    }

    public void SetShift(int? shiftId) => ShiftId = shiftId;

    public void Clear()
    {
        UserId = 0; Username = string.Empty; FullName = string.Empty;
        RoleId = 0; BranchId = null; WarehouseId = null;
        IsAdmin = false; ShiftId = null;
        _permissionKeys.Clear();
    }
}
