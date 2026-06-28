-- ============================================================
--  CorePOS Enterprise
--  Phase 5 — SQL Script 09: Views
--  All reporting and dashboard views
-- ============================================================

USE CorePOS;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- ══════════════════════════════════════════════════════════════
-- VIEW: vw_SalesDaily
-- Daily sales summary for reports and dashboard
-- ══════════════════════════════════════════════════════════════
CREATE OR ALTER VIEW dbo.vw_SalesDaily
AS
SELECT
    CAST(si.InvoiceDate AS DATE)            AS SaleDate,
    si.BranchId,
    b.NameAr                                AS BranchName,
    COUNT(si.Id)                            AS InvoiceCount,
    ISNULL(SUM(si.Subtotal),        0)      AS Subtotal,
    ISNULL(SUM(si.DiscountAmount),  0)      AS TotalDiscount,
    ISNULL(SUM(si.TaxAmount),       0)      AS TotalTax,
    ISNULL(SUM(si.DeliveryCost),    0)      AS TotalDelivery,
    ISNULL(SUM(si.TotalAmount),     0)      AS TotalRevenue,
    ISNULL(SUM(si.PaidAmount),      0)      AS TotalPaid,
    ISNULL(SUM(si.RemainingAmount), 0)      AS TotalCredit,
    ISNULL(SUM(
        (SELECT ISNULL(SUM(ii.Quantity * ii.PurchasePrice), 0)
         FROM dbo.SalesInvoiceItems ii
         WHERE ii.InvoiceId = si.Id)
    ), 0)                                   AS TotalCost,
    ISNULL(SUM(si.TotalAmount), 0) -
    ISNULL(SUM(
        (SELECT ISNULL(SUM(ii.Quantity * ii.PurchasePrice), 0)
         FROM dbo.SalesInvoiceItems ii
         WHERE ii.InvoiceId = si.Id)
    ), 0)                                   AS GrossProfit
FROM dbo.SalesInvoices si
JOIN dbo.Branches b ON si.BranchId = b.Id
WHERE si.Status = 2          -- Completed only
  AND si.IsDeleted = 0
GROUP BY CAST(si.InvoiceDate AS DATE), si.BranchId, b.NameAr;
GO

-- ══════════════════════════════════════════════════════════════
-- VIEW: vw_SalesMonthly
-- Monthly sales summary
-- ══════════════════════════════════════════════════════════════
CREATE OR ALTER VIEW dbo.vw_SalesMonthly
AS
SELECT
    YEAR(si.InvoiceDate)                    AS SaleYear,
    MONTH(si.InvoiceDate)                   AS SaleMonth,
    DATEFROMPARTS(YEAR(si.InvoiceDate),
                  MONTH(si.InvoiceDate), 1) AS MonthStart,
    si.BranchId,
    b.NameAr                                AS BranchName,
    COUNT(si.Id)                            AS InvoiceCount,
    ISNULL(SUM(si.TotalAmount),     0)      AS TotalRevenue,
    ISNULL(SUM(si.PaidAmount),      0)      AS TotalPaid,
    ISNULL(SUM(si.RemainingAmount), 0)      AS TotalCredit,
    ISNULL(SUM(si.DiscountAmount),  0)      AS TotalDiscount,
    ISNULL(SUM(si.TaxAmount),       0)      AS TotalTax
FROM dbo.SalesInvoices si
JOIN dbo.Branches b ON si.BranchId = b.Id
WHERE si.Status = 2
  AND si.IsDeleted = 0
GROUP BY YEAR(si.InvoiceDate), MONTH(si.InvoiceDate),
         si.BranchId, b.NameAr;
GO

