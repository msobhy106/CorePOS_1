-- ============================================================
--  CorePOS Enterprise
--  Phase 5 — SQL Script 07: DDL — Groups E, F, G (Part 2), H
--  Group E: PurchaseInvoices, PurchaseInvoiceItems,
--           PurchaseReturns, PurchaseReturnItems
--  Group F: ProductStock, InventoryTransactions,
--           InventorySessions, InventorySessionItems,
--           WarehouseTransfers, WarehouseTransferItems,
--           StockAdjustments, StockAdjustmentItems
--  Group G2: CashBoxTransactions, ExpenseCategories,
--            Expenses, CustomerPayments, SupplierPayments
--  Group H: Settings, Backups
-- ============================================================

USE CorePOS;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- ══════════════════════════════════════════════════════════════
-- GROUP E — PURCHASES
-- ══════════════════════════════════════════════════════════════

-- TABLE: PurchaseInvoices
CREATE TABLE dbo.PurchaseInvoices
(
    Id                  INT             NOT NULL IDENTITY(1,1),
    InvoiceNo           NVARCHAR(50)    NOT NULL COLLATE Arabic_CI_AS,
    SupplierInvoiceNo   NVARCHAR(100)   NULL,
    InvoiceDate         DATETIME2(0)    NOT NULL,
    SupplierId          INT             NULL,
    BranchId            INT             NOT NULL,
    WarehouseId         INT             NOT NULL,
    UserId              INT             NOT NULL,
    Status              TINYINT         NOT NULL DEFAULT 0,
        -- 0=Draft, 1=Approved, 2=Cancelled
    PaymentMethod       TINYINT         NOT NULL DEFAULT 0,
        -- 0=Cash, 1=Visa, 2=BankTransfer, 3=Credit
    Subtotal            DECIMAL(18,4)   NOT NULL DEFAULT 0,
    DiscountPercent     DECIMAL(5,2)    NOT NULL DEFAULT 0,
    DiscountAmount      DECIMAL(18,4)   NOT NULL DEFAULT 0,
    TaxPercent          DECIMAL(5,2)    NOT NULL DEFAULT 0,
    TaxAmount           DECIMAL(18,4)   NOT NULL DEFAULT 0,
    TotalAmount         DECIMAL(18,4)   NOT NULL DEFAULT 0,
    PaidAmount          DECIMAL(18,4)   NOT NULL DEFAULT 0,
    RemainingAmount     DECIMAL(18,4)   NOT NULL DEFAULT 0,
    Notes               NVARCHAR(MAX)   NULL,
    IsDeleted           BIT             NOT NULL DEFAULT 0,
    CreatedAt           DATETIME2(0)    NOT NULL DEFAULT GETUTCDATE(),
    ApprovedAt          DATETIME2(0)    NULL,
    ApprovedBy          INT             NULL,
    CONSTRAINT PK_PurchaseInvoices PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_PurchaseInvoices_InvoiceNo UNIQUE (InvoiceNo),
    CONSTRAINT FK_PurchaseInvoices_Suppliers
        FOREIGN KEY (SupplierId) REFERENCES dbo.Suppliers(Id),
    CONSTRAINT FK_PurchaseInvoices_Branches
        FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id),
    CONSTRAINT FK_PurchaseInvoices_Warehouses
        FOREIGN KEY (WarehouseId) REFERENCES dbo.Warehouses(Id),
    CONSTRAINT FK_PurchaseInvoices_Users
        FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_PurchaseInvoices_ApprovedBy
        FOREIGN KEY (ApprovedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_PurchaseInvoices_Status  CHECK (Status IN (0,1,2)),
    CONSTRAINT CK_PurchaseInvoices_Payment CHECK (PaymentMethod IN (0,1,2,3))
);
GO

CREATE NONCLUSTERED INDEX IX_PurchaseInvoices_SupplierId
    ON dbo.PurchaseInvoices (SupplierId, InvoiceDate DESC)
    WHERE SupplierId IS NOT NULL AND IsDeleted = 0;
CREATE NONCLUSTERED INDEX IX_PurchaseInvoices_InvoiceDate
    ON dbo.PurchaseInvoices (InvoiceDate DESC, BranchId)
    WHERE IsDeleted = 0;
GO

