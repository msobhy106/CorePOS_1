-- ============================================================
--  CorePOS Enterprise
--  Phase 5 — SQL Script 11: MASTER SCRIPT
--  Runs all scripts in correct dependency order
--
--  USAGE:
--    sqlcmd -S .\SQLEXPRESS -E -i 11_MasterScript.sql
--    or open in SSMS and execute
--
--  PREREQUISITES:
--    - SQL Server Express / Standard installed
--    - Run as sysadmin or dbcreator
--    - All script files in same folder
-- ============================================================

PRINT '╔══════════════════════════════════════════════════╗';
PRINT '║        CorePOS Enterprise — Database Setup       ║';
PRINT '║        Phase 5 — Full Installation Script        ║';
PRINT '╚══════════════════════════════════════════════════╝';
PRINT '';

-- ── Script 01: Create Database ──────────────────────────────
PRINT '▶ [1/10] Creating database...';
:r 01_CreateDatabase.sql
PRINT '✅ Database created.';
PRINT '';

-- ── Script 02: Group A — System & Security ──────────────────
PRINT '▶ [2/10] Creating security tables...';
:r 02_DDL_GroupA_Security.sql
PRINT '✅ Security tables created.';
PRINT '';

-- ── Script 03: Group B — Master Data ───────────────────────
PRINT '▶ [3/10] Creating master data tables...';
:r 03_DDL_GroupB_MasterData.sql
PRINT '✅ Master data tables created.';
PRINT '';

-- ── Script 04: Group C — People ─────────────────────────────
PRINT '▶ [4/10] Creating people tables...';
:r 04_DDL_GroupC_People.sql
PRINT '✅ People tables created.';
PRINT '';

-- ── Script 05: Group G Part 1 — CashBoxes & Shifts ─────────
PRINT '▶ [5/10] Creating treasury tables...';
:r 05_DDL_GroupG_Finance_Part1.sql
PRINT '✅ Treasury tables created.';
PRINT '';

-- ── Script 06: Group D — Sales ──────────────────────────────
PRINT '▶ [6/10] Creating sales tables...';
:r 06_DDL_GroupD_Sales.sql
PRINT '✅ Sales tables created.';
PRINT '';

-- ── Script 07: Groups E, F, G2, H ───────────────────────────
PRINT '▶ [7/10] Creating purchases, inventory, finance tables...';
:r 07_DDL_Groups_E_F_G2_H.sql
PRINT '✅ Remaining tables created.';
PRINT '';

-- ── Script 08: Stored Procedures ────────────────────────────
PRINT '▶ [8/10] Creating stored procedures...';
:r 08_StoredProcedures.sql
PRINT '✅ Stored procedures created.';
PRINT '';

-- ── Script 09: Views ────────────────────────────────────────
PRINT '▶ [9/10] Creating views...';
:r 09_Views.sql
PRINT '✅ Views created.';
PRINT '';

-- ── Script 10: Seed Data ────────────────────────────────────
PRINT '▶ [10/10] Inserting seed data...';
:r 10_SeedData.sql
PRINT '✅ Seed data inserted.';
PRINT '';

-- ── Final summary ────────────────────────────────────────────
USE CorePOS;
GO

PRINT '════════════════════════════════════════════════════';
PRINT '  CorePOS Database Installation Complete!';
PRINT '';

SELECT
    'Tables'             AS ObjectType,
    COUNT(*)             AS Count
FROM sys.tables
WHERE type = 'U'
UNION ALL
SELECT 'Views',        COUNT(*) FROM sys.views
UNION ALL
SELECT 'Procedures',   COUNT(*) FROM sys.procedures
UNION ALL
SELECT 'Indexes',      COUNT(*) FROM sys.indexes WHERE type > 0
UNION ALL
SELECT 'Seed Roles',   COUNT(*) FROM dbo.Roles
UNION ALL
SELECT 'Permissions',  COUNT(*) FROM dbo.Permissions
UNION ALL
SELECT 'Settings',     COUNT(*) FROM dbo.Settings;

PRINT '';
PRINT '  Default Login:  admin / (set on first run)';
PRINT '  Trial License:  7 days';
PRINT '════════════════════════════════════════════════════';
GO