-- ══════════════════════════════════════════════════════════════
-- VIEW: vw_ProductProfitSummary
-- Profit per product (all time)
-- ══════════════════════════════════════════════════════════════
CREATE OR ALTER VIEW dbo.vw_ProductProfitSummary
AS
SELECT
    p.Id                                            AS ProductId,
    p.Code                                          AS ProductCode,
    p.Barcode,
    p.NameAr                                        AS ProductName,
    c.NameAr                                        AS CategoryName,
    ISNULL(SUM(ii.Quantity),            0)          AS TotalSoldQty,
    ISNULL(SUM(ii.TotalPrice),          0)          AS TotalRevenue,
    ISNULL(SUM(ii.Quantity * ii.PurchasePrice), 0)  AS TotalCost,
    ISNULL(SUM(ii.TotalPrice), 0) -
    ISNULL(SUM(ii.Quantity * ii.PurchasePrice), 0)  AS GrossProfit,
    CASE
        WHEN ISNULL(SUM(ii.TotalPrice), 0) > 0
        THEN ROUND(
            (ISNULL(SUM(ii.TotalPrice), 0) -
             ISNULL(SUM(ii.Quantity * ii.PurchasePrice), 0))
            / ISNULL(SUM(ii.TotalPrice), 0) * 100, 2)
        ELSE 0
    END                                             AS ProfitMarginPct
FROM dbo.Products p
LEFT JOIN dbo.Categories c ON p.CategoryId = c.Id
LEFT JOIN dbo.SalesInvoiceItems ii ON p.Id = ii.ProductId
LEFT JOIN dbo.SalesInvoices si ON ii.InvoiceId = si.Id
    AND si.Status = 2 AND si.IsDeleted = 0
WHERE p.IsDeleted = 0
GROUP BY p.Id, p.Code, p.Barcode, p.NameAr, c.NameAr;
GO

-- ══════════════════════════════════════════════════════════════
-- VIEW: vw_TopSellingProducts
-- Top selling products by quantity and revenue
-- ══════════════════════════════════════════════════════════════
CREATE OR ALTER VIEW dbo.vw_TopSellingProducts
AS
SELECT
    p.Id                                            AS ProductId,
    p.Code,
    p.Barcode,
    p.NameAr                                        AS ProductName,
    c.NameAr                                        AS CategoryName,
    ISNULL(SUM(ii.Quantity),            0)          AS TotalSoldQty,
    ISNULL(SUM(ii.TotalPrice),          0)          AS TotalRevenue,
    ISNULL(SUM(ii.TotalPrice), 0) -
    ISNULL(SUM(ii.Quantity * ii.PurchasePrice), 0)  AS GrossProfit,
    COUNT(DISTINCT si.Id)                           AS InvoiceCount
FROM dbo.Products p
LEFT JOIN dbo.Categories c ON p.CategoryId = c.Id
INNER JOIN dbo.SalesInvoiceItems ii ON p.Id = ii.ProductId
INNER JOIN dbo.SalesInvoices si ON ii.InvoiceId = si.Id
    AND si.Status = 2 AND si.IsDeleted = 0
WHERE p.IsDeleted = 0
GROUP BY p.Id, p.Code, p.Barcode, p.NameAr, c.NameAr;
GO

-- ══════════════════════════════════════════════════════════════
-- VIEW: vw_StockBalance
-- Current stock balance for all products and warehouses
-- ══════════════════════════════════════════════════════════════
CREATE OR ALTER VIEW dbo.vw_StockBalance
AS
SELECT
    p.Id                                            AS ProductId,
    p.Code                                          AS ProductCode,
    p.Barcode,
    p.NameAr                                        AS ProductName,
    c.NameAr                                        AS CategoryName,
    w.Id                                            AS WarehouseId,
    w.NameAr                                        AS WarehouseName,
    br.NameAr                                       AS BranchName,
    ISNULL(ps.Quantity,     0)                      AS CurrentStock,
    ISNULL(ps.AverageCost,  0)                      AS AverageCost,
    ISNULL(ps.LastCost,     0)                      AS LastCost,
    ISNULL(ps.Quantity, 0) * ISNULL(ps.AverageCost, 0) AS StockValue,
    p.MinStock,
    p.ReorderLevel,
    p.SalePrice,
    p.PurchasePrice,
    CASE
        WHEN ISNULL(ps.Quantity, 0) <= 0            THEN 'نفذ من المخزن'
        WHEN ISNULL(ps.Quantity, 0) <= p.MinStock   THEN 'تحت الحد الأدنى'
        WHEN ISNULL(ps.Quantity, 0) <= p.ReorderLevel THEN 'يحتاج إعادة طلب'
        ELSE 'متوفر'
    END                                             AS StockStatus,
    ps.LastUpdated
