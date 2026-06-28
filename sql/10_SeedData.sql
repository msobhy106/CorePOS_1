-- ============================================================
--  CorePOS Enterprise
--  Phase 5 — SQL Script 10: Seed Data
--  All required initial data for first run
-- ============================================================

USE CorePOS;
GO

SET NOCOUNT ON;
SET IDENTITY_INSERT dbo.Roles ON;
GO

-- ══════════════════════════════════════════════════════════════
-- SEED: Roles (6 system roles)
-- ══════════════════════════════════════════════════════════════
INSERT INTO dbo.Roles (Id, Name, NameAr, Description, IsSystem, IsActive)
VALUES
(1, 'Admin',          N'مدير النظام',     N'صلاحيات كاملة على جميع وحدات النظام', 1, 1),
(2, 'Manager',        N'مدير الفرع',      N'إدارة المبيعات والمشتريات والتقارير',  1, 1),
(3, 'Cashier',        N'كاشير',           N'شاشة البيع والمرتجعات',                1, 1),
(4, 'PurchaseClerk',  N'مسؤول مشتريات',  N'فواتير الشراء وإدارة الموردين',         1, 1),
(5, 'Accountant',     N'محاسب',           N'الخزائن والمصروفات والتقارير المالية',  1, 1),
(6, 'WarehouseClerk', N'أمين مخزن',       N'إدارة المخزون والجرد والتحويلات',      1, 1);
GO

SET IDENTITY_INSERT dbo.Roles OFF;
GO

-- ══════════════════════════════════════════════════════════════
-- SEED: Permissions (all module/action combinations)
-- ══════════════════════════════════════════════════════════════
DECLARE @perms TABLE (ModuleKey NVARCHAR(100), ActionKey NVARCHAR(50), ModuleNameAr NVARCHAR(100), ActionNameAr NVARCHAR(50));

