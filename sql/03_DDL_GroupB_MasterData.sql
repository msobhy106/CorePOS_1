-- ============================================================
--  CorePOS Enterprise
--  Phase 5 — SQL Script 03: DDL — Group B: Master Data
--  Tables: Categories, Units, Suppliers(stub), Products,
--          ProductImages, ProductUnits, PriceLists, ProductPrices
-- ============================================================

USE CorePOS;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- ══════════════════════════════════════════════════════════════
-- TABLE: Categories (hierarchical: main + sub)
-- ══════════════════════════════════════════════════════════════
CREATE TABLE dbo.Categories
(
    Id          INT             NOT NULL IDENTITY(1,1),
    Code        NVARCHAR(20)    NOT NULL COLLATE Arabic_CI_AS,
    Name        NVARCHAR(200)   NOT NULL COLLATE Arabic_CI_AS,
    NameAr      NVARCHAR(200)   NOT NULL COLLATE Arabic_CI_AS,
    ParentId    INT             NULL,        -- NULL = main category
    Level       TINYINT         NOT NULL DEFAULT 1,   -- 1=Main, 2=Sub
    SortOrder   INT             NOT NULL DEFAULT 0,
    IsActive    BIT             NOT NULL DEFAULT 1,
    IsDeleted   BIT             NOT NULL DEFAULT 0,
    CreatedAt   DATETIME2(0)    NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Categories PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Categories_Code UNIQUE (Code),
    CONSTRAINT FK_Categories_Parent
        FOREIGN KEY (ParentId) REFERENCES dbo.Categories(Id),
    CONSTRAINT CK_Categories_Level CHECK (Level IN (1,2))
);
GO

CREATE NONCLUSTERED INDEX IX_Categories_ParentId  ON dbo.Categories (ParentId) WHERE ParentId IS NOT NULL;
CREATE NONCLUSTERED INDEX IX_Categories_NameAr    ON dbo.Categories (NameAr)   WHERE IsDeleted = 0;
GO

-- ══════════════════════════════════════════════════════════════
-- TABLE: Units
-- ══════════════════════════════════════════════════════════════
CREATE TABLE dbo.Units
(
    Id           INT             NOT NULL IDENTITY(1,1),
    Code         NVARCHAR(20)    NOT NULL COLLATE Arabic_CI_AS,
    Name         NVARCHAR(100)   NOT NULL COLLATE Arabic_CI_AS,
    NameAr       NVARCHAR(100)   NOT NULL COLLATE Arabic_CI_AS,
    Abbreviation NVARCHAR(20)    NULL,
    IsActive     BIT             NOT NULL DEFAULT 1,
    CONSTRAINT PK_Units PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Units_Code UNIQUE (Code)
);
GO

-- ══════════════════════════════════════════════════════════════
-- TABLE: Suppliers (stub here — full definition in Group C)
--   Created here because Products.DefaultSupplierId references it
-- ══════════════════════════════════════════════════════════════
CREATE TABLE dbo.Suppliers
(
    Id                INT             NOT NULL IDENTITY(1,1),
    Code              NVARCHAR(50)    NOT NULL COLLATE Arabic_CI_AS,
    Name              NVARCHAR(300)   NOT NULL COLLATE Arabic_CI_AS,
    Phone             NVARCHAR(50)    NULL,
    Phone2            NVARCHAR(50)    NULL,
    Address           NVARCHAR(500)   NULL,
    Email             NVARCHAR(200)   NULL,
    TaxNumber         NVARCHAR(100)   NULL,
    ContactPerson     NVARCHAR(200)   NULL COLLATE Arabic_CI_AS,
    OpeningBalance    DECIMAL(18,4)   NOT NULL DEFAULT 0,
    CurrentBalance    DECIMAL(18,4)   NOT NULL DEFAULT 0,
    CreditLimit       DECIMAL(18,4)   NOT NULL DEFAULT 0,
    PaymentPeriodDays INT             NOT NULL DEFAULT 0,
    IsActive          BIT             NOT NULL DEFAULT 1,
    IsDeleted         BIT             NOT NULL DEFAULT 0,
    CreatedAt         DATETIME2(0)    NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT PK_Suppliers PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Suppliers_Code UNIQUE (Code)
);
GO

