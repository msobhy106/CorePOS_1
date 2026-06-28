using Microsoft.EntityFrameworkCore;
using CorePOS.Domain.Entities;
using CorePOS.Domain.Interfaces;
using CorePOS.Persistence.DbContexts;

namespace CorePOS.Persistence.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(CorePOSDbContext db) : base(db) { }

    public async Task<User?> GetByUsernameAsync(
        string username, CancellationToken ct = default)
        => await _set
            .Include(u => u.Role)
            .Include(u => u.Branch)
            .Include(u => u.Warehouse)
            .FirstOrDefaultAsync(u => u.Username == username.ToLowerInvariant() && u.IsActive, ct);

    public async Task<User?> GetByIdWithRoleAsync(
        int id, CancellationToken ct = default)
        => await _set
            .Include(u => u.Role)
            .Include(u => u.Branch)
            .Include(u => u.Warehouse)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<IReadOnlyList<User>> GetActiveAsync(CancellationToken ct = default)
        => await _set.Include(u => u.Role)
            .Where(u => u.IsActive).OrderBy(u => u.FullName).ToListAsync(ct);

    public async Task<IReadOnlyList<User>> GetByRoleAsync(
        int roleId, CancellationToken ct = default)
        => await _set.Where(u => u.RoleId == roleId && u.IsActive).ToListAsync(ct);

    public async Task<bool> UsernameExistsAsync(
        string username, int? excludeId = null, CancellationToken ct = default)
        => await _set.AnyAsync(u =>
            u.Username == username.ToLowerInvariant() &&
            (excludeId == null || u.Id != excludeId), ct);

    // ── Roles ─────────────────────────────────────────────
    public async Task<Role?> GetRoleByIdAsync(int roleId, CancellationToken ct = default)
        => await _db.Roles.FindAsync(new object[] { roleId }, ct);

    public async Task<IReadOnlyList<Role>> GetAllRolesAsync(CancellationToken ct = default)
        => await _db.Roles.Where(r => r.IsActive).OrderBy(r => r.Id).ToListAsync(ct);

    public async Task<IReadOnlyList<Permission>> GetPermissionsForRoleAsync(
        int roleId, CancellationToken ct = default)
        => await _db.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.Permission!)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<string>> GetPermissionKeysForUserAsync(
        int userId, CancellationToken ct = default)
    {
        var user = await _set.FindAsync(new object[] { userId }, ct);
        if (user is null) return [];

        return await _db.RolePermissions
            .Where(rp => rp.RoleId == user.RoleId)
            .Select(rp => rp.Permission!.ModuleKey + "." + rp.Permission.ActionKey)
            .ToListAsync(ct);
    }

    public async Task SetRolePermissionsAsync(
        int roleId, IEnumerable<int> permissionIds, CancellationToken ct = default)
    {
        // Remove existing
        var existing = await _db.RolePermissions
            .Where(rp => rp.RoleId == roleId).ToListAsync(ct);
        _db.RolePermissions.RemoveRange(existing);

        // Add new
        var newPerms = permissionIds.Select(pid => RolePermission.Create(roleId, pid));
        await _db.RolePermissions.AddRangeAsync(newPerms, ct);
    }

    public async Task<IReadOnlyList<Permission>> GetAllPermissionsAsync(
        CancellationToken ct = default)
        => await _db.Permissions.OrderBy(p => p.ModuleKey).ThenBy(p => p.ActionKey).ToListAsync(ct);

    // ── Audit ─────────────────────────────────────────────
    public async Task LogAuditAsync(
        int userId, string action, string entityName,
        string? entityId, string? oldValues, string? newValues,
        string? machineName = null, CancellationToken ct = default)
    {
        var log = AuditLog.Create(userId, action, entityName,
            entityId, oldValues, newValues, machineName);
        await _db.AuditLogs.AddAsync(log, ct);
        await _db.SaveChangesAsync(ct);
    }
}
