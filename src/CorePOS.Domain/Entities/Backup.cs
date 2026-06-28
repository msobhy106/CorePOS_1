using CorePOS.Domain.Common;
using CorePOS.Domain.Enums;

namespace CorePOS.Domain.Entities;

public class Backup : BaseEntity
{
    public string     FileName      { get; private set; } = string.Empty;
    public string     FilePath      { get; private set; } = string.Empty;
    public BackupType BackupType    { get; private set; }
    public long       FileSizeBytes { get; private set; }
    public bool       IsSuccessful  { get; private set; } = true;
    public string?    ErrorMessage  { get; private set; }
    public int?       CreatedBy     { get; private set; }
    public DateTime   CreatedAt     { get; private set; } = DateTime.UtcNow;

    public User? User { get; private set; }

    protected Backup() { }

    public static Backup Create(string fileName, string filePath,
        BackupType backupType, long fileSize, bool isSuccessful = true,
        string? errorMessage = null, int? createdBy = null)
        => new()
        {
            FileName      = fileName,
            FilePath      = filePath,
            BackupType    = backupType,
            FileSizeBytes = fileSize,
            IsSuccessful  = isSuccessful,
            ErrorMessage  = errorMessage,
            CreatedBy     = createdBy
        };
}