FROM dbo.Products p
LEFT JOIN dbo.Categories c ON p.CategoryId = c.Id
CROSS JOIN dbo.Warehouses w
JOIN dbo.Branches br ON w.BranchId = br.Id
LEFT JOIN dbo.ProductStock ps
    ON p.Id = ps.ProductId AND w.Id = ps.WarehouseId
WHERE p.IsActive = 1
  AND p.IsDeleted = 0
  AND w.IsActive  = 1
  AND w.IsDeleted = 0;
GO

-- ══════════════════════════════════════════════════════════════
-- VIEW: vw_LowStockProducts
-- Products at or below minimum stock level
-- ══════════════════════════════════════════════════════════════
CREATE OR ALTER VIEW dbo.vw_LowStockProducts
AS
SELECT
    p.Id                            AS ProductId,
    p.Code,
    p.Barcode,
    p.NameAr                        AS ProductName,
    c.NameAr                        AS CategoryName,
    w.NameAr                        AS WarehouseName,
    br.NameAr                       AS BranchName,
    ISNULL(ps.Quantity, 0)          AS CurrentStock,
    p.MinStock,
    p.ReorderLevel,
    p.SalePrice,
    p.PurchasePrice,
    s.Name                          AS DefaultSupplierName
FROM dbo.Products p
LEFT JOIN dbo.Categories c  ON p.CategoryId = c.Id
LEFT JOIN dbo.Suppliers s   ON p.DefaultSupplierId = s.Id
CROSS JOIN dbo.Warehouses w
JOIN dbo.Branches br ON w.BranchId = br.Id
LEFT JOIN dbo.ProductStock ps
    ON p.Id = ps.ProductId AND w.Id = ps.WarehouseId
WHERE p.IsActive  = 1
  AND p.IsDeleted = 0
  AND w.IsActive  = 1
  AND w.IsDeleted = 0
  AND p.MinStock  > 0
  AND ISNULL(ps.Quantity, 0) <= p.MinStock;
GO

-- ══════════════════════════════════════════════════════════════
-- VIEW: vw_SlowMovingProducts
-- Products with no sales in last 90 days
-- ══════════════════════════════════════════════════════════════
CREATE OR ALTER VIEW dbo.vw_SlowMovingProducts
AS
SELECT
    p.Id                            AS ProductId,
    p.Code,
    p.Barcode,
    p.NameAr                        AS ProductName,
    c.NameAr                        AS CategoryName,
    ISNULL(ps_total.TotalStock, 0)  AS TotalStock,
    ISNULL(ps_total.StockValue, 0)  AS StockValue,
    last_sale.LastSaleDate,
    CASE
        WHEN last_sale.LastSaleDate IS NULL THEN 9999
        ELSE DATEDIFF(DAY, last_sale.LastSaleDate, GETDATE())
    END                             AS DaysSinceLastSale
