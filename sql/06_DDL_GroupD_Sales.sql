-- ============================================================
--  CorePOS Enterprise
--  Phase 5 — SQL Script 06: DDL — Group D: Sales
--  Tables: SalesInvoices, SalesInvoiceItems,
--          SalesReturns, SalesReturnItems
-- ============================================================

USE CorePOS;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- ══════════════════════════════════════════════════════════════
-- TABLE: SalesInvoices
-- ══════════════════════════════════════════════════════════════
CREATE TABLE dbo.SalesInvoices
(
    Id                    INT             NOT NULL IDENTITY(1,1),
    InvoiceNo             NVARCHAR(50)    NOT NULL COLLATE Arabic_CI_AS,
    InvoiceDate           DATETIME2(0)    NOT NULL,
    CustomerId            INT             NULL,
    BranchId              INT             NOT NULL,
    WarehouseId           INT             NOT NULL,
    UserId                INT             NOT NULL,
    ShiftId               INT             NULL,
    InvoiceType           TINYINT         NOT NULL DEFAULT 0,
        -- 0=Retail, 1=Wholesale, 2=HalfWholesale, 3=Special
    PaymentMethod         TINYINT         NOT NULL DEFAULT 0,
        -- 0=Cash, 1=Visa, 2=BankTransfer, 3=EWallet, 4=Credit, 5=Mixed
    Status                TINYINT         NOT NULL DEFAULT 2,
        -- 0=Draft, 1=Held, 2=Completed, 3=Cancelled, 4=PartialReturn, 5=FullReturn
    Subtotal              DECIMAL(18,4)   NOT NULL DEFAULT 0,
    DiscountPercent       DECIMAL(5,2)    NOT NULL DEFAULT 0,
    DiscountAmount        DECIMAL(18,4)   NOT NULL DEFAULT 0,
    TaxPercent            DECIMAL(5,2)    NOT NULL DEFAULT 0,
    TaxAmount             DECIMAL(18,4)   NOT NULL DEFAULT 0,
    DeliveryCost          DECIMAL(18,4)   NOT NULL DEFAULT 0,
    DeliveryAgentId       INT             NULL,
    TotalAmount           DECIMAL(18,4)   NOT NULL DEFAULT 0,
    PaidAmount            DECIMAL(18,4)   NOT NULL DEFAULT 0,
    VisaAmount            DECIMAL(18,4)   NOT NULL DEFAULT 0,
    BankTransferAmount    DECIMAL(18,4)   NOT NULL DEFAULT 0,
    EWalletAmount         DECIMAL(18,4)   NOT NULL DEFAULT 0,
    RemainingAmount       DECIMAL(18,4)   NOT NULL DEFAULT 0,
    Notes                 NVARCHAR(MAX)   NULL,
    IsDeleted             BIT             NOT NULL DEFAULT 0,
    CreatedAt             DATETIME2(0)    NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt             DATETIME2(0)    NULL,
    CONSTRAINT PK_SalesInvoices PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_SalesInvoices_InvoiceNo UNIQUE (InvoiceNo),
    CONSTRAINT FK_SalesInvoices_Customers
        FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(Id),
    CONSTRAINT FK_SalesInvoices_Branches
        FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id),
    CONSTRAINT FK_SalesInvoices_Warehouses
        FOREIGN KEY (WarehouseId) REFERENCES dbo.Warehouses(Id),
    CONSTRAINT FK_SalesInvoices_Users
        FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_SalesInvoices_Shifts
        FOREIGN KEY (ShiftId) REFERENCES dbo.Shifts(Id),
    CONSTRAINT FK_SalesInvoices_DeliveryAgent
        FOREIGN KEY (DeliveryAgentId) REFERENCES dbo.DeliveryAgents(Id),
    CONSTRAINT CK_SalesInvoices_InvoiceType
        CHECK (InvoiceType IN (0,1,2,3)),
    CONSTRAINT CK_SalesInvoices_PaymentMethod
        CHECK (PaymentMethod IN (0,1,2,3,4,5)),
    CONSTRAINT CK_SalesInvoices_Status
        CHECK (Status IN (0,1,2,3,4,5)),
    CONSTRAINT CK_SalesInvoices_Amounts
        CHECK (Subtotal >= 0 AND TotalAmount >= 0 AND PaidAmount >= 0),
    CONSTRAINT CK_SalesInvoices_DiscountPercent
        CHECK (DiscountPercent BETWEEN 0 AND 100),
    CONSTRAINT CK_SalesInvoices_TaxPercent
        CHECK (TaxPercent BETWEEN 0 AND 100)
);
GO

