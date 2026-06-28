using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CorePOS.Domain.Interfaces;
using CorePOS.Persistence.DbContexts;
using CorePOS.Persistence.Repositories;

namespace CorePOS.Persistence;

public static class ServiceExtensions
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── EF Core SQL Server ────────────────────────────
        services.AddDbContext<CorePOSDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql =>
                {
                    sql.CommandTimeout(60);
                    sql.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null);
                });

#if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
#endif
        });

        // ── Memory Cache (BUG-069: required by SettingsRepository) ──
        services.AddMemoryCache();

        // ── Unit of Work ──────────────────────────────────
        services.AddScoped<IUnitOfWork,          UnitOfWork>();

        // ── Existing Repositories ─────────────────────────
        services.AddScoped<IProductRepository,   ProductRepository>();
        services.AddScoped<ICustomerRepository,  CustomerRepository>();
        services.AddScoped<ISupplierRepository,  SupplierRepository>();
        services.AddScoped<ISalesRepository,     SalesRepository>();
        services.AddScoped<IPurchaseRepository,  PurchaseRepository>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IUserRepository,      UserRepository>();
        services.AddScoped<ISettingsRepository,  SettingsRepository>();
        services.AddScoped<ILicenseRepository,   LicenseRepository>();

        // ── New Repositories (BUG-067) ────────────────────
        services.AddScoped<IBranchRepository,    BranchRepository>();
        services.AddScoped<IWarehouseRepository, WarehouseRepository>();
        services.AddScoped<ICashBoxRepository,   CashBoxRepository>();
        services.AddScoped<IExpenseRepository,   ExpenseRepository>();
        services.AddScoped<IShiftRepository,     ShiftRepository>();
        services.AddScoped<IEmployeeRepository,  EmployeeRepository>();
        services.AddScoped<ICategoryRepository,  CategoryRepository>();
        services.AddScoped<IBackupRepository,    BackupRepository>();

        // ── IApplicationDbContext ─────────────────────────
        services.AddScoped<Application.Interfaces.IApplicationDbContext>(
            sp => sp.GetRequiredService<CorePOSDbContext>());

        return services;
    }
}
