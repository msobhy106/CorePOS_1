using Microsoft.Extensions.DependencyInjection;
using CorePOS.WinForms.Forms.Auth;
using CorePOS.WinForms.Forms.Dashboard;
using CorePOS.WinForms.Forms.MasterData;
using CorePOS.WinForms.Forms.Sales;
using CorePOS.WinForms.Forms.Purchases;
using CorePOS.WinForms.Forms.Inventory;
using CorePOS.WinForms.Forms.Finance;
using CorePOS.WinForms.Forms.Reports;
using CorePOS.WinForms.Forms.Settings;
using CorePOS.WinForms.Forms.Backup;
using CorePOS.WinForms.Forms.License;
using CorePOS.WinForms.Forms.POS;

namespace CorePOS.WinForms.DI;

/// <summary>
/// Registers WinForms forms as Transient services in DI.
/// Only registers classes that actually exist (BUG-070 fix).
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWinForms(this IServiceCollection services)
    {
        // ── Auth & Shell ──────────────────────────────────────────
        services.AddTransient<LoginForm>();
        services.AddTransient<MainForm>();

        // ── Dashboard ─────────────────────────────────────────────
        services.AddTransient<DashboardForm>();

        // ── POS ───────────────────────────────────────────────────
        services.AddTransient<POSForm>();

        // ── Master Data ───────────────────────────────────────────
        services.AddTransient<ProductsForm>();
        services.AddTransient<ProductEditDialog>();
        services.AddTransient<CustomersForm>();
        services.AddTransient<CustomerEditDialog>();
        services.AddTransient<SuppliersForm>();
        services.AddTransient<SupplierEditDialog>();
        services.AddTransient<EmployeesForm>();
        services.AddTransient<EmployeeEditDialog>();

        // ── Sales ─────────────────────────────────────────────────
        services.AddTransient<SalesListForm>();          // was SalesInvoiceListForm (BUG-070)

        // ── Purchases ─────────────────────────────────────────────
        services.AddTransient<PurchasesListForm>();      // was PurchaseInvoiceListForm (BUG-070)
        services.AddTransient<PurchaseEditForm>();
        services.AddTransient<PurchaseReturnDialog>();

        // ── Inventory ─────────────────────────────────────────────
        services.AddTransient<InventoryForm>();

        // ── Finance / Treasury ────────────────────────────────────
        services.AddTransient<FinanceForm>();            // was TreasuryForm (BUG-070)
        services.AddTransient<CashboxEditDialog>();

        // ── Reports ───────────────────────────────────────────────
        services.AddTransient<ReportsForm>();            // ReportsAndSettings.cs
        services.AddTransient<ReportsFormV2>();          // Reports/ReportsFormV2.cs

        // ── Settings ─────────────────────────────────────────────
        services.AddTransient<SettingsForm>();
        services.AddTransient<UserEditDialog>();
        services.AddTransient<UserPermissionsDialog>();

        // ── Backup & License ──────────────────────────────────────
        services.AddTransient<BackupForm>();
        services.AddTransient<LicenseForm>();

        return services;
    }
}
