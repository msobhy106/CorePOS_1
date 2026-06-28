namespace CorePOS.Domain.Enums;

/// <summary>System-defined role IDs (match Seed Data).</summary>
public static class SystemRoles
{
    public const int Admin          = 1;
    public const int Manager        = 2;
    public const int Cashier        = 3;
    public const int PurchaseClerk  = 4;
    public const int Accountant     = 5;
    public const int WarehouseClerk = 6;
}

/// <summary>Permission module keys (match Seed Data).</summary>
public static class PermissionModules
{
    public const string Dashboard  = "Dashboard";
    public const string Products   = "Products";
    public const string Categories = "Categories";
    public const string Units      = "Units";
    public const string Customers  = "Customers";
    public const string Suppliers  = "Suppliers";
    public const string Sales      = "Sales";
    public const string Purchases  = "Purchases";
    public const string Inventory  = "Inventory";
    public const string Treasury   = "Treasury";
    public const string Expenses   = "Expenses";
    public const string Employees  = "Employees";
    public const string Reports    = "Reports";
    public const string Users      = "Users";
    public const string Settings   = "Settings";
    public const string Backup     = "Backup";
    public const string License    = "License";
    public const string Branches   = "Branches";
    public const string Shifts     = "Shifts";
}

/// <summary>Permission action keys.</summary>
public static class PermissionActions
{
    public const string View    = "View";
    public const string Add     = "Add";
    public const string Edit    = "Edit";
    public const string Delete  = "Delete";
    public const string Print   = "Print";
    public const string Export  = "Export";
    public const string Return  = "Return";
    public const string Approve = "Approve";
    public const string Count   = "Count";
    public const string Transfer= "Transfer";
    public const string Adjust  = "Adjust";
    public const string Deposit = "Deposit";
    public const string Withdraw= "Withdraw";
    public const string Activate= "Activate";
    public const string Restore = "Restore";
    public const string Create  = "Create";
    public const string Open    = "Open";
    public const string Close   = "Close";
}