INSERT INTO @perms VALUES
-- Dashboard
('Dashboard',         'View',   N'لوحة التحكم',         N'عرض'),
-- Products
('Products',          'View',   N'الأصناف',              N'عرض'),
('Products',          'Add',    N'الأصناف',              N'إضافة'),
('Products',          'Edit',   N'الأصناف',              N'تعديل'),
('Products',          'Delete', N'الأصناف',              N'حذف'),
('Products',          'Print',  N'الأصناف',              N'طباعة'),
-- Categories
('Categories',        'View',   N'الأقسام',              N'عرض'),
('Categories',        'Add',    N'الأقسام',              N'إضافة'),
('Categories',        'Edit',   N'الأقسام',              N'تعديل'),
('Categories',        'Delete', N'الأقسام',              N'حذف'),
-- Units
('Units',             'View',   N'الوحدات',              N'عرض'),
('Units',             'Add',    N'الوحدات',              N'إضافة'),
('Units',             'Edit',   N'الوحدات',              N'تعديل'),
('Units',             'Delete', N'الوحدات',              N'حذف'),
-- Customers
('Customers',         'View',   N'العملاء',              N'عرض'),
('Customers',         'Add',    N'العملاء',              N'إضافة'),
('Customers',         'Edit',   N'العملاء',              N'تعديل'),
('Customers',         'Delete', N'العملاء',              N'حذف'),
('Customers',         'Export', N'العملاء',              N'تصدير'),
-- Suppliers
('Suppliers',         'View',   N'الموردين',             N'عرض'),
('Suppliers',         'Add',    N'الموردين',             N'إضافة'),
('Suppliers',         'Edit',   N'الموردين',             N'تعديل'),
('Suppliers',         'Delete', N'الموردين',             N'حذف'),
-- Sales
('Sales',             'View',   N'فواتير البيع',         N'عرض'),
('Sales',             'Add',    N'فواتير البيع',         N'إضافة'),
('Sales',             'Edit',   N'فواتير البيع',         N'تعديل'),
('Sales',             'Delete', N'فواتير البيع',         N'حذف'),
('Sales',             'Print',  N'فواتير البيع',         N'طباعة'),
('Sales',             'Return', N'فواتير البيع',         N'مرتجع'),
-- Purchases
('Purchases',         'View',   N'فواتير الشراء',        N'عرض'),
('Purchases',         'Add',    N'فواتير الشراء',        N'إضافة'),
('Purchases',         'Edit',   N'فواتير الشراء',        N'تعديل'),
('Purchases',         'Delete', N'فواتير الشراء',        N'حذف'),
('Purchases',         'Print',  N'فواتير الشراء',        N'طباعة'),
('Purchases',         'Approve',N'فواتير الشراء',        N'اعتماد'),
('Purchases',         'Return', N'فواتير الشراء',        N'مرتجع'),
-- Inventory
('Inventory',         'View',   N'إدارة المخزون',        N'عرض'),
('Inventory',         'Edit',   N'إدارة المخزون',        N'تعديل'),
('Inventory',         'Count',  N'إدارة المخزون',        N'جرد'),
('Inventory',         'Transfer',N'إدارة المخزون',       N'تحويل'),
('Inventory',         'Adjust', N'إدارة المخزون',        N'تسوية'),
-- Treasury
('Treasury',          'View',   N'الخزائن',              N'عرض'),
('Treasury',          'Add',    N'الخزائن',              N'إضافة'),
('Treasury',          'Edit',   N'الخزائن',              N'تعديل'),
('Treasury',          'Deposit',N'الخزائن',              N'إيداع'),
('Treasury',          'Withdraw',N'الخزائن',             N'سحب'),
('Treasury',          'Transfer',N'الخزائن',             N'تحويل'),
-- Expenses
('Expenses',          'View',   N'المصروفات',            N'عرض'),
('Expenses',          'Add',    N'المصروفات',            N'إضافة'),
('Expenses',          'Edit',   N'المصروفات',            N'تعديل'),
('Expenses',          'Delete', N'المصروفات',            N'حذف'),
-- Employees
('Employees',         'View',   N'الموظفين',             N'عرض'),
('Employees',         'Add',    N'الموظفين',             N'إضافة'),
('Employees',         'Edit',   N'الموظفين',             N'تعديل'),
('Employees',         'Delete', N'الموظفين',             N'حذف'),
-- Reports
('Reports',           'View',   N'التقارير',             N'عرض'),
('Reports',           'Print',  N'التقارير',             N'طباعة'),
('Reports',           'Export', N'التقارير',             N'تصدير'),
-- Users
('Users',             'View',   N'إدارة المستخدمين',     N'عرض'),
('Users',             'Add',    N'إدارة المستخدمين',     N'إضافة'),
('Users',             'Edit',   N'إدارة المستخدمين',     N'تعديل'),
('Users',             'Delete', N'إدارة المستخدمين',     N'حذف'),
-- Settings
('Settings',          'View',   N'الإعدادات',            N'عرض'),
('Settings',          'Edit',   N'الإعدادات',            N'تعديل'),
-- Backup
('Backup',            'View',   N'النسخ الاحتياطي',      N'عرض'),
('Backup',            'Create', N'النسخ الاحتياطي',      N'إنشاء نسخة'),
('Backup',            'Restore',N'النسخ الاحتياطي',      N'استعادة'),
-- License
('License',           'View',   N'الترخيص',              N'عرض'),
('License',           'Activate',N'الترخيص',             N'تفعيل'),
-- Branches
('Branches',          'View',   N'الفروع والمخازن',      N'عرض'),
('Branches',          'Add',    N'الفروع والمخازن',      N'إضافة'),
('Branches',          'Edit',   N'الفروع والمخازن',      N'تعديل'),
-- Shifts
('Shifts',            'Open',   N'الورديات',             N'فتح وردية'),
('Shifts',            'Close',  N'الورديات',             N'إغلاق وردية'),
('Shifts',            'View',   N'الورديات',             N'عرض');

