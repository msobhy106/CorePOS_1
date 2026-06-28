namespace CorePOS.Application.Interfaces;

// ════════════════════════════════════════════════════════════════════
// BACKUP SERVICE INTERFACE
// ════════════════════════════════════════════════════════════════════
/// <summary>
/// Contract for all backup and restore operations.
/// Implementation uses SQL Server backup + ZIP compression + AES-256 encryption.
/// </summary>
public interface IBackupService
{
    // ── Manual Backup ─────────────────────────────────────────────
    /// <summary>
    /// Creates a full database backup, compresses to ZIP, optionally encrypts.
    /// Returns the full path of the created backup file.
    /// </summary>
    Task<BackupResult> CreateBackupAsync(
        BackupOptions options,
        IProgress<BackupProgress>? progress = null,
        CancellationToken ct = default);

    // ── Restore ───────────────────────────────────────────────────
    /// <summary>
    /// Validates and restores a backup file (.zip or .bak).
    /// Requires the application to restart after restore.
    /// </summary>
    Task<RestoreResult> RestoreBackupAsync(
        string backupFilePath,
        string? decryptionPassword = null,
        IProgress<BackupProgress>? progress = null,
        CancellationToken ct = default);

    // ── Validation ────────────────────────────────────────────────
    /// <summary>Reads backup file metadata without restoring.</summary>
    Task<BackupFileInfo?> ValidateBackupFileAsync(
        string filePath,
        string? password = null,
        CancellationToken ct = default);

    // ── Auto Backup ───────────────────────────────────────────────
    /// <summary>Runs an automatic backup (called by the scheduler).</summary>
    Task<BackupResult> RunAutoBackupAsync(
        AutoBackupSchedule schedule,
        CancellationToken ct = default);

    // ── Cloud upload ──────────────────────────────────────────────
    /// <summary>Uploads a backup file to Google Drive via rclone.</summary>
    Task<bool> UploadToGoogleDriveAsync(
        string filePath,
        CancellationToken ct = default);

    // ── Listing ───────────────────────────────────────────────────
    /// <summary>Returns list of backup files in the backup directory.</summary>
    Task<List<BackupFileInfo>> ListBackupsAsync(
        string? directory = null,
        CancellationToken ct = default);

    /// <summary>Deletes old backup files, keeping only the most recent N.</summary>
    Task CleanOldBackupsAsync(
        string directory,
        int keepCount,
        CancellationToken ct = default);
}

// ════════════════════════════════════════════════════════════════════
// DTOs
// ════════════════════════════════════════════════════════════════════
public record BackupOptions
{
    public string   BackupDirectory  { get; init; } = string.Empty;
    public string   DatabaseName     { get; init; } = "CorePOS";
    public bool     Compress         { get; init; } = true;
    public bool     Encrypt          { get; init; } = false;
    public string?  EncryptPassword  { get; init; }
    public bool     UploadCloud      { get; init; } = false;
    public string   BackupType       { get; init; } = "Full";   // Full | Differential
    public string?  Label            { get; init; }              // custom label
}

public record BackupResult
{
    public bool     Success      { get; init; }
    public string   FilePath     { get; init; } = string.Empty;
    public string   FileName     { get; init; } = string.Empty;
    public long     FileSizeBytes{ get; init; }
    public TimeSpan Duration     { get; init; }
    public string?  Error        { get; init; }
    public bool     Uploaded     { get; init; }

    public string FileSizeDisplay =>
        FileSizeBytes >= 1_048_576
            ? $"{FileSizeBytes / 1_048_576.0:N1} MB"
            : $"{FileSizeBytes / 1024.0:N1} KB";
}

public record RestoreResult
{
    public bool    Success   { get; init; }
    public string? Error     { get; init; }
    public string  BackupDate{ get; init; } = string.Empty;
    public string  DbVersion { get; init; } = string.Empty;
}

public record BackupFileInfo
{
    public string   FilePath     { get; init; } = string.Empty;
    public string   FileName     { get; init; } = string.Empty;
    public DateTime BackupDate   { get; init; }
    public long     FileSizeBytes{ get; init; }
    public bool     IsEncrypted  { get; init; }
    public bool     IsCompressed { get; init; }
    public string   DbVersion    { get; init; } = string.Empty;
    public string   BackupType   { get; init; } = "Full";
    public string?  Label        { get; init; }

    public string FileSizeDisplay =>
        FileSizeBytes >= 1_048_576
            ? $"{FileSizeBytes / 1_048_576.0:N1} MB"
            : $"{FileSizeBytes / 1024.0:N1} KB";

    public string DateDisplay => BackupDate.ToString("dd/MM/yyyy  HH:mm:ss");
}

public record BackupProgress(
    string Stage,          // "جاري النسخ", "جاري الضغط", "جاري التشفير", "جاري الرفع"
    int    PercentComplete,
    string Detail = "");

public enum AutoBackupSchedule { Daily, Weekly, Monthly }