-- Critical performance indexes for 500K+ invoices
CREATE NONCLUSTERED INDEX IX_SalesInvoices_InvoiceDate
    ON dbo.SalesInvoices (InvoiceDate DESC)
    INCLUDE (InvoiceNo, CustomerId, TotalAmount, Status)
    WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_SalesInvoices_CustomerId
    ON dbo.SalesInvoices (CustomerId, InvoiceDate DESC)
    WHERE CustomerId IS NOT NULL AND IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_SalesInvoices_BranchDate
    ON dbo.SalesInvoices (BranchId, InvoiceDate DESC)
    WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_SalesInvoices_ShiftId
    ON dbo.SalesInvoices (ShiftId)
    WHERE ShiftId IS NOT NULL;

CREATE NONCLUSTERED INDEX IX_SalesInvoices_Status
    ON dbo.SalesInvoices (Status, InvoiceDate DESC)
    WHERE IsDeleted = 0;
GO

-- ══════════════════════════════════════════════════════════════
-- TABLE: SalesInvoiceItems
-- ══════════════════════════════════════════════════════════════
CREATE TABLE dbo.SalesInvoiceItems
(
    Id                   INT             NOT NULL IDENTITY(1,1),
    InvoiceId            INT             NOT NULL,
    ProductId            INT             NOT NULL,
    UnitId               INT             NOT NULL,
    Barcode              NVARCHAR(100)   NULL,       -- snapshot at time of sale
    ProductNameAr        NVARCHAR(300)   NOT NULL COLLATE Arabic_CI_AS,
    Quantity             DECIMAL(18,3)   NOT NULL,
    UnitPrice            DECIMAL(18,4)   NOT NULL,
    PurchasePrice        DECIMAL(18,4)   NOT NULL DEFAULT 0,  -- for profit calculation
    DiscountPercent      DECIMAL(5,2)    NOT NULL DEFAULT 0,
    DiscountAmount       DECIMAL(18,4)   NOT NULL DEFAULT 0,
    TaxPercent           DECIMAL(5,2)    NOT NULL DEFAULT 0,
    TaxAmount            DECIMAL(18,4)   NOT NULL DEFAULT 0,
    TotalPrice           DECIMAL(18,4)   NOT NULL,
    ReturnedQty          DECIMAL(18,3)   NOT NULL DEFAULT 0,
    SortOrder            INT             NOT NULL DEFAULT 0,
    CONSTRAINT PK_SalesInvoiceItems PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_SalesInvoiceItems_Invoices
        FOREIGN KEY (InvoiceId) REFERENCES dbo.SalesInvoices(Id) ON DELETE CASCADE,
    CONSTRAINT FK_SalesInvoiceItems_Products
        FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
    CONSTRAINT FK_SalesInvoiceItems_Units
        FOREIGN KEY (UnitId) REFERENCES dbo.Units(Id),
    CONSTRAINT CK_SalesInvItems_Quantity    CHECK (Quantity > 0),
    CONSTRAINT CK_SalesInvItems_UnitPrice   CHECK (UnitPrice >= 0),
    CONSTRAINT CK_SalesInvItems_TotalPrice  CHECK (TotalPrice >= 0),
    CONSTRAINT CK_SalesInvItems_ReturnedQty CHECK (ReturnedQty >= 0),
    CONSTRAINT CK_SalesInvItems_DiscountPct CHECK (DiscountPercent BETWEEN 0 AND 100)
);
GO

CREATE NONCLUSTERED INDEX IX_SalesInvoiceItems_InvoiceId
    ON dbo.SalesInvoiceItems (InvoiceId);

CREATE NONCLUSTERED INDEX IX_SalesInvoiceItems_ProductId
    ON dbo.SalesInvoiceItems (ProductId, InvoiceId);
