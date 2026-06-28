using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CorePOS.Domain.Entities;
using CorePOS.Domain.Enums;
using CorePOS.Persistence.DbContexts;

namespace CorePOS.Persistence.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(CorePOSDbContext db,
        ILogger logger, CancellationToken ct = default)
    {
        try
        {
            // Apply pending migrations
            await db.Database.MigrateAsync(ct);

            // Seed only if empty
            if (!await db.Roles.AnyAsync(ct))
                await SeedRolesAndPermissionsAsync(db, ct);

            if (!await db.Branches.AnyAsync(ct))
                await SeedMasterDataAsync(db, ct);

            if (!await db.Users.AnyAsync(ct))
                await SeedAdminUserAsync(db, ct);

            if (!await db.Licenses.AnyAsync(ct))
                await SeedTrialLicenseAsync(db, ct);

            if (!await db.Settings.AnyAsync(ct))
                await SeedSettingsAsync(db, ct);

            await db.SaveChangesAsync(ct);
            logger.LogInformation("Database seeded successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database seeding failed.");
            throw;
        }
    }

    private static async Task SeedRolesAndPermissionsAsync(
        CorePOSDbContext db, CancellationToken ct)
    {
        var roles = new[]
        {
            Role.Create("Admin",          "مدير النظام",    "صلاحيات كاملة",        isSystem: true),
            Role.Create("Manager",        "مدير الفرع",     "إدارة الفرع",           isSystem: true),
            Role.Create("Cashier",        "كاشير",          "شاشة البيع",            isSystem: true),
            Role.Create("PurchaseClerk",  "مسؤول مشتريات","فواتير الشراء",          isSystem: true),
            Role.Create("Accountant",     "محاسب",          "الخزائن والتقارير",     isSystem: true),
            Role.Create("WarehouseClerk", "أمين مخزن",     "إدارة المخزون",         isSystem: true),
        };
        await db.Roles.AddRangeAsync(roles, ct);
        await db.SaveChangesAsync(ct);

        // Seed system permissions
        var modules = new Dictionary<string, string>
        {
            ["Dashboard"] = "لوحة التحكم", ["Products"] = "الأصناف",
            ["Categories"] = "الأقسام",    ["Units"] = "الوحدات",
            ["Customers"] = "العملاء",      ["Suppliers"] = "الموردين",
            ["Sales"] = "المبيعات",         ["Purchases"] = "المشتريات",
            ["Inventory"] = "المخزون",      ["Treasury"] = "الخزائن",
            ["Expenses"] = "المصروفات",     ["Employees"] = "الموظفين",
            ["Reports"] = "التقارير",       ["Users"] = "المستخدمين",
            ["Settings"] = "الإعدادات",     ["Backup"] = "النسخ الاحتياطي",
            ["License"] = "الترخيص",        ["Branches"] = "الفروع",
            ["Shifts"] = "الورديات",
        };

        var actions = new Dictionary<string, string>
        {
            ["View"] = "عرض",  ["Add"] = "إضافة",   ["Edit"] = "تعديل",
            ["Delete"] = "حذف",["Print"] = "طباعة", ["Export"] = "تصدير",
            ["Return"] = "مرتجع", ["Approve"] = "اعتماد", ["Open"] = "فتح",
            ["Close"] = "إغلاق",
        };

        var permissions = new List<Permission>();
        foreach (var (mk, mName) in modules)
        {
            // Add relevant actions per module
            var moduleActions = mk switch
            {
                "Dashboard"  => new[] { "View" },
                "Shifts"     => new[] { "View", "Open", "Close" },
                "License"    => new[] { "View", "Add" },
                "Backup"     => new[] { "View", "Add" },
                "Settings"   => new[] { "View", "Edit" },
                "Reports"    => new[] { "View", "Print", "Export" },
                "Sales"      => new[] { "View", "Add", "Edit", "Delete", "Print", "Return" },
                "Purchases"  => new[] { "View", "Add", "Edit", "Delete", "Print", "Return", "Approve" },
                _            => new[] { "View", "Add", "Edit", "Delete" }
            };

            foreach (var ak in moduleActions)
            {
                if (actions.TryGetValue(ak, out var aName))
                    permissions.Add(Permission.Create(mk, ak, mName, aName));
            }
        }

        await db.Permissions.AddRangeAsync(permissions, ct);
        await db.SaveChangesAsync(ct);

        // Give Admin all permissions
        var adminRole = await db.Roles.FirstAsync(r => r.Name == "Admin", ct);
        var allPerms  = await db.Permissions.ToListAsync(ct);
        var rolePerms = allPerms.Select(p => RolePermission.Create(adminRole.Id, p.Id));
        await db.RolePermissions.AddRangeAsync(rolePerms, ct);
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedMasterDataAsync(
        CorePOSDbContext db, CancellationToken ct)
    {
        var branch = Branch.Create("BR001", "Main Branch", "الفرع الرئيسي", isMain: true);
        await db.Branches.AddAsync(branch, ct);
        await db.SaveChangesAsync(ct);

        var warehouse = Warehouse.Create("WH001", "Main Warehouse", "المخزن الرئيسي", branch.Id, isMain: true);
        await db.Warehouses.AddAsync(warehouse, ct);

        var cashbox = CashBox.Create("CB001", "Main CashBox", "الخزينة الرئيسية", branch.Id, isMain: true);
        await db.CashBoxes.AddAsync(cashbox, ct);

        // Units
        var units = new[]
        {
            Unit.Create("PCS","Piece","قطعة","قطعة"),
            Unit.Create("KG","Kilogram","كيلوجرام","كجم"),
            Unit.Create("GM","Gram","جرام","جم"),
            Unit.Create("LTR","Liter","لتر","لتر"),
            Unit.Create("BOX","Box","صندوق","صندوق"),
            Unit.Create("PKT","Packet","باكت","باكت"),
            Unit.Create("CTN","Carton","كرتون","كرتون"),
            Unit.Create("DZN","Dozen","دزينة","دزينة"),
        };
        await db.Units.AddRangeAsync(units, ct);

        // Categories
        var mainCat = Category.CreateMain("GEN","General","عام");
        await db.Categories.AddAsync(mainCat, ct);

        // Default customer
        var walkIn = Customer.Create("CASH", "عميل نقدي");
        await db.Customers.AddAsync(walkIn, ct);

        // Expense categories
        var expCats = new[]
        {
            ExpenseCategory.Create("Rent",        "إيجار",     isSystem: true),
            ExpenseCategory.Create("Electricity",  "كهرباء",   isSystem: true),
            ExpenseCategory.Create("Water",        "مياه",      isSystem: true),
            ExpenseCategory.Create("Internet",     "إنترنت",   isSystem: true),
            ExpenseCategory.Create("Salaries",     "مرتبات",   isSystem: true),
            ExpenseCategory.Create("Transport",    "نقل",       isSystem: true),
            ExpenseCategory.Create("Other",        "أخرى",     isSystem: true),
        };
        await db.ExpenseCategories.AddRangeAsync(expCats, ct);
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedAdminUserAsync(
        CorePOSDbContext db, CancellationToken ct)
    {
        var adminRole = await db.Roles.FirstAsync(r => r.Name == "Admin", ct);
        var branch    = await db.Branches.FirstAsync(ct);
        var warehouse = await db.Warehouses.FirstAsync(ct);

        // Hash of "Admin@123" using BCrypt
        var hash = BCrypt.Net.BCrypt.HashPassword("Admin@123", 11);
        var admin = User.Create("admin", hash, "System Administrator",
            adminRole.Id, branch.Id, warehouse.Id,
            "مدير النظام", null, null);

        await db.Users.AddAsync(admin, ct);
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedTrialLicenseAsync(
        CorePOSDbContext db, CancellationToken ct)
    {
        var license = Domain.Entities.License.CreateTrial(7);
        await db.Licenses.AddAsync(license, ct);
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedSettingsAsync(
        CorePOSDbContext db, CancellationToken ct)
    {
        var settings = new Dictionary<string, (string? Value, string Group, string? DataType)>
        {
            ["CompanyName"]           = ("Core POS",       "Company",  "string"),
            ["CompanyNameAr"]         = ("Core POS",       "Company",  "string"),
            ["CurrencyName"]          = ("جنيه",           "Finance",  "string"),
            ["CurrencySymbol"]        = ("ج.م",            "Finance",  "string"),
            ["DecimalPlaces"]         = ("2",               "Finance",  "int"),
            ["DefaultTaxPercent"]     = ("0",               "Finance",  "decimal"),
            ["POSRequireShift"]       = ("true",            "POS",      "bool"),
            ["POSAllowNegativeStock"] = ("false",           "POS",      "bool"),
            ["PrinterSize"]           = ("80mm",            "Printing", "string"),
            ["PrinterAskEveryTime"]   = ("false",           "Printing", "bool"),
            ["PrintReceiptOnSale"]    = ("true",            "Printing", "bool"),
            ["PrintCopies"]           = ("1",               "Printing", "int"),
            ["InvoiceFooterText"]     = ("شكراً لزيارتكم","Printing", "string"),
            ["CashDrawerEnabled"]     = ("false",           "Hardware", "bool"),
            ["BarcodeScannerEnabled"] = ("false",           "Hardware", "bool"),
            ["BackupAutoEnabled"]     = ("true",            "Backup",   "bool"),
            ["BackupSchedule"]        = ("Daily",           "Backup",   "string"),
            ["BackupPath"]            = (@"C:\CorePOS\Backups","Backup","string"),
            ["BackupRetainDays"]      = ("30",              "Backup",   "int"),
            ["ForcePasswordChange"]   = ("true",            "System",   "bool"),
            ["AppVersion"]            = ("1.0.0",           "System",   "string"),
        };

        foreach (var kv in settings)
        {
            var s = Setting.Create(kv.Key, kv.Value.Value,
                kv.Value.Group, kv.Value.DataType);
            await db.Settings.AddAsync(s, ct);
        }

        await db.SaveChangesAsync(ct);
    }
}