CREATE NONCLUSTERED INDEX IX_Suppliers_Name ON dbo.Suppliers (Name) WHERE IsDeleted = 0;
GO

-- ══════════════════════════════════════════════════════════════
-- TABLE: Products
-- ══════════════════════════════════════════════════════════════
CREATE TABLE dbo.Products
(
    Id                   INT             NOT NULL IDENTITY(1,1),
    Code                 NVARCHAR(50)    NOT NULL COLLATE Arabic_CI_AS,
    Barcode              NVARCHAR(100)   NULL,
    SecondBarcode        NVARCHAR(100)   NULL,
    NameAr               NVARCHAR(300)   NOT NULL COLLATE Arabic_CI_AS,
    NameEn               NVARCHAR(300)   NULL,
    CategoryId           INT             NOT NULL,
    BaseUnitId           INT             NOT NULL,
    SaleUnitId           INT             NOT NULL,
    PurchaseUnitId       INT             NOT NULL,
    DefaultSupplierId    INT             NULL,
    PurchasePrice        DECIMAL(18,4)   NOT NULL DEFAULT 0,
    SalePrice            DECIMAL(18,4)   NOT NULL DEFAULT 0,
    WholesalePrice       DECIMAL(18,4)   NOT NULL DEFAULT 0,
    HalfWholesalePrice   DECIMAL(18,4)   NOT NULL DEFAULT 0,
    SpecialPrice         DECIMAL(18,4)   NOT NULL DEFAULT 0,
    TaxPercent           DECIMAL(5,2)    NOT NULL DEFAULT 0,
    MinStock             DECIMAL(18,3)   NOT NULL DEFAULT 0,
    ReorderLevel         DECIMAL(18,3)   NOT NULL DEFAULT 0,
    ExpiryDate           DATE            NULL,
    HasExpiry            BIT             NOT NULL DEFAULT 0,
    Manufacturer         NVARCHAR(200)   NULL COLLATE Arabic_CI_AS,
    Description          NVARCHAR(MAX)   NULL,
    IsActive             BIT             NOT NULL DEFAULT 1,
    IsDeleted            BIT             NOT NULL DEFAULT 0,
    CreatedAt            DATETIME2(0)    NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt            DATETIME2(0)    NULL,
    CreatedBy            INT             NULL,
    CONSTRAINT PK_Products PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Products_Code UNIQUE (Code),
    CONSTRAINT FK_Products_Categories
        FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(Id),
    CONSTRAINT FK_Products_BaseUnit
        FOREIGN KEY (BaseUnitId) REFERENCES dbo.Units(Id),
    CONSTRAINT FK_Products_SaleUnit
        FOREIGN KEY (SaleUnitId) REFERENCES dbo.Units(Id),
    CONSTRAINT FK_Products_PurchaseUnit
        FOREIGN KEY (PurchaseUnitId) REFERENCES dbo.Units(Id),
    CONSTRAINT FK_Products_DefaultSupplier
        FOREIGN KEY (DefaultSupplierId) REFERENCES dbo.Suppliers(Id),
    CONSTRAINT FK_Products_CreatedBy
        FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(Id),
    CONSTRAINT CK_Products_PricesNonNegative
        CHECK (PurchasePrice >= 0 AND SalePrice >= 0 AND WholesalePrice >= 0),
    CONSTRAINT CK_Products_TaxPercent
        CHECK (TaxPercent BETWEEN 0 AND 100)
);
GO

-- Performance indexes for 100K+ products
CREATE NONCLUSTERED INDEX IX_Products_Barcode
    ON dbo.Products (Barcode) WHERE Barcode IS NOT NULL AND IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_Products_SecondBarcode
    ON dbo.Products (SecondBarcode) WHERE SecondBarcode IS NOT NULL AND IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_Products_CategoryId
    ON dbo.Products (CategoryId) WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_Products_NameAr
    ON dbo.Products (NameAr) INCLUDE (Code, Barcode, SalePrice, IsActive)
    WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_Products_Active
    ON dbo.Products (IsActive, IsDeleted) INCLUDE (Id, Code, Barcode, NameAr, SalePrice);
