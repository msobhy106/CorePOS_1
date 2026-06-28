using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using CorePOS.Application.Interfaces;
using CorePOS.Infrastructure.Security;
using CorePOS.Infrastructure.Services;
using CorePOS.Infrastructure.Logging;
using CorePOS.Infrastructure.Printing;
using CorePOS.Infrastructure.Hardware;
using CorePOS.Infrastructure.Backup;
using CorePOS.Infrastructure.License;

namespace CorePOS.Infrastructure;

public static class ServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Security
        services.AddSingleton<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IPasswordHasher,     PasswordHasher>();

        // Audit
        services.AddScoped<IAuditService, AuditService>();

        // Hardware & Printing
        services.AddSingleton<IPrinterService,    ThermalPrinterService>();
        services.AddSingleton<IBarcodeService,    BarcodeScannerService>();
        services.AddSingleton<ICashDrawerService, CashDrawerService>();

        // Business Services
        services.AddScoped<ISequenceService,      SequenceService>();
        services.AddScoped<ILicenseService,       LicenseService>();
        services.AddScoped<IBackupService,        BackupService>();
        services.AddScoped<IReportService,        ReportService>();  // BUG-068

        return services;
    }
}
// (ReportService registered separately — requires Reporting project reference)