FROM dbo.Products p
LEFT JOIN dbo.Categories c ON p.CategoryId = c.Id
LEFT JOIN (
    SELECT ProductId,
           SUM(Quantity) AS TotalStock,
           SUM(Quantity * AverageCost) AS StockValue
    FROM dbo.ProductStock
    GROUP BY ProductId
) ps_total ON p.Id = ps_total.ProductId
LEFT JOIN (
    SELECT ii.ProductId, MAX(si.InvoiceDate) AS LastSaleDate
    FROM dbo.SalesInvoiceItems ii
    JOIN dbo.SalesInvoices si ON ii.InvoiceId = si.Id
        AND si.Status = 2 AND si.IsDeleted = 0
    GROUP BY ii.ProductId
) last_sale ON p.Id = last_sale.ProductId
WHERE p.IsActive  = 1
  AND p.IsDeleted = 0
  AND ISNULL(ps_total.TotalStock, 0) > 0
  AND (last_sale.LastSaleDate IS NULL
    OR last_sale.LastSaleDate < DATEADD(DAY, -90, GETDATE()));
GO

-- ══════════════════════════════════════════════════════════════
-- VIEW: vw_CustomerAccountStatement
-- Full account statement per customer
-- ══════════════════════════════════════════════════════════════
CREATE OR ALTER VIEW dbo.vw_CustomerAccountStatement
AS
-- Sales invoices (debit)
SELECT
    c.Id                        AS CustomerId,
    c.Code                      AS CustomerCode,
    c.Name                      AS CustomerName,
    c.Phone,
    si.InvoiceDate              AS TransactionDate,
    si.InvoiceNo                AS ReferenceNo,
    N'فاتورة بيع'               AS TransactionType,
    si.TotalAmount              AS Debit,
    0                           AS Credit,
    si.RemainingAmount          AS Balance
FROM dbo.Customers c
JOIN dbo.SalesInvoices si ON c.Id = si.CustomerId
WHERE si.Status = 2 AND si.IsDeleted = 0

UNION ALL

-- Customer payments (credit)
SELECT
    c.Id,
    c.Code,
    c.Name,
    c.Phone,
    CAST(cp.PaymentDate AS DATETIME2),
    cp.PaymentNo,
    N'سداد عميل',
    0,
    cp.Amount,
    -cp.Amount
FROM dbo.Customers c
JOIN dbo.CustomerPayments cp ON c.Id = cp.CustomerId

UNION ALL

-- Sale returns (credit)
SELECT
    c.Id,
    c.Code,
    c.Name,
    c.Phone,
    sr.ReturnDate,
    sr.ReturnNo,
    N'مرتجع بيع',
    0,
    sr.TotalAmount,
    -sr.TotalAmount
FROM dbo.Customers c
JOIN dbo.SalesReturns sr ON c.Id = sr.CustomerId;
GO

-- ══════════════════════════════════════════════════════════════
-- VIEW: vw_CustomerDebtSummary
-- Customer debt summary with aging
-- ══════════════════════════════════════════════════════════════
CREATE OR ALTER VIEW dbo.vw_CustomerDebtSummary
AS
SELECT
    c.Id                                            AS CustomerId,
    c.Code                                          AS CustomerCode,
    c.Name                                          AS CustomerName,
    c.Phone,
    c.CreditLimit,
    c.PaymentPeriodDays,
    c.CurrentBalance                                AS TotalDebt,
    c.CreditLimit - c.CurrentBalance                AS AvailableCredit,
    CASE WHEN c.CurrentBalance > c.CreditLimit
         THEN 1 ELSE 0 END                          AS IsOverLimit,
    -- Aging buckets
    ISNULL((
        SELECT SUM(RemainingAmount)
        FROM dbo.SalesInvoices
        WHERE CustomerId = c.Id
          AND Status = 2 AND IsDeleted = 0
          AND RemainingAmount > 0
          AND DATEDIFF(DAY, InvoiceDate, GETDATE()) <= 30
    ), 0)                                           AS Debt_0_30Days,
    ISNULL((
        SELECT SUM(RemainingAmount)
        FROM dbo.SalesInvoices
        WHERE CustomerId = c.Id
          AND Status = 2 AND IsDeleted = 0
          AND RemainingAmount > 0
          AND DATEDIFF(DAY, InvoiceDate, GETDATE()) BETWEEN 31 AND 60
    ), 0)                                           AS Debt_31_60Days,
    ISNULL((
        SELECT SUM(RemainingAmount)
        FROM dbo.SalesInvoices
        WHERE CustomerId = c.Id
          AND Status = 2 AND IsDeleted = 0
          AND RemainingAmount > 0
          AND DATEDIFF(DAY, InvoiceDate, GETDATE()) BETWEEN 61 AND 90
    ), 0)                                           AS Debt_61_90Days,
    ISNULL((
        SELECT SUM(RemainingAmount)
        FROM dbo.SalesInvoices
        WHERE CustomerId = c.Id
          AND Status = 2 AND IsDeleted = 0
          AND RemainingAmount > 0
          AND DATEDIFF(DAY, InvoiceDate, GETDATE()) > 90
    ), 0)                                           AS Debt_Over90Days