GO

-- ══════════════════════════════════════════════════════════════
-- TABLE: SalesReturns
-- ══════════════════════════════════════════════════════════════
CREATE TABLE dbo.SalesReturns
(
    Id                  INT             NOT NULL IDENTITY(1,1),
    ReturnNo            NVARCHAR(50)    NOT NULL COLLATE Arabic_CI_AS,
    ReturnDate          DATETIME2(0)    NOT NULL,
    OriginalInvoiceId   INT             NOT NULL,
    CustomerId          INT             NULL,
    BranchId            INT             NOT NULL,
    WarehouseId         INT             NOT NULL,
    UserId              INT             NOT NULL,
    ShiftId             INT             NULL,
    ReturnType          TINYINT         NOT NULL,    -- 0=Full, 1=Partial
    RefundMethod        TINYINT         NOT NULL,    -- 0=Cash, 1=Credit, 2=Exchange
    TotalAmount         DECIMAL(18,4)   NOT NULL DEFAULT 0,
    Notes               NVARCHAR(MAX)   NULL,
    CreatedAt           DATETIME2(0)    NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_SalesReturns PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_SalesReturns_ReturnNo UNIQUE (ReturnNo),
    CONSTRAINT FK_SalesReturns_OriginalInvoice
        FOREIGN KEY (OriginalInvoiceId) REFERENCES dbo.SalesInvoices(Id),
    CONSTRAINT FK_SalesReturns_Customers
        FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(Id),
    CONSTRAINT FK_SalesReturns_Branches
        FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id),
    CONSTRAINT FK_SalesReturns_Warehouses
        FOREIGN KEY (WarehouseId) REFERENCES dbo.Warehouses(Id),
    CONSTRAINT FK_SalesReturns_Users
        FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_SalesReturns_Shifts
        FOREIGN KEY (ShiftId) REFERENCES dbo.Shifts(Id),
    CONSTRAINT CK_SalesReturns_ReturnType   CHECK (ReturnType IN (0,1)),
    CONSTRAINT CK_SalesReturns_RefundMethod CHECK (RefundMethod IN (0,1,2))
);
GO

CREATE NONCLUSTERED INDEX IX_SalesReturns_OriginalInvoice
    ON dbo.SalesReturns (OriginalInvoiceId);
CREATE NONCLUSTERED INDEX IX_SalesReturns_ReturnDate
    ON dbo.SalesReturns (ReturnDate DESC);
GO

-- ══════════════════════════════════════════════════════════════
-- TABLE: SalesReturnItems
-- ══════════════════════════════════════════════════════════════
CREATE TABLE dbo.SalesReturnItems
(
    Id              INT             NOT NULL IDENTITY(1,1),
    ReturnId        INT             NOT NULL,
    InvoiceItemId   INT             NOT NULL,
    ProductId       INT             NOT NULL,
    UnitId          INT             NOT NULL,
    ProductNameAr   NVARCHAR(300)   NOT NULL COLLATE Arabic_CI_AS,
    Quantity        DECIMAL(18,3)   NOT NULL,
    UnitPrice       DECIMAL(18,4)   NOT NULL,
    TotalPrice      DECIMAL(18,4)   NOT NULL,
    CONSTRAINT PK_SalesReturnItems PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_SalesReturnItems_Returns
        FOREIGN KEY (ReturnId) REFERENCES dbo.SalesReturns(Id) ON DELETE CASCADE,
    CONSTRAINT FK_SalesReturnItems_InvoiceItems
        FOREIGN KEY (InvoiceItemId) REFERENCES dbo.SalesInvoiceItems(Id),
    CONSTRAINT FK_SalesReturnItems_Products
        FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
    CONSTRAINT FK_SalesReturnItems_Units
        FOREIGN KEY (UnitId) REFERENCES dbo.Units(Id),
    CONSTRAINT CK_SalesReturnItems_Qty CHECK (Quantity > 0)
);
GO

CREATE NONCLUSTERED INDEX IX_SalesReturnItems_ReturnId
    ON dbo.SalesReturnItems (ReturnId);
GO

PRINT '✅ Script 06: Group D (Sales) tables created.';
GO