INSERT INTO dbo.Permissions (ModuleKey, ActionKey, ModuleNameAr, ActionNameAr)
SELECT ModuleKey, ActionKey, ModuleNameAr, ActionNameAr FROM @perms;
GO

-- ══════════════════════════════════════════════════════════════
-- SEED: RolePermissions — Admin gets ALL permissions
-- ══════════════════════════════════════════════════════════════
INSERT INTO dbo.RolePermissions (RoleId, PermissionId)
SELECT 1, Id FROM dbo.Permissions;   -- Admin = all
GO

-- Manager — everything except Users/Settings full/Backup/License
INSERT INTO dbo.RolePermissions (RoleId, PermissionId)
SELECT 2, Id FROM dbo.Permissions
WHERE ModuleKey NOT IN ('Users','Settings','Backup','License','Branches');
GO

-- Cashier — Sales + Dashboard + basic stock view + Shifts
INSERT INTO dbo.RolePermissions (RoleId, PermissionId)
SELECT 3, Id FROM dbo.Permissions
WHERE (ModuleKey = 'Sales'     AND ActionKey IN ('View','Add','Print','Return'))
   OR (ModuleKey = 'Dashboard' AND ActionKey = 'View')
   OR (ModuleKey = 'Inventory' AND ActionKey = 'View')
   OR (ModuleKey = 'Customers' AND ActionKey IN ('View','Add'))
   OR (ModuleKey = 'Shifts'    AND ActionKey IN ('Open','Close','View'))
   OR (ModuleKey = 'Reports'   AND ActionKey = 'View');
GO

-- Purchase Clerk — Purchases + Suppliers + Inventory view
INSERT INTO dbo.RolePermissions (RoleId, PermissionId)
SELECT 4, Id FROM dbo.Permissions
WHERE (ModuleKey = 'Purchases' AND ActionKey NOT IN ('Delete'))
   OR (ModuleKey = 'Suppliers' AND ActionKey IN ('View','Add','Edit'))
   OR (ModuleKey = 'Inventory' AND ActionKey = 'View')
   OR (ModuleKey = 'Dashboard' AND ActionKey = 'View')
   OR (ModuleKey = 'Reports'   AND ActionKey IN ('View','Print'));
GO

-- Accountant — Treasury + Expenses + Reports + Customer/Supplier accounts
INSERT INTO dbo.RolePermissions (RoleId, PermissionId)
SELECT 5, Id FROM dbo.Permissions
WHERE ModuleKey IN ('Treasury','Expenses','Reports')
   OR (ModuleKey = 'Customers' AND ActionKey IN ('View','Edit'))
   OR (ModuleKey = 'Suppliers' AND ActionKey IN ('View','Edit'))
   OR (ModuleKey = 'Dashboard' AND ActionKey = 'View')
   OR (ModuleKey = 'Shifts'    AND ActionKey IN ('Open','Close','View'));
GO

-- Warehouse Clerk — Inventory full + Products view
INSERT INTO dbo.RolePermissions (RoleId, PermissionId)
SELECT 6, Id FROM dbo.Permissions
WHERE ModuleKey = 'Inventory'
   OR (ModuleKey = 'Products' AND ActionKey IN ('View','Print'))
   OR (ModuleKey = 'Dashboard' AND ActionKey = 'View')
   OR (ModuleKey = 'Reports'   AND ActionKey IN ('View','Print'));
GO

-- ══════════════════════════════════════════════════════════════
-- SEED: Main Branch
-- ══════════════════════════════════════════════════════════════
SET IDENTITY_INSERT dbo.Branches ON;
INSERT INTO dbo.Branches (Id, Code, Name, NameAr, IsMain, IsActive)
VALUES (1, 'BR001', 'Main Branch', N'الفرع الرئيسي', 1, 1);
SET IDENTITY_INSERT dbo.Branches OFF;
GO