GO

-- Full-Text Search on product name (for fast search in POS)
IF NOT EXISTS (SELECT * FROM sys.fulltext_catalogs WHERE name = 'FT_CorePOS')
    CREATE FULLTEXT CATALOG FT_CorePOS AS DEFAULT;
GO

CREATE FULLTEXT INDEX ON dbo.Products
(
    NameAr    LANGUAGE 1025,   -- Arabic
    NameEn    LANGUAGE 1033,   -- English
    Code      LANGUAGE 0
)
KEY INDEX PK_Products
ON FT_CorePOS
WITH CHANGE_TRACKING AUTO;
GO

-- ══════════════════════════════════════════════════════════════
-- TABLE: ProductImages
-- ══════════════════════════════════════════════════════════════
CREATE TABLE dbo.ProductImages
(
    Id          INT             NOT NULL IDENTITY(1,1),
    ProductId   INT             NOT NULL,
    ImagePath   NVARCHAR(500)   NOT NULL,
    IsMain      BIT             NOT NULL DEFAULT 0,
    SortOrder   INT             NOT NULL DEFAULT 0,
    CONSTRAINT PK_ProductImages PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_ProductImages_Products
        FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id) ON DELETE CASCADE
);
GO

CREATE NONCLUSTERED INDEX IX_ProductImages_ProductId ON dbo.ProductImages (ProductId);
GO

-- ══════════════════════════════════════════════════════════════
-- TABLE: ProductUnits (multi-unit per product)
-- ══════════════════════════════════════════════════════════════
CREATE TABLE dbo.ProductUnits
(
    Id               INT             NOT NULL IDENTITY(1,1),
    ProductId        INT             NOT NULL,
    UnitId           INT             NOT NULL,
    ConversionFactor DECIMAL(18,4)   NOT NULL DEFAULT 1,
    Barcode          NVARCHAR(100)   NULL,
    CONSTRAINT PK_ProductUnits PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_ProductUnits UNIQUE (ProductId, UnitId),
    CONSTRAINT FK_ProductUnits_Products
        FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ProductUnits_Units
        FOREIGN KEY (UnitId) REFERENCES dbo.Units(Id),
    CONSTRAINT CK_ProductUnits_ConversionFactor
        CHECK (ConversionFactor > 0)
);
GO

-- ══════════════════════════════════════════════════════════════
-- TABLE: PriceLists
-- ══════════════════════════════════════════════════════════════
CREATE TABLE dbo.PriceLists
(
    Id               INT             NOT NULL IDENTITY(1,1),
    Name             NVARCHAR(200)   NOT NULL COLLATE Arabic_CI_AS,
    NameAr           NVARCHAR(200)   NOT NULL COLLATE Arabic_CI_AS,
    DiscountPercent  DECIMAL(5,2)    NOT NULL DEFAULT 0,
    IsActive         BIT             NOT NULL DEFAULT 1,
    CONSTRAINT PK_PriceLists PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT CK_PriceLists_Discount CHECK (DiscountPercent BETWEEN 0 AND 100)
);
GO

-- ══════════════════════════════════════════════════════════════
-- TABLE: ProductPrices (custom price per price list)
-- ══════════════════════════════════════════════════════════════
CREATE TABLE dbo.ProductPrices
(
    Id          INT             NOT NULL IDENTITY(1,1),
    ProductId   INT             NOT NULL,
    PriceListId INT             NOT NULL,
    Price       DECIMAL(18,4)   NOT NULL,
    CONSTRAINT PK_ProductPrices PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_ProductPrices UNIQUE (ProductId, PriceListId),
    CONSTRAINT FK_ProductPrices_Products
        FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ProductPrices_PriceLists
        FOREIGN KEY (PriceListId) REFERENCES dbo.PriceLists(Id) ON DELETE CASCADE,
    CONSTRAINT CK_ProductPrices_Price CHECK (Price >= 0)
);
GO

CREATE NONCLUSTERED INDEX IX_ProductPrices_ProductId ON dbo.ProductPrices (ProductId);
GO

PRINT '✅ Script 03: Group B (Master Data) tables created.';
GO
