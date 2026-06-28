namespace CorePOS.Application.Interfaces;

public interface ICurrentUserService
{
    int     UserId      { get; }
    string  Username    { get; }
    string  FullName    { get; }
    int     RoleId      { get; }
    int?    BranchId    { get; }
    int?    WarehouseId { get; }
    bool    IsAdmin     { get; }
    bool    IsLoggedIn  { get; }
    int?    ShiftId     { get; }

    bool HasPermission(string moduleKey, string actionKey);
    void SetUser(int userId, string username, string fullName, int roleId,
                 int? branchId, int? warehouseId, IEnumerable<string> permissionKeys);
    void SetShift(int? shiftId);
    void Clear();
}
