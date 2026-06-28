-- ============================================================
--  CorePOS Enterprise
--  Phase 5 — SQL Script 02: DDL — Group A: System & Security
--  Tables: Licenses, Roles, Permissions, RolePermissions,
--          Users, AuditLogs, Sequences
-- ============================================================

USE CorePOS;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- ══════════════════════════════════════════════════════════════
-- TABLE: Licenses
-- ══════════════════════════════════════════════════════════════
CREATE TABLE dbo.Licenses
(
    Id              INT             NOT NULL IDENTITY(1,1),
    LicenseKey      NVARCHAR(100)   NOT NULL,
    ActivationCode  NVARCHAR(200)   NULL,
    MachineId       NVARCHAR(200)   NULL,
    LicenseType     TINYINT         NOT NULL DEFAULT 0,
        -- 0=Trial, 1=Standard, 2=Professional
    StartDate       DATETIME2(0)    NOT NULL,
    ExpiryDate      DATETIME2(0)    NOT NULL,
    IsActive        BIT             NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2(0)    NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Licenses PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Licenses_LicenseKey UNIQUE (LicenseKey),
    CONSTRAINT CK_Licenses_Type CHECK (LicenseType IN (0,1,2))
);
GO

CREATE NONCLUSTERED INDEX IX_Licenses_ExpiryDate ON dbo.Licenses (ExpiryDate) INCLUDE (IsActive, LicenseType);
GO

-- ══════════════════════════════════════════════════════════════
-- TABLE: Roles
-- ══════════════════════════════════════════════════════════════
CREATE TABLE dbo.Roles
(
    Id          INT             NOT NULL IDENTITY(1,1),
    Name        NVARCHAR(100)   NOT NULL COLLATE Arabic_CI_AS,
    NameAr      NVARCHAR(100)   NOT NULL COLLATE Arabic_CI_AS,
    Description NVARCHAR(500)   NULL,
    IsSystem    BIT             NOT NULL DEFAULT 0,  -- system roles cannot be deleted
    IsActive    BIT             NOT NULL DEFAULT 1,
    CreatedAt   DATETIME2(0)    NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Roles PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Roles_Name UNIQUE (Name)
);
GO

-- ══════════════════════════════════════════════════════════════
-- TABLE: Permissions
-- ══════════════════════════════════════════════════════════════
CREATE TABLE dbo.Permissions
(
    Id            INT             NOT NULL IDENTITY(1,1),
    ModuleKey     NVARCHAR(100)   NOT NULL,
    ActionKey     NVARCHAR(50)    NOT NULL,
    ModuleNameAr  NVARCHAR(100)   NOT NULL COLLATE Arabic_CI_AS,
    ActionNameAr  NVARCHAR(50)    NOT NULL COLLATE Arabic_CI_AS,
    CONSTRAINT PK_Permissions PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Permissions_ModuleAction UNIQUE (ModuleKey, ActionKey)
);
GO

-- ══════════════════════════════════════════════════════════════
-- TABLE: RolePermissions (M:M junction)
-- ══════════════════════════════════════════════════════════════
CREATE TABLE dbo.RolePermissions
(
    Id            INT     NOT NULL IDENTITY(1,1),
    RoleId        INT     NOT NULL,
    PermissionId  INT     NOT NULL,
    CONSTRAINT PK_RolePermissions PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_RolePermissions UNIQUE (RoleId, PermissionId),
    CONSTRAINT FK_RolePermissions_Roles
        FOREIGN KEY (RoleId) REFERENCES dbo.Roles(Id) ON DELETE CASCADE,
    CONSTRAINT FK_RolePermissions_Permissions
        FOREIGN KEY (PermissionId) REFERENCES dbo.Permissions(Id) ON DELETE CASCADE
);
GO

CREATE NONCLUSTERED INDEX IX_RolePermissions_RoleId ON dbo.RolePermissions (RoleId);
CREATE NONCLUSTERED INDEX IX_RolePermissions_PermissionId ON dbo.RolePermissions (PermissionId);
GO

-- ══════════════════════════════════════════════════════════════
-- TABLE: Branches (needed before Users FK)
-- ══════════════════════════════════════════════════════════════
CREATE TABLE dbo.Branches
(
    Id          INT             NOT NULL IDENTITY(1,1),
    Code        NVARCHAR(20)    NOT NULL COLLATE Arabic_CI_AS,
    Name        NVARCHAR(200)   NOT NULL COLLATE Arabic_CI_AS,
    NameAr      NVARCHAR(200)   NOT NULL COLLATE Arabic_CI_AS,
    Address     NVARCHAR(500)   NULL,
    Phone       NVARCHAR(50)    NULL,
    ManagerName NVARCHAR(200)   NULL COLLATE Arabic_CI_AS,
    IsMain      BIT             NOT NULL DEFAULT 0,
    IsActive    BIT             NOT NULL DEFAULT 1,
    IsDeleted   BIT             NOT NULL DEFAULT 0,
    CreatedAt   DATETIME2(0)    NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Branches PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Branches_Code UNIQUE (Code)
);
GO

