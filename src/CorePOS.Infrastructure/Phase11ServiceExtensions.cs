using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using CorePOS.Application.Interfaces;
using CorePOS.Infrastructure.Backup;

namespace CorePOS.Infrastructure;

/// <summary>Phase 11 — DI registrations. Add to AddInfrastructure().</summary>
public static class Phase11ServiceExtensions
{
    public static IServiceCollection AddPhase11Services(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Backup service ────────────────────────────────────────
        services.AddScoped<IBackupService, BackupService>();

        // ── Auto backup background scheduler ─────────────────────
        services.AddHostedService<AutoBackupScheduler>();

        return services;
    }
}

// ════════════════════════════════════════════════════════════════════
// DOMAIN ENTITY — BackupLog
// ════════════════════════════════════════════════════════════════════
namespace CorePOS.Domain.Entities;

/// <summary>Tracks backup history in the database.</summary>
public class BackupLog
{
    public int      BackupLogId   { get; set; }
    public string   FileName      { get; set; } = string.Empty;
    public string   FilePath      { get; set; } = string.Empty;
    public long     FileSizeBytes { get; set; }
    public string   BackupType    { get; set; } = "Manual";  // Manual | Daily | Weekly | Monthly
    public bool     IsEncrypted   { get; set; }
    public bool     IsUploaded    { get; set; }
    public DateTime CreatedAt     { get; set; } = DateTime.Now;
    public string?  Notes         { get; set; }

    // Navigation
    public int?  CreatedByUserId { get; set; }
    public User? CreatedBy       { get; set; }
}