-- TABLE: PurchaseInvoiceItems
CREATE TABLE dbo.PurchaseInvoiceItems
(
    Id               INT             NOT NULL IDENTITY(1,1),
    InvoiceId        INT             NOT NULL,
    ProductId        INT             NOT NULL,
    UnitId           INT             NOT NULL,
    ProductNameAr    NVARCHAR(300)   NOT NULL COLLATE Arabic_CI_AS,
    Quantity         DECIMAL(18,3)   NOT NULL,
    UnitCost         DECIMAL(18,4)   NOT NULL,
    DiscountPercent  DECIMAL(5,2)    NOT NULL DEFAULT 0,
    DiscountAmount   DECIMAL(18,4)   NOT NULL DEFAULT 0,
    TaxPercent       DECIMAL(5,2)    NOT NULL DEFAULT 0,
    TaxAmount        DECIMAL(18,4)   NOT NULL DEFAULT 0,
    TotalCost        DECIMAL(18,4)   NOT NULL,
    SalePriceAfter   DECIMAL(18,4)   NULL,       -- optional: override sale price
    ReturnedQty      DECIMAL(18,3)   NOT NULL DEFAULT 0,
    CONSTRAINT PK_PurchaseInvoiceItems PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_PurchaseInvoiceItems_Invoices
        FOREIGN KEY (InvoiceId) REFERENCES dbo.PurchaseInvoices(Id) ON DELETE CASCADE,
    CONSTRAINT FK_PurchaseInvoiceItems_Products
        FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
    CONSTRAINT FK_PurchaseInvoiceItems_Units
        FOREIGN KEY (UnitId) REFERENCES dbo.Units(Id),
    CONSTRAINT CK_PurchInvItems_Qty     CHECK (Quantity > 0),
    CONSTRAINT CK_PurchInvItems_Cost    CHECK (UnitCost >= 0)
);
GO

CREATE NONCLUSTERED INDEX IX_PurchaseInvoiceItems_InvoiceId
    ON dbo.PurchaseInvoiceItems (InvoiceId);
GO

-- TABLE: PurchaseReturns
CREATE TABLE dbo.PurchaseReturns
(
    Id                  INT             NOT NULL IDENTITY(1,1),
    ReturnNo            NVARCHAR(50)    NOT NULL COLLATE Arabic_CI_AS,
    ReturnDate          DATETIME2(0)    NOT NULL,
    OriginalInvoiceId   INT             NOT NULL,
    SupplierId          INT             NULL,
    BranchId            INT             NOT NULL,
    WarehouseId         INT             NOT NULL,
    UserId              INT             NOT NULL,
    ReturnType          TINYINT         NOT NULL,    -- 0=Full, 1=Partial
    TotalAmount         DECIMAL(18,4)   NOT NULL DEFAULT 0,
    Notes               NVARCHAR(MAX)   NULL,
    CreatedAt           DATETIME2(0)    NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_PurchaseReturns PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_PurchaseReturns_ReturnNo UNIQUE (ReturnNo),
    CONSTRAINT FK_PurchaseReturns_OrigInvoice
        FOREIGN KEY (OriginalInvoiceId) REFERENCES dbo.PurchaseInvoices(Id),
    CONSTRAINT FK_PurchaseReturns_Suppliers
        FOREIGN KEY (SupplierId) REFERENCES dbo.Suppliers(Id),
    CONSTRAINT FK_PurchaseReturns_Branches
        FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id),
    CONSTRAINT FK_PurchaseReturns_Warehouses
        FOREIGN KEY (WarehouseId) REFERENCES dbo.Warehouses(Id),
    CONSTRAINT FK_PurchaseReturns_Users
        FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_PurchaseReturns_Type CHECK (ReturnType IN (0,1))
);
GO