FROM dbo.Customers c
WHERE c.IsActive  = 1
  AND c.IsDeleted = 0
  AND c.CurrentBalance > 0;
GO

-- ══════════════════════════════════════════════════════════════
-- VIEW: vw_SupplierAccountStatement
-- Full account statement per supplier
-- ══════════════════════════════════════════════════════════════
CREATE OR ALTER VIEW dbo.vw_SupplierAccountStatement
AS
-- Purchase invoices (credit — we owe supplier)
SELECT
    s.Id                        AS SupplierId,
    s.Code                      AS SupplierCode,
    s.Name                      AS SupplierName,
    s.Phone,
    pi.InvoiceDate              AS TransactionDate,
    pi.InvoiceNo                AS ReferenceNo,
    N'فاتورة شراء'              AS TransactionType,
    0                           AS Debit,
    pi.TotalAmount              AS Credit,
    pi.RemainingAmount          AS Balance
FROM dbo.Suppliers s
JOIN dbo.PurchaseInvoices pi ON s.Id = pi.SupplierId
WHERE pi.Status = 1 AND pi.IsDeleted = 0   -- Approved

UNION ALL

-- Supplier payments (debit — we paid)
SELECT
    s.Id,
    s.Code,
    s.Name,
    s.Phone,
    CAST(sp.PaymentDate AS DATETIME2),
    sp.PaymentNo,
    N'سداد مورد',
    sp.Amount,
    0,
    -sp.Amount
FROM dbo.Suppliers s
JOIN dbo.SupplierPayments sp ON s.Id = sp.SupplierId

UNION ALL

-- Purchase returns (debit — supplier owes us)
SELECT
    s.Id,
    s.Code,
    s.Name,
    s.Phone,
    pr.ReturnDate,
    pr.ReturnNo,
    N'مرتجع شراء',
    pr.TotalAmount,
    0,
    -pr.TotalAmount
FROM dbo.Suppliers s
JOIN dbo.PurchaseReturns pr ON s.Id = pr.SupplierId;
GO

-- ══════════════════════════════════════════════════════════════
-- VIEW: vw_SupplierDebtSummary
-- Supplier payable summary
-- ══════════════════════════════════════════════════════════════
CREATE OR ALTER VIEW dbo.vw_SupplierDebtSummary
AS
SELECT
    s.Id                            AS SupplierId,
    s.Code                          AS SupplierCode,
    s.Name                          AS SupplierName,
    s.Phone,
    s.CurrentBalance                AS TotalPayable,
    s.CreditLimit,
    s.PaymentPeriodDays,
    ISNULL((
        SELECT SUM(RemainingAmount)
        FROM dbo.PurchaseInvoices
        WHERE SupplierId = s.Id
          AND Status = 1 AND IsDeleted = 0
          AND RemainingAmount > 0
          AND DATEDIFF(DAY, InvoiceDate, GETDATE()) <= 30
    ), 0)                           AS Due_0_30Days,
    ISNULL((
        SELECT SUM(RemainingAmount)
        FROM dbo.PurchaseInvoices
        WHERE SupplierId = s.Id
          AND Status = 1 AND IsDeleted = 0
          AND RemainingAmount > 0
          AND DATEDIFF(DAY, InvoiceDate, GETDATE()) BETWEEN 31 AND 60
    ), 0)                           AS Due_31_60Days,
    ISNULL((
        SELECT SUM(RemainingAmount)
        FROM dbo.PurchaseInvoices
        WHERE SupplierId = s.Id
          AND Status = 1 AND IsDeleted = 0
          AND RemainingAmount > 0
          AND DATEDIFF(DAY, InvoiceDate, GETDATE()) > 60
    ), 0)                           AS Due_Over60Days
