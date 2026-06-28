-- ============================================================
--  CorePOS Enterprise
--  Phase 5 — SQL Script 01: Database Creation & Configuration
--  SQL Server Express / Standard
--  Run as: sa or Windows Authentication with sysadmin role
-- ============================================================

USE master;
GO

-- ── Drop and recreate (dev only — remove in production) ──────
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'CorePOS')
BEGIN
    ALTER DATABASE CorePOS SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE CorePOS;
END
GO

-- ── Create database ──────────────────────────────────────────
CREATE DATABASE CorePOS
ON PRIMARY
(
    NAME = N'CorePOS_Data',
    FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\CorePOS.mdf',
    SIZE = 256MB,
    MAXSIZE = UNLIMITED,
    FILEGROWTH = 128MB
)
LOG ON
(
    NAME = N'CorePOS_Log',
    FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\CorePOS_log.ldf',
    SIZE = 64MB,
    MAXSIZE = 2048MB,
    FILEGROWTH = 64MB
);
GO

ALTER DATABASE CorePOS SET RECOVERY SIMPLE;       -- Suitable for desktop POS
ALTER DATABASE CorePOS SET AUTO_SHRINK OFF;
ALTER DATABASE CorePOS SET AUTO_UPDATE_STATISTICS ON;
ALTER DATABASE CorePOS SET AUTO_CREATE_STATISTICS ON;
ALTER DATABASE CorePOS COLLATE Arabic_CI_AS;      -- Arabic collation
GO

USE CorePOS;
GO

-- ── Ensure Arabic collation + UTF-8 ─────────────────────────
EXEC sp_executesql N'ALTER DATABASE CURRENT COLLATE Arabic_CI_AS';
GO

PRINT '✅ Database CorePOS created successfully.';
GO