-- TABLE: PurchaseReturnItems
CREATE TABLE dbo.PurchaseReturnItems
(
    Id              INT             NOT NULL IDENTITY(1,1),
    ReturnId        INT             NOT NULL,
    InvoiceItemId   INT             NOT NULL,
    ProductId       INT             NOT NULL,
    UnitId          INT             NOT NULL,
    ProductNameAr   NVARCHAR(300)   NOT NULL COLLATE Arabic_CI_AS,
    Quantity        DECIMAL(18,3)   NOT NULL,
    UnitCost        DECIMAL(18,4)   NOT NULL,
    TotalCost       DECIMAL(18,4)   NOT NULL,
    CONSTRAINT PK_PurchaseReturnItems PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_PurchReturnItems_Returns
        FOREIGN KEY (ReturnId) REFERENCES dbo.PurchaseReturns(Id) ON DELETE CASCADE,
    CONSTRAINT FK_PurchReturnItems_InvoiceItems
        FOREIGN KEY (InvoiceItemId) REFERENCES dbo.PurchaseInvoiceItems(Id),
    CONSTRAINT FK_PurchReturnItems_Products
        FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
    CONSTRAINT FK_PurchReturnItems_Units
        FOREIGN KEY (UnitId) REFERENCES dbo.Units(Id)
);
GO

-- ══════════════════════════════════════════════════════════════
-- GROUP F — INVENTORY
-- ══════════════════════════════════════════════════════════════

-- TABLE: ProductStock (current balance snapshot)
CREATE TABLE dbo.ProductStock
(
    Id           INT             NOT NULL IDENTITY(1,1),
    ProductId    INT             NOT NULL,
    WarehouseId  INT             NOT NULL,
    Quantity     DECIMAL(18,3)   NOT NULL DEFAULT 0,
    AverageCost  DECIMAL(18,4)   NOT NULL DEFAULT 0,
    LastCost     DECIMAL(18,4)   NOT NULL DEFAULT 0,
    LastUpdated  DATETIME2(0)    NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_ProductStock PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_ProductStock UNIQUE (ProductId, WarehouseId),
    CONSTRAINT FK_ProductStock_Products
        FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ProductStock_Warehouses
        FOREIGN KEY (WarehouseId) REFERENCES dbo.Warehouses(Id)
);
GO

CREATE NONCLUSTERED INDEX IX_ProductStock_ProductId
    ON dbo.ProductStock (ProductId) INCLUDE (Quantity, WarehouseId, AverageCost);
CREATE NONCLUSTERED INDEX IX_ProductStock_WarehouseId
    ON dbo.ProductStock (WarehouseId) INCLUDE (ProductId, Quantity);
GO

-- TABLE: InventoryTransactions (full movement ledger)
CREATE TABLE dbo.InventoryTransactions
(
    Id              BIGINT          NOT NULL IDENTITY(1,1),
    ProductId       INT             NOT NULL,
    WarehouseId     INT             NOT NULL,
    TransactionDate DATETIME2(0)    NOT NULL,
    TransactionType TINYINT         NOT NULL,
        -- 0=Opening, 1=SaleOut, 2=SaleReturnIn,
        -- 3=PurchaseIn, 4=PurchaseReturnOut,
        -- 5=TransferOut, 6=TransferIn,
        -- 7=AdjustmentPlus, 8=AdjustmentMinus,
        -- 9=InventoryCountAdjust
    Quantity        DECIMAL(18,3)   NOT NULL,
    Direction       TINYINT         NOT NULL,       -- 0=In, 1=Out
    UnitCost        DECIMAL(18,4)   NOT NULL DEFAULT 0,
    TotalCost       DECIMAL(18,4)   NOT NULL DEFAULT 0,
    BalanceAfter    DECIMAL(18,3)   NOT NULL DEFAULT 0,
    ReferenceId     INT             NULL,
    ReferenceType   NVARCHAR(50)    NULL,
    Notes           NVARCHAR(500)   NULL,
    UserId          INT             NULL,
    CreatedAt       DATETIME2(0)    NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_InventoryTransactions PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_InvTrans_Products
        FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
    CONSTRAINT FK_InvTrans_Warehouses
        FOREIGN KEY (WarehouseId) REFERENCES dbo.Warehouses(Id),
    CONSTRAINT FK_InvTrans_Users
        FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_InvTrans_Type
        CHECK (TransactionType BETWEEN 0 AND 9),
    CONSTRAINT CK_InvTrans_Direction CHECK (Direction IN (0,1)),
    CONSTRAINT CK_InvTrans_Quantity   CHECK (Quantity > 0)
);
GO

CREATE NONCLUSTERED INDEX IX_InvTrans_ProductWarehouse
    ON dbo.InventoryTransactions (ProductId, WarehouseId, TransactionDate DESC);
CREATE NONCLUSTERED INDEX IX_InvTrans_Date
    ON dbo.InventoryTransactions (TransactionDate DESC);
