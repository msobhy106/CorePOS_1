-- ============================================================
--  CorePOS Enterprise
--  Phase 5 — SQL Script 04: DDL — Group C: People
--  Tables: CustomerGroups, Customers, LoyaltyPoints, Coupons,
--          (Suppliers already created in Script 03),
--          Employees, EmployeeTransactions, DeliveryAgents
-- ============================================================

USE CorePOS;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- ══════════════════════════════════════════════════════════════
-- TABLE: CustomerGroups
-- ══════════════════════════════════════════════════════════════
CREATE TABLE dbo.CustomerGroups
(
    Id                INT             NOT NULL IDENTITY(1,1),
    Name              NVARCHAR(200)   NOT NULL COLLATE Arabic_CI_AS,
    DiscountPercent   DECIMAL(5,2)    NOT NULL DEFAULT 0,
    PointsMultiplier  DECIMAL(5,2)    NOT NULL DEFAULT 1,
    IsActive          BIT             NOT NULL DEFAULT 1,
    CONSTRAINT PK_CustomerGroups PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT CK_CustomerGroups_Discount CHECK (DiscountPercent BETWEEN 0 AND 100),
    CONSTRAINT CK_CustomerGroups_PointsMult CHECK (PointsMultiplier >= 0)
);
GO

-- ══════════════════════════════════════════════════════════════
-- TABLE: Customers
-- ══════════════════════════════════════════════════════════════
CREATE TABLE dbo.Customers
(
    Id                INT             NOT NULL IDENTITY(1,1),
    Code              NVARCHAR(50)    NOT NULL COLLATE Arabic_CI_AS,
    Name              NVARCHAR(300)   NOT NULL COLLATE Arabic_CI_AS,
    Phone             NVARCHAR(50)    NULL,
    Phone2            NVARCHAR(50)    NULL,
    Address           NVARCHAR(500)   NULL,
    Email             NVARCHAR(200)   NULL,
    InstapayNumber    NVARCHAR(100)   NULL,
    TaxNumber         NVARCHAR(100)   NULL,
    BranchId          INT             NULL,
    GroupId           INT             NULL,
    PriceListId       INT             NULL,
    OpeningBalance    DECIMAL(18,4)   NOT NULL DEFAULT 0,
    CreditLimit       DECIMAL(18,4)   NOT NULL DEFAULT 0,
    PaymentPeriodDays INT             NOT NULL DEFAULT 0,
    CurrentBalance    DECIMAL(18,4)   NOT NULL DEFAULT 0,
    TotalPoints       DECIMAL(18,2)   NOT NULL DEFAULT 0,
    IsActive          BIT             NOT NULL DEFAULT 1,
    IsDeleted         BIT             NOT NULL DEFAULT 0,
    CreatedAt         DATETIME2(0)    NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Customers PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Customers_Code UNIQUE (Code),
    CONSTRAINT FK_Customers_Branches
        FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id),
    CONSTRAINT FK_Customers_Groups
        FOREIGN KEY (GroupId) REFERENCES dbo.CustomerGroups(Id),
    CONSTRAINT FK_Customers_PriceLists
        FOREIGN KEY (PriceListId) REFERENCES dbo.PriceLists(Id),
    CONSTRAINT CK_Customers_CreditLimit CHECK (CreditLimit >= 0),
    CONSTRAINT CK_Customers_PaymentDays CHECK (PaymentPeriodDays >= 0)
);
GO

CREATE NONCLUSTERED INDEX IX_Customers_Name
    ON dbo.Customers (Name) WHERE IsDeleted = 0;
CREATE NONCLUSTERED INDEX IX_Customers_Phone
    ON dbo.Customers (Phone) WHERE Phone IS NOT NULL AND IsDeleted = 0;
CREATE NONCLUSTERED INDEX IX_Customers_Code
    ON dbo.Customers (Code) WHERE IsDeleted = 0;
CREATE NONCLUSTERED INDEX IX_Customers_BranchId
    ON dbo.Customers (BranchId) WHERE BranchId IS NOT NULL AND IsDeleted = 0;
GO

-- ══════════════════════════════════════════════════════════════
-- TABLE: LoyaltyPoints
-- ══════════════════════════════════════════════════════════════
CREATE TABLE dbo.LoyaltyPoints
(
    Id              INT             NOT NULL IDENTITY(1,1),
    CustomerId      INT             NOT NULL,
    TransactionDate DATETIME2(0)    NOT NULL,
    Points          DECIMAL(18,2)   NOT NULL,   -- positive=earn, negative=redeem
    TransactionType TINYINT         NOT NULL,
        -- 0=Earn, 1=Redeem, 2=Adjust, 3=Expire
    ReferenceId     INT             NULL,
    ReferenceType   NVARCHAR(50)    NULL,        -- 'SalesInvoice','Manual'...
    Notes           NVARCHAR(500)   NULL,
    CreatedBy       INT             NULL,
    CONSTRAINT PK_LoyaltyPoints PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_LoyaltyPoints_Customers
        FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_LoyaltyPoints_CreatedBy
        FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_LoyaltyPoints_Type CHECK (TransactionType IN (0,1,2,3))
);
GO

