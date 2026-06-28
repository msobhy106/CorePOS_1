using CorePOS.Domain.Entities;

namespace CorePOS.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<User?> GetByIdWithRoleAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<User>> GetActiveAsync(CancellationToken ct = default);
    Task<IReadOnlyList<User>> GetByRoleAsync(int roleId, CancellationToken ct = default);
    Task<bool> UsernameExistsAsync(string username, int? excludeId = null, CancellationToken ct = default);

    // Roles
    Task<Role?> GetRoleByIdAsync(int roleId, CancellationToken ct = default);
    Task<IReadOnlyList<Role>> GetAllRolesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Permission>> GetPermissionsForRoleAsync(int roleId, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetPermissionKeysForUserAsync(int userId, CancellationToken ct = default);
    Task SetRolePermissionsAsync(int roleId, IEnumerable<int> permissionIds, CancellationToken ct = default);
    Task<IReadOnlyList<Permission>> GetAllPermissionsAsync(CancellationToken ct = default);

    // Audit
    Task LogAuditAsync(int userId, string action, string entityName,
        string? entityId, string? oldValues, string? newValues,
        string? machineName = null, CancellationToken ct = default);
}