CREATE NONCLUSTERED INDEX IX_InvTrans_Type
    ON dbo.InventoryTransactions (TransactionType, TransactionDate DESC);
GO

-- TABLE: InventorySessions
CREATE TABLE dbo.InventorySessions
(
    Id          INT             NOT NULL IDENTITY(1,1),
    SessionNo   NVARCHAR(50)    NOT NULL COLLATE Arabic_CI_AS,
    SessionDate DATETIME2(0)    NOT NULL,
    WarehouseId INT             NOT NULL,
    CountType   TINYINT         NOT NULL,    -- 0=Full, 1=Partial
    Status      TINYINT         NOT NULL DEFAULT 0,
        -- 0=Open, 1=Approved, 2=Cancelled
    UserId      INT             NOT NULL,
    ApprovedBy  INT             NULL,
    ApprovedAt  DATETIME2(0)    NULL,
    Notes       NVARCHAR(MAX)   NULL,
    CreatedAt   DATETIME2(0)    NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_InventorySessions PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_InventorySessions_SessionNo UNIQUE (SessionNo),
    CONSTRAINT FK_InvSessions_Warehouses
        FOREIGN KEY (WarehouseId) REFERENCES dbo.Warehouses(Id),
    CONSTRAINT FK_InvSessions_Users
        FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_InvSessions_ApprovedBy
        FOREIGN KEY (ApprovedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_InvSessions_Type   CHECK (CountType IN (0,1)),
    CONSTRAINT CK_InvSessions_Status CHECK (Status IN (0,1,2))
);
GO

-- TABLE: InventorySessionItems
CREATE TABLE dbo.InventorySessionItems
(
    Id             INT             NOT NULL IDENTITY(1,1),
    SessionId      INT             NOT NULL,
    ProductId      INT             NOT NULL,
    SystemQuantity DECIMAL(18,3)   NOT NULL DEFAULT 0,
    ActualQuantity DECIMAL(18,3)   NOT NULL DEFAULT 0,
    Difference     AS (ActualQuantity - SystemQuantity),  -- computed column
    UnitCost       DECIMAL(18,4)   NOT NULL DEFAULT 0,
    Notes          NVARCHAR(500)   NULL,
    CONSTRAINT PK_InventorySessionItems PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_InvSessionItems_Sessions
        FOREIGN KEY (SessionId) REFERENCES dbo.InventorySessions(Id) ON DELETE CASCADE,
    CONSTRAINT FK_InvSessionItems_Products
        FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
    CONSTRAINT CK_InvSessionItems_ActualQty CHECK (ActualQuantity >= 0)
);
GO

CREATE NONCLUSTERED INDEX IX_InvSessionItems_SessionId
    ON dbo.InventorySessionItems (SessionId);
GO

-- TABLE: WarehouseTransfers
CREATE TABLE dbo.WarehouseTransfers
(
    Id                INT             NOT NULL IDENTITY(1,1),
    TransferNo        NVARCHAR(50)    NOT NULL COLLATE Arabic_CI_AS,
    TransferDate      DATETIME2(0)    NOT NULL,
    FromWarehouseId   INT             NOT NULL,
    ToWarehouseId     INT             NOT NULL,
    FromBranchId      INT             NOT NULL,
    ToBranchId        INT             NOT NULL,
    Status            TINYINT         NOT NULL DEFAULT 0,
        -- 0=Draft, 1=Approved, 2=Cancelled
    UserId            INT             NOT NULL,
    Notes             NVARCHAR(MAX)   NULL,
    CreatedAt         DATETIME2(0)    NOT NULL DEFAULT GETUTCDATE(),
    ApprovedAt        DATETIME2(0)    NULL,
    CONSTRAINT PK_WarehouseTransfers PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_WarehouseTransfers_No UNIQUE (TransferNo),
    CONSTRAINT FK_Transfers_FromWarehouse
        FOREIGN KEY (FromWarehouseId) REFERENCES dbo.Warehouses(Id),
    CONSTRAINT FK_Transfers_ToWarehouse
        FOREIGN KEY (ToWarehouseId) REFERENCES dbo.Warehouses(Id),
    CONSTRAINT FK_Transfers_FromBranch
        FOREIGN KEY (FromBranchId) REFERENCES dbo.Branches(Id),
    CONSTRAINT FK_Transfers_ToBranch
        FOREIGN KEY (ToBranchId) REFERENCES dbo.Branches(Id),
    CONSTRAINT FK_Transfers_Users
        FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_Transfers_Status CHECK (Status IN (0,1,2)),
    CONSTRAINT CK_Transfers_DiffWarehouses
        CHECK (FromWarehouseId <> ToWarehouseId)
);
GO

-- TABLE: WarehouseTransferItems
CREATE TABLE dbo.WarehouseTransferItems
(
    Id            INT             NOT NULL IDENTITY(1,1),
    TransferId    INT             NOT NULL,
    ProductId     INT             NOT NULL,
    ProductNameAr NVARCHAR(300)   NOT NULL COLLATE Arabic_CI_AS,
    Quantity      DECIMAL(18,3)   NOT NULL,
    UnitCost      DECIMAL(18,4)   NOT NULL DEFAULT 0,
    CONSTRAINT PK_WarehouseTransferItems PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_TransferItems_Transfers
        FOREIGN KEY (TransferId) REFERENCES dbo.WarehouseTransfers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_TransferItems_Products
        FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
    CONSTRAINT CK_TransferItems_Qty CHECK (Quantity > 0)
);
GO

-- TABLE: StockAdjustments
CREATE TABLE dbo.StockAdjustments
(
    Id              INT             NOT NULL IDENTITY(1,1),
    AdjustmentNo    NVARCHAR(50)    NOT NULL COLLATE Arabic_CI_AS,
    AdjustmentDate  DATETIME2(0)    NOT NULL,
    WarehouseId     INT             NOT NULL,
    Type            TINYINT         NOT NULL,    -- 0=Increase, 1=Decrease
    UserId          INT             NOT NULL,
    Notes           NVARCHAR(MAX)   NULL,
    CreatedAt       DATETIME2(0)    NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_StockAdjustments PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_StockAdjustments_No UNIQUE (AdjustmentNo),
    CONSTRAINT FK_StockAdj_Warehouses
        FOREIGN KEY (WarehouseId) REFERENCES dbo.Warehouses(Id),
    CONSTRAINT FK_StockAdj_Users
        FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_StockAdj_Type CHECK (Type IN (0,1))
);
GO

-- TABLE: StockAdjustmentItems
CREATE TABLE dbo.StockAdjustmentItems
(
    Id            INT             NOT NULL IDENTITY(1,1),
    AdjustmentId  INT             NOT NULL,
    ProductId     INT             NOT NULL,
    ProductNameAr NVARCHAR(300)   NOT NULL COLLATE Arabic_CI_AS,
    Quantity      DECIMAL(18,3)   NOT NULL,
    UnitCost      DECIMAL(18,4)   NOT NULL DEFAULT 0,
    Reason        NVARCHAR(500)   NULL,
    CONSTRAINT PK_StockAdjustmentItems PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_StockAdjItems_Adjustments
        FOREIGN KEY (AdjustmentId) REFERENCES dbo.StockAdjustments(Id) ON DELETE CASCADE,
    CONSTRAINT FK_StockAdjItems_Products
        FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id),
    CONSTRAINT CK_StockAdjItems_Qty CHECK (Quantity > 0)
);
GO

