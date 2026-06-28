-- ============================================================
--  CorePOS Enterprise
--  Phase 5 — SQL Script 05: DDL — Group G (Finance — Part 1)
--  Tables: CashBoxes, Shifts
--  (Created before Sales because SalesInvoices.ShiftId → Shifts)
-- ============================================================

USE CorePOS;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- ══════════════════════════════════════════════════════════════
-- TABLE: CashBoxes
-- ══════════════════════════════════════════════════════════════
CREATE TABLE dbo.CashBoxes
(
    Id             INT             NOT NULL IDENTITY(1,1),
    Code           NVARCHAR(20)    NOT NULL COLLATE Arabic_CI_AS,
    Name           NVARCHAR(200)   NOT NULL COLLATE Arabic_CI_AS,
    NameAr         NVARCHAR(200)   NOT NULL COLLATE Arabic_CI_AS,
    BranchId       INT             NOT NULL,
    IsMain         BIT             NOT NULL DEFAULT 0,
    OpeningBalance DECIMAL(18,4)   NOT NULL DEFAULT 0,
    CurrentBalance DECIMAL(18,4)   NOT NULL DEFAULT 0,
    IsActive       BIT             NOT NULL DEFAULT 1,
    CONSTRAINT PK_CashBoxes PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_CashBoxes_Code UNIQUE (Code),
    CONSTRAINT FK_CashBoxes_Branches
        FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id)
);
GO

CREATE NONCLUSTERED INDEX IX_CashBoxes_BranchId ON dbo.CashBoxes (BranchId);
GO

-- ══════════════════════════════════════════════════════════════
-- TABLE: Shifts
-- ══════════════════════════════════════════════════════════════
CREATE TABLE dbo.Shifts
(
    Id             INT             NOT NULL IDENTITY(1,1),
    ShiftNo        NVARCHAR(50)    NOT NULL COLLATE Arabic_CI_AS,
    UserId         INT             NOT NULL,
    BranchId       INT             NOT NULL,
    CashBoxId      INT             NOT NULL,
    OpeningBalance DECIMAL(18,4)   NOT NULL DEFAULT 0,
    ClosingBalance DECIMAL(18,4)   NOT NULL DEFAULT 0,
    ActualBalance  DECIMAL(18,4)   NOT NULL DEFAULT 0,
    StartTime      DATETIME2(0)    NOT NULL,
    EndTime        DATETIME2(0)    NULL,
    Status         TINYINT         NOT NULL DEFAULT 0,   -- 0=Open, 1=Closed
    Notes          NVARCHAR(MAX)   NULL,
    CONSTRAINT PK_Shifts PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Shifts_ShiftNo UNIQUE (ShiftNo),
    CONSTRAINT FK_Shifts_Users
        FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_Shifts_Branches
        FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id),
    CONSTRAINT FK_Shifts_CashBoxes
        FOREIGN KEY (CashBoxId) REFERENCES dbo.CashBoxes(Id),
    CONSTRAINT CK_Shifts_Status CHECK (Status IN (0,1))
);
GO

CREATE NONCLUSTERED INDEX IX_Shifts_UserId   ON dbo.Shifts (UserId);
CREATE NONCLUSTERED INDEX IX_Shifts_BranchId ON dbo.Shifts (BranchId);
CREATE NONCLUSTERED INDEX IX_Shifts_Status   ON dbo.Shifts (Status) INCLUDE (UserId, BranchId, CashBoxId);
GO

PRINT '✅ Script 05: Group G Part 1 (CashBoxes, Shifts) tables created.';
GO
