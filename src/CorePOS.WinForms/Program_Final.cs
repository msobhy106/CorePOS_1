// ════════════════════════════════════════════════════════════════════
// MAINFORM UPDATES — Add to existing MainForm.cs
// Phase 11: Backup navigation
// Phase 12: License navigation + expiry warning
// ════════════════════════════════════════════════════════════════════

/*
 * 1. Add to NavPages constants:
 */
// public const string Backup  = "Backup";
// public const string License = "License";

/*
 * 2. Add to sidebar menu items in BuildSidebarMenu():
 */
// new SidebarItemDef("💾",  "النسخ الاحتياطي", NavPages.Backup,  Modules.Backup),
// new SidebarItemDef("🔑",  "الترخيص",         NavPages.License, null),

/*
 * 3. Add to NavigateTo() switch:
 */
// NavPages.Backup  => Program.ServiceProvider.GetRequiredService<BackupForm>(),
// NavPages.License => Program.ServiceProvider.GetRequiredService<LicenseForm>(),

/*
 * 4. Add to DI in Program.cs:
 */
// services.AddTransient<BackupForm>();
// services.AddTransient<LicenseForm>();

/*
 * 5. Full updated Program.cs services registration:
 */

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using CorePOS.Application.Interfaces;
using CorePOS.Infrastructure;
using CorePOS.WinForms.Forms;
using CorePOS.WinForms.Forms.Auth;
using CorePOS.WinForms.Forms.Dashboard;
using CorePOS.WinForms.Forms.POS;
using CorePOS.WinForms.Forms.Sales;
using CorePOS.WinForms.Forms.Purchases;
using CorePOS.WinForms.Forms.Inventory;
using CorePOS.WinForms.Forms.Finance;
using CorePOS.WinForms.Forms.MasterData;
using CorePOS.WinForms.Forms.Reports;
using CorePOS.WinForms.Forms.Settings;
using CorePOS.WinForms.Forms.Backup;
using CorePOS.WinForms.Forms.License;

namespace CorePOS.WinForms;

internal static class ProgramFinal
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    [STAThread]
    static async Task Main()
    {
        ApplicationConfiguration.Initialize();
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        var host = CreateHostBuilder().Build();
        ServiceProvider = host.Services;

        // ── License check ──────────────────────────────────────
        var licenseService = ServiceProvider.GetRequiredService<ILicenseService>();
        var licenseInfo    = await licenseService.ValidateLicenseAsync();

        if (!licenseInfo.IsValid)
        {
            using var guard = new LicenseGuard(licenseService, licenseInfo);
            Application.Run(guard);
            if (!guard.AllowContinue)
            {
                Application.Exit();
                return;
            }
        }
        else if (licenseInfo.WillExpireSoon)
        {
            MessageBox.Show(
                $"⚠ تنبيه: سينتهي الترخيص خلال {licenseInfo.DaysRemaining} أيام.\nيرجى تجديد الترخيص.",
                "تجديد الترخيص", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        // ── Start background services (auto backup scheduler) ──
        await host.StartAsync();

        // ── Show login ─────────────────────────────────────────
        var loginForm = ServiceProvider.GetRequiredService<LoginForm>();
        Application.Run(loginForm);

        await host.StopAsync();
    }

    private static IHostBuilder CreateHostBuilder() =>
        Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((_, cfg) =>
                cfg.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                   .AddJsonFile("appsettings.json", optional: false))
            .ConfigureServices((ctx, services) =>
            {
                // ── Phase 6/7/8: Domain + Application + Infrastructure ──
                services.AddInfrastructure(ctx.Configuration);
                services.AddApplication();

                // ── Phase 10: Reports + Printing ──
                services.AddPhase10Services(ctx.Configuration);

                // ── Phase 11: Backup + Auto Scheduler ──
                services.AddPhase11Services(ctx.Configuration);

                // ── Phase 12: License ──
                services.AddPhase12Services();

                // ── MediatR ──
                services.AddMediatR(cfg =>
                    cfg.RegisterServicesFromAssembly(
                        typeof(Application.AssemblyMarker).Assembly));

                // ── Memory Cache (for SettingsRepository) ──
                services.AddMemoryCache();

                // ── WinForms: all forms ──
                // Shell
                services.AddTransient<LoginForm>();
                services.AddTransient<MainForm>();
                // Modules
                services.AddTransient<DashboardForm>();
                services.AddTransient<POSForm>();
                services.AddTransient<SalesListForm>();
                services.AddTransient<PurchasesListForm>();
                services.AddTransient<InventoryForm>();
                services.AddTransient<FinanceForm>();
                services.AddTransient<CustomersForm>();
                services.AddTransient<SuppliersForm>();
                services.AddTransient<ProductsForm>();
                services.AddTransient<EmployeesForm>();
                services.AddTransient<ReportsFormV2>();
                services.AddTransient<SettingsForm>();
                // Phase 11
                services.AddTransient<BackupForm>();
                // Phase 12
                services.AddTransient<LicenseForm>();
            });
}