-- ══════════════════════════════════════════════════════════════
-- TABLE: Warehouses (needed before Users FK)
-- ══════════════════════════════════════════════════════════════
CREATE TABLE dbo.Warehouses
(
    Id          INT             NOT NULL IDENTITY(1,1),
    Code        NVARCHAR(20)    NOT NULL COLLATE Arabic_CI_AS,
    Name        NVARCHAR(200)   NOT NULL COLLATE Arabic_CI_AS,
    NameAr      NVARCHAR(200)   NOT NULL COLLATE Arabic_CI_AS,
    BranchId    INT             NOT NULL,
    Address     NVARCHAR(500)   NULL,
    ManagerName NVARCHAR(200)   NULL COLLATE Arabic_CI_AS,
    IsMain      BIT             NOT NULL DEFAULT 0,
    IsActive    BIT             NOT NULL DEFAULT 1,
    IsDeleted   BIT             NOT NULL DEFAULT 0,
    CreatedAt   DATETIME2(0)    NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Warehouses PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Warehouses_Code UNIQUE (Code),
    CONSTRAINT FK_Warehouses_Branches
        FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id)
);
GO

CREATE NONCLUSTERED INDEX IX_Warehouses_BranchId ON dbo.Warehouses (BranchId);
GO

-- ══════════════════════════════════════════════════════════════
-- TABLE: Users
-- ══════════════════════════════════════════════════════════════
CREATE TABLE dbo.Users
(
    Id            INT             NOT NULL IDENTITY(1,1),
    Username      NVARCHAR(100)   NOT NULL,
    PasswordHash  NVARCHAR(256)   NOT NULL,
    FullName      NVARCHAR(200)   NOT NULL COLLATE Arabic_CI_AS,
    FullNameAr    NVARCHAR(200)   NULL COLLATE Arabic_CI_AS,
    Email         NVARCHAR(200)   NULL,
    Phone         NVARCHAR(50)    NULL,
    PhotoPath     NVARCHAR(500)   NULL,
    RoleId        INT             NOT NULL,
    BranchId      INT             NULL,
    WarehouseId   INT             NULL,
    IsActive      BIT             NOT NULL DEFAULT 1,
    IsDeleted     BIT             NOT NULL DEFAULT 0,
    LastLogin     DATETIME2(0)    NULL,
    CreatedAt     DATETIME2(0)    NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy     INT             NULL,
    CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Users_Username UNIQUE (Username),
    CONSTRAINT FK_Users_Roles
        FOREIGN KEY (RoleId) REFERENCES dbo.Roles(Id),
    CONSTRAINT FK_Users_Branches
        FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id),
    CONSTRAINT FK_Users_Warehouses
        FOREIGN KEY (WarehouseId) REFERENCES dbo.Warehouses(Id),
    CONSTRAINT FK_Users_CreatedBy
        FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(Id)
);
GO

CREATE NONCLUSTERED INDEX IX_Users_Username  ON dbo.Users (Username) WHERE IsDeleted = 0;
CREATE NONCLUSTERED INDEX IX_Users_RoleId    ON dbo.Users (RoleId);
CREATE NONCLUSTERED INDEX IX_Users_BranchId  ON dbo.Users (BranchId) WHERE BranchId IS NOT NULL;
GO

-- ══════════════════════════════════════════════════════════════
-- TABLE: AuditLogs
-- ══════════════════════════════════════════════════════════════
CREATE TABLE dbo.AuditLogs
(
    Id          BIGINT          NOT NULL IDENTITY(1,1),
    UserId      INT             NOT NULL,
    Action      NVARCHAR(100)   NOT NULL,   -- Create,Update,Delete,Login,Logout,Print...
    EntityName  NVARCHAR(100)   NOT NULL,   -- SalesInvoice,Product,Customer...
    EntityId    NVARCHAR(50)    NULL,
    OldValues   NVARCHAR(MAX)   NULL,       -- JSON snapshot before change
    NewValues   NVARCHAR(MAX)   NULL,       -- JSON snapshot after change
    IPAddress   NVARCHAR(50)    NULL,
    MachineName NVARCHAR(100)   NULL,
    CreatedAt   DATETIME2(0)    NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_AuditLogs PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_AuditLogs_Users
        FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
);
GO

CREATE NONCLUSTERED INDEX IX_AuditLogs_UserId     ON dbo.AuditLogs (UserId);
CREATE NONCLUSTERED INDEX IX_AuditLogs_EntityName ON dbo.AuditLogs (EntityName, EntityId);
CREATE NONCLUSTERED INDEX IX_AuditLogs_CreatedAt  ON dbo.AuditLogs (CreatedAt DESC);
GO

-- ══════════════════════════════════════════════════════════════
-- TABLE: Sequences (atomic invoice numbering)
-- ══════════════════════════════════════════════════════════════
CREATE TABLE dbo.Sequences
(
    Id           INT             NOT NULL IDENTITY(1,1),
    SequenceKey  NVARCHAR(100)   NOT NULL,
    CurrentValue INT             NOT NULL DEFAULT 0,
    Prefix       NVARCHAR(20)    NULL,
    CONSTRAINT PK_Sequences PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Sequences_Key UNIQUE (SequenceKey)
);
GO

PRINT '✅ Script 02: Group A (System & Security) tables created.';
GO
