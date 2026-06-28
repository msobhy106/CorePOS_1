-- ================================================================
-- CorePOS Phase 11 — Backup & Restore
-- SQL Script: BackupLog table + Backup settings seed
-- ================================================================

USE CorePOS;
GO

-- ── BackupLog table ─────────────────────────────────────────────
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BackupLogs')
BEGIN
    CREATE TABLE BackupLogs (
        BackupLogId     INT IDENTITY(1,1) PRIMARY KEY,
        FileName        NVARCHAR(300)   NOT NULL,
        FilePath        NVARCHAR(500)   NOT NULL,
        FileSizeBytes   BIGINT          NOT NULL DEFAULT 0,
        BackupType      NVARCHAR(20)    NOT NULL DEFAULT 'Manual',  -- Manual/Daily/Weekly/Monthly
        IsEncrypted     BIT             NOT NULL DEFAULT 0,
        IsUploaded      BIT             NOT NULL DEFAULT 0,
        Notes           NVARCHAR(500)   NULL,
        CreatedByUserId INT             NULL,
        CreatedAt       DATETIME2       NOT NULL DEFAULT GETDATE(),

        CONSTRAINT FK_BackupLogs_Users
            FOREIGN KEY (CreatedByUserId) REFERENCES Users(UserId)
    );

    CREATE INDEX IX_BackupLogs_CreatedAt
        ON BackupLogs (CreatedAt DESC);

    PRINT 'BackupLogs table created.';
END
ELSE
    PRINT 'BackupLogs table already exists.';
GO

-- ── Backup settings seed ─────────────────────────────────────────
-- Insert default backup settings (skip if already exist)
MERGE INTO Settings AS target
USING (VALUES
    ('BackupPath',            'C:\CorePOS_Backups'),
    ('BackupKeepCount',       '30'),
    ('BackupEncrypt',         'false'),
    ('BackupPassword',        ''),
    ('BackupUploadCloud',     'false'),
    ('RclonePath',            'rclone.exe'),
    ('RcloneRemote',          'gdrive'),
    ('RcloneFolder',          'CorePOS_Backups'),
    ('AutoBackupDaily',       'false'),
    ('AutoBackupDailyTime',   '23:00'),
    ('AutoBackupWeekly',      'false'),
    ('AutoBackupWeeklyDay',   '5'),
    ('AutoBackupWeeklyTime',  '22:00'),
    ('AutoBackupMonthly',     'false'),
    ('AutoBackupMonthlyDay',  '1'),
    ('AutoBackupMonthlyTime', '01:00'),
    ('LastBackup_Daily',      ''),
    ('LastBackup_Weekly',     ''),
    ('LastBackup_Monthly',    '')
) AS source (SettingKey, SettingValue)
ON target.SettingKey = source.SettingKey
WHEN NOT MATCHED THEN
    INSERT (SettingKey, SettingValue)
    VALUES (source.SettingKey, source.SettingValue);

PRINT 'Backup settings seeded.';
GO

-- ── View: backup summary ─────────────────────────────────────────
IF EXISTS (SELECT * FROM sys.views WHERE name = 'vw_BackupSummary')
    DROP VIEW vw_BackupSummary;
GO

CREATE VIEW vw_BackupSummary AS
SELECT
    bl.BackupLogId,
    bl.FileName,
    bl.FilePath,
    CAST(bl.FileSizeBytes / 1048576.0 AS DECIMAL(10,2)) AS FileSizeMB,
    bl.BackupType,
    bl.IsEncrypted,
    bl.IsUploaded,
    bl.CreatedAt,
    u.FullName AS CreatedByName
FROM BackupLogs bl
LEFT JOIN Users u ON bl.CreatedByUserId = u.UserId;
GO

PRINT 'Phase 11 SQL script completed successfully.';
GO