-- ══════════════════════════════════════════════════════════════
-- GROUP G (Part 2) — FINANCE
-- ══════════════════════════════════════════════════════════════

-- TABLE: CashBoxTransactions
CREATE TABLE dbo.CashBoxTransactions
(
    Id              BIGINT          NOT NULL IDENTITY(1,1),
    CashBoxId       INT             NOT NULL,
    ShiftId         INT             NULL,
    TransactionDate DATETIME2(0)    NOT NULL,
    TransactionType TINYINT         NOT NULL,
        -- 0=OpenShift, 1=CloseShift, 2=Sale, 3=SaleReturn,
        -- 4=Purchase, 5=PurchaseReturn, 6=Deposit, 7=Withdraw,
        -- 8=Transfer, 9=Expense, 10=CustomerPayment, 11=SupplierPayment
    Direction       TINYINT         NOT NULL,   -- 0=In, 1=Out
    Amount          DECIMAL(18,4)   NOT NULL,
    BalanceAfter    DECIMAL(18,4)   NOT NULL,
    ReferenceId     INT             NULL,
    ReferenceType   NVARCHAR(50)    NULL,
    Description     NVARCHAR(500)   NULL,
    UserId          INT             NULL,
    CONSTRAINT PK_CashBoxTransactions PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_CBTrans_CashBoxes
        FOREIGN KEY (CashBoxId) REFERENCES dbo.CashBoxes(Id),
    CONSTRAINT FK_CBTrans_Shifts
        FOREIGN KEY (ShiftId) REFERENCES dbo.Shifts(Id),
    CONSTRAINT FK_CBTrans_Users
        FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_CBTrans_Type      CHECK (TransactionType BETWEEN 0 AND 11),
    CONSTRAINT CK_CBTrans_Direction CHECK (Direction IN (0,1)),
    CONSTRAINT CK_CBTrans_Amount    CHECK (Amount > 0)
);
GO