-- ══════════════════════════════════════════════════════════════
-- SEED: Main Warehouse
-- ══════════════════════════════════════════════════════════════
SET IDENTITY_INSERT dbo.Warehouses ON;
INSERT INTO dbo.Warehouses (Id, Code, Name, NameAr, BranchId, IsMain, IsActive)
VALUES (1, 'WH001', 'Main Warehouse', N'المخزن الرئيسي', 1, 1, 1);
SET IDENTITY_INSERT dbo.Warehouses OFF;
GO

-- ══════════════════════════════════════════════════════════════
-- SEED: Admin User
-- Password: Admin@123  (BCrypt hash — must be regenerated by app on first login)
-- App will detect default password and force change
-- ══════════════════════════════════════════════════════════════
SET IDENTITY_INSERT dbo.Users ON;
INSERT INTO dbo.Users
    (Id, Username, PasswordHash, FullName, FullNameAr, RoleId, BranchId, WarehouseId, IsActive)
VALUES
(1, 'admin',
 '$2a$11$PLACEHOLDER_HASH_REPLACE_ON_FIRST_RUN_BY_APPLICATION',
 'System Administrator', N'مدير النظام',
 1, 1, 1, 1);
SET IDENTITY_INSERT dbo.Users OFF;
GO

-- ══════════════════════════════════════════════════════════════
-- SEED: Main CashBox
-- ══════════════════════════════════════════════════════════════
SET IDENTITY_INSERT dbo.CashBoxes ON;
INSERT INTO dbo.CashBoxes (Id, Code, Name, NameAr, BranchId, IsMain, CurrentBalance)
VALUES (1, 'CB001', 'Main CashBox', N'الخزينة الرئيسية', 1, 1, 0);
SET IDENTITY_INSERT dbo.CashBoxes OFF;
GO

-- ══════════════════════════════════════════════════════════════
-- SEED: Units of Measure
-- ══════════════════════════════════════════════════════════════
SET IDENTITY_INSERT dbo.Units ON;
INSERT INTO dbo.Units (Id, Code, Name, NameAr, Abbreviation, IsActive) VALUES
(1,  'PCS',   'Piece',     N'قطعة',      N'قطعة', 1),
(2,  'KG',    'Kilogram',  N'كيلوجرام',  N'كجم',  1),
(3,  'GM',    'Gram',      N'جرام',      N'جم',   1),
(4,  'LTR',   'Liter',     N'لتر',       N'لتر',  1),
(5,  'ML',    'Milliliter',N'مللي لتر',  N'مل',   1),
(6,  'MTR',   'Meter',     N'متر',       N'م',    1),
(7,  'CM',    'Centimeter',N'سنتيمتر',   N'سم',   1),
(8,  'BOX',   'Box',       N'صندوق',     N'صندوق',1),
(9,  'PKT',   'Packet',    N'باكت',      N'باكت', 1),
(10, 'DZN',   'Dozen',     N'دزينة',     N'دزينة',1),
(11, 'CTN',   'Carton',    N'كرتون',     N'كرتون',1),
(12, 'BTL',   'Bottle',    N'زجاجة',     N'زجاجة',1),
(13, 'CAN',   'Can',       N'علبة',      N'علبة', 1),
(14, 'SAC',   'Sac',       N'كيس',       N'كيس',  1),
(15, 'ROL',   'Roll',      N'لفة',       N'لفة',  1);
SET IDENTITY_INSERT dbo.Units OFF;
GO