FROM dbo.Suppliers s
WHERE s.IsActive  = 1
  AND s.IsDeleted = 0
  AND s.CurrentBalance > 0;
GO

-- ══════════════════════════════════════════════════════════════
-- VIEW: vw_CashBoxSummary
-- Current cash box balances per branch
-- ══════════════════════════════════════════════════════════════
CREATE OR ALTER VIEW dbo.vw_CashBoxSummary
AS
SELECT
    cb.Id                           AS CashBoxId,
    cb.Code,
    cb.NameAr                       AS CashBoxName,
    b.NameAr                        AS BranchName,
    cb.IsMain,
    cb.CurrentBalance,
    -- Open shift info
    sh.Id                           AS OpenShiftId,
    sh.ShiftNo,
    u.FullName                      AS CashierName,
    sh.StartTime                    AS ShiftStartTime,
    sh.OpeningBalance               AS ShiftOpeningBalance,
    -- Today's in/out
    ISNULL(today.TodayIn,  0)       AS TodayTotalIn,
    ISNULL(today.TodayOut, 0)       AS TodayTotalOut
FROM dbo.CashBoxes cb
JOIN dbo.Branches b ON cb.BranchId = b.Id
LEFT JOIN dbo.Shifts sh
    ON cb.Id = sh.CashBoxId AND sh.Status = 0   -- open shift
LEFT JOIN dbo.Users u ON sh.UserId = u.Id
LEFT JOIN (
    SELECT CashBoxId,
           SUM(CASE WHEN Direction = 0 THEN Amount ELSE 0 END) AS TodayIn,
           SUM(CASE WHEN Direction = 1 THEN Amount ELSE 0 END) AS TodayOut
    FROM dbo.CashBoxTransactions
    WHERE CAST(TransactionDate AS DATE) = CAST(GETDATE() AS DATE)
    GROUP BY CashBoxId
) today ON cb.Id = today.CashBoxId
WHERE cb.IsActive = 1;
GO

-- ══════════════════════════════════════════════════════════════
-- VIEW: vw_ProductMovement
-- Complete product movement history
-- ══════════════════════════════════════════════════════════════
CREATE OR ALTER VIEW dbo.vw_ProductMovement
AS
SELECT
    it.Id,
    p.Code                          AS ProductCode,
    p.NameAr                        AS ProductName,
    w.NameAr                        AS WarehouseName,
    br.NameAr                       AS BranchName,
    it.TransactionDate,
    CASE it.TransactionType
        WHEN 0 THEN N'رصيد افتتاحي'
        WHEN 1 THEN N'بيع'
        WHEN 2 THEN N'مرتجع بيع'
        WHEN 3 THEN N'شراء'
        WHEN 4 THEN N'مرتجع شراء'
        WHEN 5 THEN N'تحويل خروج'
        WHEN 6 THEN N'تحويل دخول'
        WHEN 7 THEN N'تسوية زيادة'
        WHEN 8 THEN N'تسوية نقص'
        WHEN 9 THEN N'جرد'
        ELSE N'أخرى'
    END                             AS TransactionTypeName,
    CASE it.Direction
        WHEN 0 THEN N'وارد'
        WHEN 1 THEN N'صادر'
    END                             AS Direction,
    it.Quantity,
    it.UnitCost,
    it.TotalCost,
    it.BalanceAfter,
    it.ReferenceId,
    it.ReferenceType,
    it.Notes,
    u.FullName                      AS CreatedByName