CREATE NONCLUSTERED INDEX IX_CBTrans_CashBoxId
    ON dbo.CashBoxTransactions (CashBoxId, TransactionDate DESC);
CREATE NONCLUSTERED INDEX IX_CBTrans_ShiftId
    ON dbo.CashBoxTransactions (ShiftId) WHERE ShiftId IS NOT NULL;
CREATE NONCLUSTERED INDEX IX_CBTrans_Date
    ON dbo.CashBoxTransactions (TransactionDate DESC);
GO

-- TABLE: ExpenseCategories
CREATE TABLE dbo.ExpenseCategories
(
    Id       INT             NOT NULL IDENTITY(1,1),
    Name     NVARCHAR(200)   NOT NULL COLLATE Arabic_CI_AS,
    NameAr   NVARCHAR(200)   NOT NULL COLLATE Arabic_CI_AS,
    IsSystem BIT             NOT NULL DEFAULT 0,   -- seed categories can't be deleted
    IsActive BIT             NOT NULL DEFAULT 1,
    CONSTRAINT PK_ExpenseCategories PRIMARY KEY CLUSTERED (Id)
);
GO

-- TABLE: Expenses
CREATE TABLE dbo.Expenses
(
    Id          INT             NOT NULL IDENTITY(1,1),
    ExpenseNo   NVARCHAR(50)    NOT NULL COLLATE Arabic_CI_AS,
    ExpenseDate DATE            NOT NULL,
    CategoryId  INT             NOT NULL,
    BranchId    INT             NOT NULL,
    CashBoxId   INT             NULL,
    ShiftId     INT             NULL,
    UserId      INT             NOT NULL,
    Amount      DECIMAL(18,4)   NOT NULL,
    Description NVARCHAR(500)   NULL,
    CreatedAt   DATETIME2(0)    NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Expenses PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Expenses_ExpenseNo UNIQUE (ExpenseNo),
    CONSTRAINT FK_Expenses_Categories
        FOREIGN KEY (CategoryId) REFERENCES dbo.ExpenseCategories(Id),
    CONSTRAINT FK_Expenses_Branches
        FOREIGN KEY (BranchId) REFERENCES dbo.Branches(Id),
    CONSTRAINT FK_Expenses_CashBoxes
        FOREIGN KEY (CashBoxId) REFERENCES dbo.CashBoxes(Id),
    CONSTRAINT FK_Expenses_Shifts
        FOREIGN KEY (ShiftId) REFERENCES dbo.Shifts(Id),
    CONSTRAINT FK_Expenses_Users
        FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_Expenses_Amount CHECK (Amount > 0)
);
GO

CREATE NONCLUSTERED INDEX IX_Expenses_Date
    ON dbo.Expenses (ExpenseDate DESC, BranchId);
CREATE NONCLUSTERED INDEX IX_Expenses_CategoryId
    ON dbo.Expenses (CategoryId);
GO

