-- ================================================================
-- CorePOS Phase 12 — License System
-- SQL Script: License table
-- ================================================================

USE CorePOS;
GO

-- ── License table (stores activated license info) ────────────────
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Licenses')
BEGIN
    CREATE TABLE Licenses (
        LicenseId       INT IDENTITY(1,1) PRIMARY KEY,
        LicenseKey      NVARCHAR(MAX)   NOT NULL,   -- encrypted license blob
        LicenseType     NVARCHAR(50)    NOT NULL DEFAULT 'Trial',
        LicensedTo      NVARCHAR(200)   NULL,
        MachineId       NVARCHAR(100)   NOT NULL,
        IssuedDate      DATE            NOT NULL,
        ExpiryDate      DATE            NOT NULL,
        ActivatedAt     DATETIME2       NOT NULL DEFAULT GETDATE(),
        IsActive        BIT             NOT NULL DEFAULT 1,
        Notes           NVARCHAR(500)   NULL
    );
    PRINT 'Licenses table created.';
END
ELSE
    PRINT 'Licenses table already exists.';
GO