-- ══════════════════════════════════════════════════════════════
-- SEED: Categories (main + sample subs)
-- ══════════════════════════════════════════════════════════════
SET IDENTITY_INSERT dbo.Categories ON;
INSERT INTO dbo.Categories (Id, Code, Name, NameAr, ParentId, Level, SortOrder) VALUES
-- Main categories
(1,  'GEN',   'General',         N'عام',                NULL, 1, 0),
(2,  'FOOD',  'Food',            N'مواد غذائية',         NULL, 1, 1),
(3,  'BVRG',  'Beverages',       N'مشروبات',             NULL, 1, 2),
(4,  'CLTH',  'Clothing',        N'ملابس',               NULL, 1, 3),
(5,  'ELEC',  'Electronics',     N'إلكترونيات',          NULL, 1, 4),
(6,  'MOB',   'Mobile',          N'موبايل وإكسسوار',     NULL, 1, 5),
(7,  'COMP',  'Computers',       N'كمبيوتر وملحقات',     NULL, 1, 6),
(8,  'PHARM', 'Pharmacy',        N'أدوية ومستلزمات طبية',NULL, 1, 7),
(9,  'CSMTC', 'Cosmetics',       N'مستحضرات تجميل',      NULL, 1, 8),
(10, 'HOME',  'Home',            N'منزل وأدوات',         NULL, 1, 9),
-- Sub categories (examples)
(11, 'FOOD-DRY', 'Dry Food',     N'مواد غذائية جافة',    2, 2, 0),
(12, 'FOOD-FRZ', 'Frozen',       N'مجمدات',              2, 2, 1),
(13, 'BVRG-HOT', 'Hot Drinks',   N'مشروبات ساخنة',       3, 2, 0),
(14, 'BVRG-CLD', 'Cold Drinks',  N'مشروبات باردة',       3, 2, 1),
(15, 'MOB-PHN',  'Phones',       N'هواتف',               6, 2, 0),
(16, 'MOB-ACC',  'Accessories',  N'إكسسوارات',           6, 2, 1),
(17, 'COMP-LAP', 'Laptops',      N'لابتوب',              7, 2, 0),
(18, 'COMP-ACC', 'PC Accessories',N'ملحقات كمبيوتر',     7, 2, 1);
SET IDENTITY_INSERT dbo.Categories OFF;
GO

-- ══════════════════════════════════════════════════════════════
-- SEED: Expense Categories (SRS section 13 — all 7 types)
-- ══════════════════════════════════════════════════════════════
SET IDENTITY_INSERT dbo.ExpenseCategories ON;
INSERT INTO dbo.ExpenseCategories (Id, Name, NameAr, IsSystem, IsActive) VALUES
(1, 'Rent',       N'إيجار',    1, 1),
(2, 'Electricity',N'كهرباء',   1, 1),
(3, 'Water',      N'مياه',     1, 1),
(4, 'Internet',   N'إنترنت',   1, 1),
(5, 'Salaries',   N'مرتبات',   1, 1),
(6, 'Transport',  N'نقل',      1, 1),
(7, 'Other',      N'أخرى',     1, 1),
(8, 'Maintenance',N'صيانة',    0, 1),
(9, 'Marketing',  N'تسويق',    0, 1),
(10,'Insurance',  N'تأمين',    0, 1);
SET IDENTITY_INSERT dbo.ExpenseCategories OFF;
GO

-- ══════════════════════════════════════════════════════════════
-- SEED: Price Lists
-- ══════════════════════════════════════════════════════════════
SET IDENTITY_INSERT dbo.PriceLists ON;
INSERT INTO dbo.PriceLists (Id, Name, NameAr, DiscountPercent, IsActive) VALUES
(1, 'Retail',          N'قطاعي',         0,  1),
(2, 'Wholesale',       N'جملة',           0,  1),
(3, 'Half Wholesale',  N'نصف جملة',       0,  1),
(4, 'VIP',             N'VIP - خاص',      5,  1);
SET IDENTITY_INSERT dbo.PriceLists OFF;
GO