-- TABLE: CustomerPayments
CREATE TABLE dbo.CustomerPayments
(
    Id            INT             NOT NULL IDENTITY(1,1),
    PaymentNo     NVARCHAR(50)    NOT NULL COLLATE Arabic_CI_AS,
    PaymentDate   DATE            NOT NULL,
    CustomerId    INT             NOT NULL,
    CashBoxId     INT             NULL,
    ShiftId       INT             NULL,
    UserId        INT             NOT NULL,
    PaymentMethod TINYINT         NOT NULL DEFAULT 0,
    Amount        DECIMAL(18,4)   NOT NULL,
    Notes         NVARCHAR(500)   NULL,
    CreatedAt     DATETIME2(0)    NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_CustomerPayments PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_CustomerPayments_No UNIQUE (PaymentNo),
    CONSTRAINT FK_CustPayments_Customers
        FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(Id),
    CONSTRAINT FK_CustPayments_CashBoxes
        FOREIGN KEY (CashBoxId) REFERENCES dbo.CashBoxes(Id),
    CONSTRAINT FK_CustPayments_Shifts
        FOREIGN KEY (ShiftId) REFERENCES dbo.Shifts(Id),
    CONSTRAINT FK_CustPayments_Users
        FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_CustPayments_Amount CHECK (Amount > 0)
);
GO

CREATE NONCLUSTERED INDEX IX_CustPayments_CustomerId
    ON dbo.CustomerPayments (CustomerId, PaymentDate DESC);
GO

-- TABLE: SupplierPayments
CREATE TABLE dbo.SupplierPayments
(
    Id            INT             NOT NULL IDENTITY(1,1),
    PaymentNo     NVARCHAR(50)    NOT NULL COLLATE Arabic_CI_AS,
    PaymentDate   DATE            NOT NULL,
    SupplierId    INT             NOT NULL,
    CashBoxId     INT             NULL,
    ShiftId       INT             NULL,
    UserId        INT             NOT NULL,
    PaymentMethod TINYINT         NOT NULL DEFAULT 0,
    Amount        DECIMAL(18,4)   NOT NULL,
    Notes         NVARCHAR(500)   NULL,
    CreatedAt     DATETIME2(0)    NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_SupplierPayments PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_SupplierPayments_No UNIQUE (PaymentNo),
    CONSTRAINT FK_SuppPayments_Suppliers
        FOREIGN KEY (SupplierId) REFERENCES dbo.Suppliers(Id),
    CONSTRAINT FK_SuppPayments_CashBoxes
        FOREIGN KEY (CashBoxId) REFERENCES dbo.CashBoxes(Id),
    CONSTRAINT FK_SuppPayments_Shifts
        FOREIGN KEY (ShiftId) REFERENCES dbo.Shifts(Id),
    CONSTRAINT FK_SuppPayments_Users
        FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_SuppPayments_Amount CHECK (Amount > 0)
);
GO

CREATE NONCLUSTERED INDEX IX_SuppPayments_SupplierId
    ON dbo.SupplierPayments (SupplierId, PaymentDate DESC);
GO

-- ══════════════════════════════════════════════════════════════
-- GROUP H — SETTINGS
-- ══════════════════════════════════════════════════════════════

-- TABLE: Settings (key-value store)
CREATE TABLE dbo.Settings
(
    Id           INT             NOT NULL IDENTITY(1,1),
    SettingKey   NVARCHAR(200)   NOT NULL COLLATE Arabic_CI_AS,
    SettingValue NVARCHAR(MAX)   NULL,
    SettingGroup NVARCHAR(100)   NULL,
    DataType     NVARCHAR(50)    NULL,     -- 'string','int','bool','decimal'
    Description  NVARCHAR(500)   NULL,
    CONSTRAINT PK_Settings PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Settings_Key UNIQUE (SettingKey)
);
GO

-- TABLE: Backups
CREATE TABLE dbo.Backups
(
    Id             INT             NOT NULL IDENTITY(1,1),
    FileName       NVARCHAR(500)   NOT NULL,
    FilePath       NVARCHAR(1000)  NOT NULL,
    BackupType     TINYINT         NOT NULL,    -- 0=Manual, 1=Daily, 2=Weekly, 3=Monthly
    FileSizeBytes  BIGINT          NOT NULL DEFAULT 0,
    IsSuccessful   BIT             NOT NULL DEFAULT 1,
    ErrorMessage   NVARCHAR(MAX)   NULL,
    CreatedBy      INT             NULL,
    CreatedAt      DATETIME2(0)    NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Backups PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Backups_Users
        FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_Backups_Type CHECK (BackupType IN (0,1,2,3))
);
GO

CREATE NONCLUSTERED INDEX IX_Backups_CreatedAt
    ON dbo.Backups (CreatedAt DESC);
GO

PRINT '✅ Script 07: Groups E, F, G(Part2), H tables created.';
GO
