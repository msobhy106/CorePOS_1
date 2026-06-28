namespace CorePOS.WinForms.Infrastructure;

/// <summary>
/// Singleton holding the currently logged-in user and their permissions.
/// Set after successful login. Used by all forms for RBAC.
/// </summary>
public sealed class UserSession
{
    // ── Singleton ─────────────────────────────────────────────────
    private static UserSession? _current;
    public  static UserSession  Current => _current ?? throw new InvalidOperationException("No active session.");
    public  static bool         IsLoggedIn => _current != null;

    // ── User Info ─────────────────────────────────────────────────
    public int    UserId       { get; private set; }
    public string Username     { get; private set; } = string.Empty;
    public string FullName     { get; private set; } = string.Empty;
    public string FullNameAr   { get; private set; } = string.Empty;
    public string RoleName     { get; private set; } = string.Empty;
    public string RoleNameAr   { get; private set; } = string.Empty;
    public int    BranchId     { get; private set; }
    public string BranchName   { get; private set; } = string.Empty;
    public int    WarehouseId  { get; private set; }
    public string? PhotoPath   { get; private set; }
    public DateTime LoginTime  { get; private set; }

    // ── Shift Info ────────────────────────────────────────────────
    public int?   ActiveShiftId   { get; set; }
    public string ActiveShiftNo   { get; set; } = string.Empty;
    public bool   HasOpenShift    => ActiveShiftId.HasValue;

    // ── Permissions ───────────────────────────────────────────────
    // Key: "Module:Action" e.g. "Sales:Add", "Reports:Export"
    private readonly HashSet<string> _permissions = new(StringComparer.OrdinalIgnoreCase);

    private UserSession() { }

    // ── Factory ───────────────────────────────────────────────────
    public static void CreateSession(
        int    userId,
        string username,
        string fullName,
        string fullNameAr,
        string roleName,
        string roleNameAr,
        int    branchId,
        string branchName,
        int    warehouseId,
        string? photoPath,
        IEnumerable<(string Module, string Action)> permissions)
    {
        var session = new UserSession
        {
            UserId      = userId,
            Username    = username,
            FullName    = fullName,
            FullNameAr  = fullNameAr,
            RoleName    = roleName,
            RoleNameAr  = roleNameAr,
            BranchId    = branchId,
            BranchName  = branchName,
            WarehouseId = warehouseId,
            PhotoPath   = photoPath,
            LoginTime   = DateTime.Now
        };

        foreach (var (module, action) in permissions)
            session._permissions.Add($"{module}:{action}");

        _current = session;
    }

    public static void ClearSession()
    {
        _current = null;
    }

    // ── Permission Checks ─────────────────────────────────────────
    public bool HasPermission(string module, string action)
        => _permissions.Contains($"{module}:{action}");

    public bool CanAdd(string module)    => HasPermission(module, "Add");
    public bool CanEdit(string module)   => HasPermission(module, "Edit");
    public bool CanDelete(string module) => HasPermission(module, "Delete");
    public bool CanView(string module)   => HasPermission(module, "View");
    public bool CanPrint(string module)  => HasPermission(module, "Print");
    public bool CanExport(string module) => HasPermission(module, "Export");

    // ── Display Info ──────────────────────────────────────────────
    public string DisplayName => string.IsNullOrEmpty(FullNameAr) ? FullName : FullNameAr;
}

/// <summary>Permission module keys — matches DB Permissions.ModuleKey</summary>
public static class Modules
{
    public const string Sales        = "Sales";
    public const string Purchases    = "Purchases";
    public const string Inventory    = "Inventory";
    public const string Reports      = "Reports";
    public const string Settings     = "Settings";
    public const string Customers    = "Customers";
    public const string Suppliers    = "Suppliers";
    public const string Products     = "Products";
    public const string Categories   = "Categories";
    public const string Finance      = "Finance";
    public const string Employees    = "Employees";
    public const string Users        = "Users";
    public const string Backup       = "Backup";
}