-- ══════════════════════════════════════════════════════════════
-- SEED: Customer Groups
-- ══════════════════════════════════════════════════════════════
SET IDENTITY_INSERT dbo.CustomerGroups ON;
INSERT INTO dbo.CustomerGroups (Id, Name, DiscountPercent, PointsMultiplier, IsActive) VALUES
(1, N'عميل عادي',    0,   1.0, 1),
(2, N'عميل جملة',   0,   1.5, 1),
(3, N'عميل VIP',    5,   2.0, 1),
(4, N'موظف',        10,  1.0, 1);
SET IDENTITY_INSERT dbo.CustomerGroups OFF;
GO

-- ══════════════════════════════════════════════════════════════
-- SEED: Default walk-in customer
-- ══════════════════════════════════════════════════════════════
SET IDENTITY_INSERT dbo.Customers ON;
INSERT INTO dbo.Customers
    (Id, Code, Name, GroupId, PriceListId, IsActive)
VALUES
(1, 'CASH', N'عميل نقدي', 1, 1, 1);
SET IDENTITY_INSERT dbo.Customers OFF;
GO

-- ══════════════════════════════════════════════════════════════
-- SEED: Sequences (all invoice number sequences)
-- ══════════════════════════════════════════════════════════════
INSERT INTO dbo.Sequences (SequenceKey, CurrentValue, Prefix) VALUES
('Sales',           0, 'S'),
('Purchases',       0, 'P'),
('SalesReturn',     0, 'SR'),
('PurchaseReturn',  0, 'PR'),
('Transfer',        0, 'WT'),
('Adjustment',      0, 'ADJ'),
('InventoryCount',  0, 'INV'),
('Expense',         0, 'EXP'),
('Shift',           0, 'SHF'),
('CustomerPayment', 0, 'CP'),
('SupplierPayment', 0, 'SP'),
('ProductCode',     0, 'PRD');
GO

