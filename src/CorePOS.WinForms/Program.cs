using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using MediatR;
using Serilog;
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
using CorePOS.WinForms.Infrastructure;
using CorePOS.Persistence;
using CorePOS.Infrastructure;

namespace CorePOS.WinForms;

internal static class Program
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    [STAThread]
    static void Main()
    {
        // ── Configure Serilog ─────────────────────────────────────
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                path:           Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "corepos-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30)
            .CreateLogger();

        try
        {
            Log.Information("Core POS starting...");

            // ── Windows Forms setup ───────────────────────────────
            ApplicationConfiguration.Initialize();
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // ── Build DI host ─────────────────────────────────────
            var host = CreateHostBuilder().Build();
            ServiceProvider = host.Services;

            // ── Check license before showing UI ───────────────────
            if (!CheckLicense())
            {
                MessageBox.Show(
                    "انتهت صلاحية الترخيص. يرجى التواصل مع الدعم الفني.",
                    "ترخيص منتهي",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // ── Show login form ───────────────────────────────────
            var loginForm = ServiceProvider.GetRequiredService<LoginForm>();
            Application.Run(loginForm);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Unhandled exception during startup");
            MessageBox.Show(
                $"خطأ غير متوقع أثناء تشغيل البرنامج:\n{ex.Message}",
                "خطأ",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    // ── DI Host Builder ───────────────────────────────────────────
    private static IHostBuilder CreateHostBuilder() =>
        Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureAppConfiguration((ctx, cfg) =>
            {
                cfg.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                   .AddJsonFile($"appsettings.{ctx.HostingEnvironment.EnvironmentName}.json", optional: true);
            })
            .ConfigureServices((ctx, services) =>
            {
                // ── Persistence (EF Core + Repositories) ─────────
                services.AddPersistence(ctx.Configuration);

                // ── Infrastructure (from Phase 8) ─────────────────
                services.AddInfrastructure(ctx.Configuration);

                // ── Application (from Phase 7) ────────────────────
                services.AddApplication();

                // ── MediatR ───────────────────────────────────────
                services.AddMediatR(cfg =>
                {
                    cfg.RegisterServicesFromAssembly(typeof(Application.AssemblyMarker).Assembly);
                });

                // ── WinForms — register all forms as Transient ────
                // Shell forms
                services.AddTransient<LoginForm>();
                services.AddTransient<MainForm>();

                // Module forms
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
                services.AddTransient<ReportsForm>();
                services.AddTransient<SettingsForm>();
            });

    // ── License check (stub — Phase 12 will implement fully) ─────
    private static bool CheckLicense()
    {
        try
        {
            var licensePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "license.dat");
            if (!File.Exists(licensePath))
            {
                // No license file → show trial warning but allow startup
                var result = MessageBox.Show(
                    "لم يتم العثور على ملف الترخيص.\nهل تريد تشغيل النسخة التجريبية؟",
                    "ترخيص",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                return result == DialogResult.Yes;
            }

            // Phase 12 will decrypt and validate the license file
            // For now: always valid if file exists
            return true;
        }
        catch
        {
            return true; // Don't block on license error during development
        }
    }
}