CREATE NONCLUSTERED INDEX IX_LoyaltyPoints_CustomerId
    ON dbo.LoyaltyPoints (CustomerId, TransactionDate DESC);
GO

-- ══════════════════════════════════════════════════════════════
-- TABLE: Coupons
-- ══════════════════════════════════════════════════════════════
CREATE TABLE dbo.Coupons
(
    Id                 INT             NOT NULL IDENTITY(1,1),
    Code               NVARCHAR(50)    NOT NULL COLLATE Arabic_CI_AS,
    CustomerId         INT             NULL,        -- NULL = any customer
    DiscountType       TINYINT         NOT NULL,    -- 0=Percent, 1=FixedAmount
    DiscountValue      DECIMAL(18,4)   NOT NULL,
    MinPurchase        DECIMAL(18,4)   NOT NULL DEFAULT 0,
    ExpiryDate         DATE            NULL,
    IsUsed             BIT             NOT NULL DEFAULT 0,
    UsedDate           DATETIME2(0)    NULL,
    UsedByInvoiceId    INT             NULL,
    IsActive           BIT             NOT NULL DEFAULT 1,
    CreatedAt          DATETIME2(0)    NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Coupons PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Coupons_Code UNIQUE (Code),
    CONSTRAINT FK_Coupons_Customers
        FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(Id) ON DELETE SET NULL,
    CONSTRAINT CK_Coupons_DiscountType CHECK (DiscountType IN (0,1)),
    CONSTRAINT CK_Coupons_DiscountValue CHECK (DiscountValue > 0),
    CONSTRAINT CK_Coupons_MinPurchase CHECK (MinPurchase >= 0)
);
GO

CREATE NONCLUSTERED INDEX IX_Coupons_Code     ON dbo.Coupons (Code) WHERE IsActive = 1;
CREATE NONCLUSTERED INDEX IX_Coupons_Customer ON dbo.Coupons (CustomerId) WHERE CustomerId IS NOT NULL;
GO

-- ══════════════════════════════════════════════════════════════
-- TABLE: Employees
-- ══════════════════════════════════════════════════════════════
CREATE TABLE dbo.Employees
(
    Id          INT             NOT NULL IDENTITY(1,1),
    Code        NVARCHAR(50)    NOT NULL COLLATE Arabic_CI_AS,
    Name        NVARCHAR(300)   NOT NULL COLLATE Arabic_CI_AS,
    JobTitle    NVARCHAR(200)   NULL COLLATE Arabic_CI_AS,
    Phone       NVARCHAR(50)    NULL,
    Address     NVARCHAR(500)   NULL,
    Salary      DECIMAL(18,4)   NOT NULL DEFAULT 0,
    HireDate    DATE            NULL,
    BranchId    INT             NULL,
    IsActive    BIT             NOT NULL DEFAULT 1,
    IsDeleted   BIT             NOT NULL DEFAULT 0,
    CreatedAt   DATETIME2(0)    NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Employees PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Employees_Code UNIQUE (Code),
    CONSTRAINT FK_Employees_Branches
        FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id),
    CONSTRAINT CK_Employees_Salary CHECK (Salary >= 0)
);
GO

CREATE NONCLUSTERED INDEX IX_Employees_Name ON dbo.Employees (Name) WHERE IsDeleted = 0;
GO

-- ══════════════════════════════════════════════════════════════
-- TABLE: EmployeeTransactions (advances, deductions, bonuses)
-- ══════════════════════════════════════════════════════════════
CREATE TABLE dbo.EmployeeTransactions
(
    Id              INT             NOT NULL IDENTITY(1,1),
    EmployeeId      INT             NOT NULL,
    TransactionDate DATE            NOT NULL,
    Type            TINYINT         NOT NULL,   -- 0=Advance, 1=Deduction, 2=Bonus
    Amount          DECIMAL(18,4)   NOT NULL,
    Notes           NVARCHAR(500)   NULL,
    CreatedBy       INT             NULL,
    CreatedAt       DATETIME2(0)    NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_EmployeeTransactions PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_EmpTransactions_Employees
        FOREIGN KEY (EmployeeId) REFERENCES dbo.Employees(Id) ON DELETE CASCADE,
    CONSTRAINT FK_EmpTransactions_CreatedBy
        FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_EmpTransactions_Type CHECK (Type IN (0,1,2)),
    CONSTRAINT CK_EmpTransactions_Amount CHECK (Amount > 0)
);
GO

CREATE NONCLUSTERED INDEX IX_EmpTransactions_EmployeeId
    ON dbo.EmployeeTransactions (EmployeeId, TransactionDate DESC);
GO

-- ══════════════════════════════════════════════════════════════
-- TABLE: DeliveryAgents
-- ══════════════════════════════════════════════════════════════
CREATE TABLE dbo.DeliveryAgents
(
    Id          INT             NOT NULL IDENTITY(1,1),
    Name        NVARCHAR(200)   NOT NULL COLLATE Arabic_CI_AS,
    Phone       NVARCHAR(50)    NULL,
    BranchId    INT             NULL,
    IsActive    BIT             NOT NULL DEFAULT 1,
    CONSTRAINT PK_DeliveryAgents PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_DeliveryAgents_Branches
        FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id)
);
GO

PRINT '✅ Script 04: Group C (People) tables created.';
GO