FROM dbo.InventoryTransactions it
JOIN dbo.Products p  ON it.ProductId   = p.Id
JOIN dbo.Warehouses w ON it.WarehouseId = w.Id
JOIN dbo.Branches br  ON w.BranchId    = br.Id
LEFT JOIN dbo.Users u ON it.UserId     = u.Id;
GO

-- ══════════════════════════════════════════════════════════════
-- VIEW: vw_ShiftSummary
-- Shift performance summary
-- ══════════════════════════════════════════════════════════════
CREATE OR ALTER VIEW dbo.vw_ShiftSummary
AS
SELECT
    sh.Id                           AS ShiftId,
    sh.ShiftNo,
    u.FullName                      AS CashierName,
    b.NameAr                        AS BranchName,
    cb.NameAr                       AS CashBoxName,
    sh.StartTime,
    sh.EndTime,
    CASE sh.Status
        WHEN 0 THEN N'مفتوحة'
        WHEN 1 THEN N'مغلقة'
    END                             AS StatusName,
    sh.OpeningBalance,
    sh.ClosingBalance,
    sh.ActualBalance,
    sh.ClosingBalance - sh.OpeningBalance   AS NetMovement,
    sh.ActualBalance  - sh.ClosingBalance   AS Difference,
    -- Sales during shift
    ISNULL(sales.InvoiceCount, 0)   AS SalesCount,
    ISNULL(sales.TotalRevenue, 0)   AS SalesRevenue,
    -- Expenses during shift
    ISNULL(exp.TotalExpenses, 0)    AS TotalExpenses
FROM dbo.Shifts sh
JOIN dbo.Users u    ON sh.UserId    = u.Id
JOIN dbo.Branches b ON sh.BranchId  = b.Id
JOIN dbo.CashBoxes cb ON sh.CashBoxId = cb.Id
LEFT JOIN (
    SELECT ShiftId, COUNT(*) AS InvoiceCount, SUM(TotalAmount) AS TotalRevenue
    FROM dbo.SalesInvoices
    WHERE Status = 2 AND IsDeleted = 0
    GROUP BY ShiftId
) sales ON sh.Id = sales.ShiftId
LEFT JOIN (
    SELECT ShiftId, SUM(Amount) AS TotalExpenses
    FROM dbo.Expenses
    GROUP BY ShiftId
) exp ON sh.Id = exp.ShiftId;
GO

-- ══════════════════════════════════════════════════════════════
-- VIEW: vw_ExpenseSummary
-- Expense summary by category and period
-- ══════════════════════════════════════════════════════════════
CREATE OR ALTER VIEW dbo.vw_ExpenseSummary
AS
SELECT
    e.ExpenseDate,
    YEAR(e.ExpenseDate)             AS ExpenseYear,
    MONTH(e.ExpenseDate)            AS ExpenseMonth,
    ec.NameAr                       AS CategoryName,
    b.NameAr                        AS BranchName,
    u.FullName                      AS CreatedByName,
    e.Amount,
    e.Description,
    e.ExpenseNo
FROM dbo.Expenses e
JOIN dbo.ExpenseCategories ec ON e.CategoryId = ec.Id
JOIN dbo.Branches b           ON e.BranchId   = b.Id
JOIN dbo.Users u              ON e.UserId      = u.Id;
GO

