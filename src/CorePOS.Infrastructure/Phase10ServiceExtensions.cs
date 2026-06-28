using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using CorePOS.Application.Interfaces;
using CorePOS.Infrastructure.Services;

namespace CorePOS.Infrastructure;

/// <summary>
/// Phase 10 additions to ServiceExtensions.
/// Add these registrations to the existing AddInfrastructure() method in Phase 8.
/// </summary>
public static class Phase10ServiceExtensions
{
    /// <summary>
    /// Call this inside AddInfrastructure() after existing registrations.
    /// services.AddPhase10Services(configuration);
    /// </summary>
    public static IServiceCollection AddPhase10Services(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Report Service ────────────────────────────────────────
        services.AddScoped<IReportService, ReportService>();

        // ── Thermal Printer Service ───────────────────────────────
        services.AddScoped<IPrinterService, ThermalPrinterService>();

        // ── Settings Repository (if not already registered in Phase 8) ──
        // services.AddScoped<ISettingsRepository, SettingsRepository>();

        return services;
    }
}