-- ══════════════════════════════════════════════════════════════
-- SEED: Settings (all application settings)
-- ══════════════════════════════════════════════════════════════
INSERT INTO dbo.Settings (SettingKey, SettingValue, SettingGroup, DataType, Description) VALUES
-- Company
('CompanyName',             N'',            'Company',  'string',  N'اسم الشركة'),
('CompanyNameAr',           N'',            'Company',  'string',  N'اسم الشركة بالعربي'),
('CompanyAddress',          N'',            'Company',  'string',  N'عنوان الشركة'),
('CompanyPhone',            N'',            'Company',  'string',  N'هاتف الشركة'),
('CompanyEmail',            N'',            'Company',  'string',  N'البريد الإلكتروني'),
('CompanyLogoPath',         N'',            'Company',  'string',  N'مسار شعار الشركة'),
('CompanyTaxNumber',        N'',            'Company',  'string',  N'الرقم الضريبي'),
-- POS
('POSDefaultWarehouseId',   '1',            'POS',      'int',     N'المخزن الافتراضي لنقطة البيع'),
('POSDefaultCashBoxId',     '1',            'POS',      'int',     N'الخزينة الافتراضية'),
('POSRequireShift',         'true',         'POS',      'bool',    N'إجبار فتح وردية قبل البيع'),
('POSAllowNegativeStock',   'false',        'POS',      'bool',    N'السماح بالبيع عند نفاذ المخزون'),
('POSShowProductImage',     'true',         'POS',      'bool',    N'عرض صورة الصنف في الكاشير'),
('POSBarcodeAutoAdd',       'true',         'POS',      'bool',    N'إضافة الصنف تلقائياً عند المسح'),
-- Printing
('PrinterSize',             '80mm',         'Printing', 'string',  N'حجم ورق الطباعة (58mm,80mm,A4,A5)'),
('PrinterAskEveryTime',     'false',        'Printing', 'bool',    N'السؤال عن حجم الورق عند كل طباعة'),
('ReceiptPrinterName',      N'',            'Printing', 'string',  N'اسم طابعة الإيصالات'),
('BarcodePrinterName',      N'',            'Printing', 'string',  N'اسم طابعة الباركود'),
('PrintReceiptOnSale',      'true',         'Printing', 'bool',    N'طباعة إيصال عند البيع تلقائياً'),
('PrintCopies',             '1',            'Printing', 'int',     N'عدد نسخ الطباعة'),
('InvoiceHeaderText',       N'',            'Printing', 'string',  N'رأس الفاتورة'),
('InvoiceFooterText',       N'شكراً لزيارتكم', 'Printing', 'string', N'تذييل الفاتورة'),
-- Hardware
('CashDrawerEnabled',       'false',        'Hardware', 'bool',    N'تفعيل درج الكاش'),
('CashDrawerPort',          'COM1',         'Hardware', 'string',  N'منفذ درج الكاش'),
('BarcodeScannerEnabled',   'false',        'Hardware', 'bool',    N'تفعيل قارئ الباركود'),
('BarcodeScannerPort',      'USB',          'Hardware', 'string',  N'نوع اتصال قارئ الباركود'),
-- Financial
('DefaultTaxPercent',       '0',            'Finance',  'decimal', N'نسبة الضريبة الافتراضية'),
('CurrencyName',            N'جنيه',        'Finance',  'string',  N'اسم العملة'),
('CurrencySymbol',          N'ج.م',         'Finance',  'string',  N'رمز العملة'),
('DecimalPlaces',           '2',            'Finance',  'int',     N'عدد الخانات العشرية'),
('PointsPerPound',          '1',            'Finance',  'decimal', N'نقاط لكل جنيه مشتريات'),
('PointsRedemptionRate',    '100',          'Finance',  'decimal', N'عدد النقاط = 1 جنيه'),
-- Inventory
('StockValuationMethod',    'Average',      'Inventory','string',  N'طريقة تقييم المخزون (Average/FIFO)'),
('LowStockAlertEnabled',    'true',         'Inventory','bool',    N'تنبيه المخزون المنخفض'),
-- Backup
('BackupAutoEnabled',       'true',         'Backup',   'bool',    N'النسخ الاحتياطي التلقائي'),
('BackupSchedule',          'Daily',        'Backup',   'string',  N'جدول النسخ الاحتياطي'),
('BackupTime',              '23:00',        'Backup',   'string',  N'وقت النسخ الاحتياطي'),
('BackupPath',              'C:\CorePOS\Backups', 'Backup', 'string', N'مسار حفظ النسخ الاحتياطية'),
('BackupRetainDays',        '30',           'Backup',   'int',     N'مدة الاحتفاظ بالنسخ الاحتياطية (يوم)'),
-- System
('AppVersion',              '1.0.0',        'System',   'string',  N'إصدار البرنامج'),
('DefaultLanguage',         'ar',           'System',   'string',  N'اللغة الافتراضية'),
('DefaultBranchId',         '1',            'System',   'int',     N'الفرع الافتراضي'),
('DateFormat',              'dd/MM/yyyy',   'System',   'string',  N'تنسيق التاريخ'),
('ForcePasswordChange',     'true',         'System',   'bool',    N'إجبار تغيير كلمة المرور عند أول دخول');
GO

-- ══════════════════════════════════════════════════════════════
-- SEED: Trial License (7 days from today)
-- ══════════════════════════════════════════════════════════════
INSERT INTO dbo.Licenses
    (LicenseKey, LicenseType, StartDate, ExpiryDate, IsActive)
VALUES
(
    'TRIAL-' + CONVERT(NVARCHAR(50), NEWID()),
    0,                          -- Trial
    GETUTCDATE(),
    DATEADD(DAY, 7, GETUTCDATE()),
    1
);
GO

PRINT '✅ Script 10: All Seed Data inserted successfully.';
PRINT '';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
PRINT '  IMPORTANT: Admin password must be set by app  ';
PRINT '  Default username: admin                        ';
PRINT '  Force password change: YES                     ';
PRINT '  Trial license: 7 days from today               ';
PRINT '━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━';
GO