-- ══════════════════════════════════════════════════════════════
-- VIEW: vw_PurchaseSummary
-- Purchase invoices summary
-- ══════════════════════════════════════════════════════════════
CREATE OR ALTER VIEW dbo.vw_PurchaseSummary
AS
SELECT
    pi.Id,
    pi.InvoiceNo,
    pi.InvoiceDate,
    s.Name                          AS SupplierName,
    s.Code                          AS SupplierCode,
    b.NameAr                        AS BranchName,
    w.NameAr                        AS WarehouseName,
    u.FullName                      AS CreatedByName,
    CASE pi.Status
        WHEN 0 THEN N'مسودة'
        WHEN 1 THEN N'معتمدة'
        WHEN 2 THEN N'ملغاة'
    END                             AS StatusName,
    CASE pi.PaymentMethod
        WHEN 0 THEN N'نقدي'
        WHEN 1 THEN N'فيزا'
        WHEN 2 THEN N'تحويل بنكي'
        WHEN 3 THEN N'آجل'
    END                             AS PaymentMethodName,
    pi.Subtotal,
    pi.DiscountAmount,
    pi.TaxAmount,
    pi.TotalAmount,
    pi.PaidAmount,
    pi.RemainingAmount,
    (SELECT COUNT(*) FROM dbo.PurchaseInvoiceItems
     WHERE InvoiceId = pi.Id)       AS ItemsCount
FROM dbo.PurchaseInvoices pi
LEFT JOIN dbo.Suppliers s ON pi.SupplierId  = s.Id
JOIN  dbo.Branches b      ON pi.BranchId    = b.Id
JOIN  dbo.Warehouses w    ON pi.WarehouseId = w.Id
JOIN  dbo.Users u         ON pi.UserId      = u.Id
WHERE pi.IsDeleted = 0;
GO

-- ══════════════════════════════════════════════════════════════
-- VIEW: vw_SalesInvoicesFull
-- Complete sales invoices with all details
-- ══════════════════════════════════════════════════════════════
CREATE OR ALTER VIEW dbo.vw_SalesInvoicesFull
AS
SELECT
    si.Id,
    si.InvoiceNo,
    si.InvoiceDate,
    ISNULL(c.Name, N'نقدي')         AS CustomerName,
    ISNULL(c.Code, N'---')          AS CustomerCode,
    ISNULL(c.Phone, N'')            AS CustomerPhone,
    b.NameAr                        AS BranchName,
    w.NameAr                        AS WarehouseName,
    u.FullName                      AS CashierName,
    CASE si.InvoiceType
        WHEN 0 THEN N'قطاعي'
        WHEN 1 THEN N'جملة'
        WHEN 2 THEN N'نصف جملة'
        WHEN 3 THEN N'خاص'
    END                             AS InvoiceTypeName,
    CASE si.PaymentMethod
        WHEN 0 THEN N'نقدي'
        WHEN 1 THEN N'فيزا'
        WHEN 2 THEN N'تحويل بنكي'
        WHEN 3 THEN N'محفظة إلكترونية'
        WHEN 4 THEN N'آجل'
        WHEN 5 THEN N'مختلط'
    END                             AS PaymentMethodName,
    CASE si.Status
        WHEN 0 THEN N'مسودة'
        WHEN 1 THEN N'معلقة'
        WHEN 2 THEN N'مكتملة'
        WHEN 3 THEN N'ملغاة'
        WHEN 4 THEN N'مرتجع جزئي'
        WHEN 5 THEN N'مرتجع كلي'
    END                             AS StatusName,
    si.Subtotal,
    si.DiscountPercent,
    si.DiscountAmount,
    si.TaxPercent,
    si.TaxAmount,
    si.DeliveryCost,
    si.TotalAmount,
    si.PaidAmount,
    si.RemainingAmount,
    si.Notes,
    si.CreatedAt,
    (SELECT COUNT(*) FROM dbo.SalesInvoiceItems
     WHERE InvoiceId = si.Id)       AS ItemsCount
FROM dbo.SalesInvoices si
LEFT JOIN dbo.Customers c ON si.CustomerId  = c.Id
JOIN  dbo.Branches b      ON si.BranchId    = b.Id
JOIN  dbo.Warehouses w    ON si.WarehouseId = w.Id
JOIN  dbo.Users u         ON si.UserId      = u.Id
WHERE si.IsDeleted = 0;
GO

PRINT '✅ Script 09: All Views created successfully.';
GO
