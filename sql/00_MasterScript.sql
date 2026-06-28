-- ================================================================
-- CorePOS — MASTER SQL SCRIPT
-- يشغّل جميع scripts المراحل بالترتيب الصحيح
-- ================================================================
-- كيفية الاستخدام:
-- 1. افتح SQL Server Management Studio
-- 2. اتصل بـ SQL Server الخاص بك
-- 3. شغّل هذا الملف
-- ================================================================

PRINT '========================================';
PRINT ' CorePOS — Master Database Setup Script ';
PRINT ' Core Tech © 2024                       ';
PRINT '========================================';
PRINT '';

-- Phase 5: Core database schema
PRINT '>>> Phase 5: Creating database and tables...';
:r .\Phase5_01_CreateDatabase.sql
:r .\Phase5_02_DDL_Security.sql
:r .\Phase5_03_DDL_MasterData.sql
:r .\Phase5_04_DDL_People.sql
:r .\Phase5_05_DDL_Finance.sql
:r .\Phase5_06_DDL_Sales.sql
:r .\Phase5_07_DDL_Inventory.sql
:r .\Phase5_08_StoredProcedures.sql
:r .\Phase5_09_Views.sql
:r .\Phase5_10_SeedData.sql
PRINT '>>> Phase 5: Done.';
PRINT '';

-- Phase 11: Backup log table
PRINT '>>> Phase 11: Adding BackupLog table...';
:r .\Phase11_BackupLog.sql
PRINT '>>> Phase 11: Done.';
PRINT '';

-- Phase 12: License table
PRINT '>>> Phase 12: Adding License table...';
:r .\Phase12_License.sql
PRINT '>>> Phase 12: Done.';
PRINT '';

PRINT '========================================';
PRINT ' Database setup completed successfully! ';
PRINT '========================================';
GO
