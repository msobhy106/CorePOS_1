-- ============================================================
--  CorePOS Enterprise
--  Phase 5 — SQL Script 08: Stored Procedures
--  All business-critical procedures
-- ============================================================

USE CorePOS;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- ══════════════════════════════════════════════════════════════
-- SP: usp_GetNextSequence
-- Atomically increments and returns next invoice number
-- ══════════════════════════════════════════════════════════════
CREATE OR ALTER PROCEDURE dbo.usp_GetNextSequence
    @SequenceKey    NVARCHAR(100),
    @NextValue      INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Insert if not exists, then increment atomically
    MERGE dbo.Sequences AS target
    USING (SELECT @SequenceKey AS SequenceKey) AS source
        ON target.SequenceKey = source.SequenceKey
    WHEN NOT MATCHED THEN
        INSERT (SequenceKey, CurrentValue) VALUES (@SequenceKey, 1)
    WHEN MATCHED THEN
        UPDATE SET CurrentValue = target.CurrentValue + 1;

    SELECT @NextValue = CurrentValue
    FROM dbo.Sequences
    WHERE SequenceKey = @SequenceKey;
END;
GO

-- ══════════════════════════════════════════════════════════════
-- SP: usp_UpdateProductStock
-- Updates ProductStock and logs InventoryTransaction
-- Called from application after every stock movement
-- ══════════════════════════════════════════════════════════════
CREATE OR ALTER PROCEDURE dbo.usp_UpdateProductStock
    @ProductId          INT,
    @WarehouseId        INT,
    @Quantity           DECIMAL(18,3),
    @Direction          TINYINT,         -- 0=In, 1=Out
    @TransactionType    TINYINT,
    @UnitCost           DECIMAL(18,4)   = 0,
    @ReferenceId        INT             = NULL,
    @ReferenceType      NVARCHAR(50)    = NULL,
    @Notes              NVARCHAR(500)   = NULL,
    @UserId             INT             = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    DECLARE @CurrentQty     DECIMAL(18,3)   = 0;
    DECLARE @CurrentAvgCost DECIMAL(18,4)   = 0;
    DECLARE @NewQty         DECIMAL(18,3)   = 0;
    DECLARE @NewAvgCost     DECIMAL(18,4)   = 0;
    DECLARE @TotalCost      DECIMAL(18,4)   = 0;

    -- Get current stock (lock the row)
    SELECT @CurrentQty = ISNULL(Quantity, 0),
           @CurrentAvgCost = ISNULL(AverageCost, 0)
    FROM dbo.ProductStock WITH (UPDLOCK, ROWLOCK)
    WHERE ProductId = @ProductId AND WarehouseId = @WarehouseId;

    -- Calculate new quantity
    IF @Direction = 0  -- In
        SET @NewQty = @CurrentQty + @Quantity;
    ELSE               -- Out
        SET @NewQty = @CurrentQty - @Quantity;

    -- Calculate weighted average cost (only on In transactions)
    IF @Direction = 0 AND @UnitCost > 0
    BEGIN
        SET @NewAvgCost = CASE
            WHEN (@CurrentQty + @Quantity) > 0
            THEN ((@CurrentQty * @CurrentAvgCost) + (@Quantity * @UnitCost))
                 / (@CurrentQty + @Quantity)
            ELSE @UnitCost
        END;
    END
    ELSE
        SET @NewAvgCost = @CurrentAvgCost;

    SET @TotalCost = @Quantity * @UnitCost;

    -- Upsert ProductStock
    MERGE dbo.ProductStock AS target
    USING (SELECT @ProductId AS ProductId, @WarehouseId AS WarehouseId) AS source
        ON target.ProductId = source.ProductId
       AND target.WarehouseId = source.WarehouseId
    WHEN NOT MATCHED THEN
        INSERT (ProductId, WarehouseId, Quantity, AverageCost, LastCost, LastUpdated)
        VALUES (@ProductId, @WarehouseId, @NewQty, @NewAvgCost,
                CASE WHEN @Direction = 0 AND @UnitCost > 0 THEN @UnitCost ELSE 0 END,
                GETUTCDATE())
    WHEN MATCHED THEN
        UPDATE SET
            Quantity    = @NewQty,
            AverageCost = @NewAvgCost,
            LastCost    = CASE WHEN @Direction = 0 AND @UnitCost > 0
                               THEN @UnitCost ELSE target.LastCost END,
            LastUpdated = GETUTCDATE();

    -- Log inventory transaction
    INSERT INTO dbo.InventoryTransactions
        (ProductId, WarehouseId, TransactionDate, TransactionType,
         Quantity, Direction, UnitCost, TotalCost, BalanceAfter,
         ReferenceId, ReferenceType, Notes, UserId, CreatedAt)
    VALUES
        (@ProductId, @WarehouseId, GETUTCDATE(), @TransactionType,
         @Quantity, @Direction, @UnitCost, @TotalCost, @NewQty,
         @ReferenceId, @ReferenceType, @Notes, @UserId, GETUTCDATE());

    COMMIT TRANSACTION;
END;
GO

-- ══════════════════════════════════════════════════════════════
-- SP: usp_CompleteSaleInvoice
-- Completes a sale: updates stock + treasury + customer balance
-- ══════════════════════════════════════════════════════════════
CREATE OR ALTER PROCEDURE dbo.usp_CompleteSaleInvoice
    @InvoiceId  INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    DECLARE @BranchId       INT;
    DECLARE @WarehouseId    INT;
    DECLARE @UserId         INT;
    DECLARE @CustomerId     INT;
    DECLARE @TotalAmount    DECIMAL(18,4);
    DECLARE @PaidAmount     DECIMAL(18,4);
    DECLARE @RemainingAmt   DECIMAL(18,4);
    DECLARE @ShiftId        INT;

    -- Get invoice header
    SELECT
        @BranchId       = BranchId,
        @WarehouseId    = WarehouseId,
        @UserId         = UserId,
        @CustomerId     = CustomerId,
        @TotalAmount    = TotalAmount,
        @PaidAmount     = PaidAmount,
        @RemainingAmt   = RemainingAmount,
        @ShiftId        = ShiftId
    FROM dbo.SalesInvoices
    WHERE Id = @InvoiceId AND Status = 0;  -- must be Draft

    IF @@ROWCOUNT = 0
    BEGIN
        RAISERROR('Invoice not found or already processed.', 16, 1);
        ROLLBACK;
        RETURN;
    END

    -- 1. Deduct stock for each item
    DECLARE @ProductId  INT;
    DECLARE @Quantity   DECIMAL(18,3);
    DECLARE @UnitCost   DECIMAL(18,4);

    DECLARE item_cursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT ProductId, Quantity, PurchasePrice
        FROM dbo.SalesInvoiceItems
        WHERE InvoiceId = @InvoiceId;

    OPEN item_cursor;
    FETCH NEXT FROM item_cursor INTO @ProductId, @Quantity, @UnitCost;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC dbo.usp_UpdateProductStock
            @ProductId       = @ProductId,
            @WarehouseId     = @WarehouseId,
            @Quantity        = @Quantity,
            @Direction       = 1,           -- Out
            @TransactionType = 1,           -- SaleOut
            @UnitCost        = @UnitCost,
            @ReferenceId     = @InvoiceId,
            @ReferenceType   = 'SalesInvoice',
            @UserId          = @UserId;

        FETCH NEXT FROM item_cursor INTO @ProductId, @Quantity, @UnitCost;
    END

    CLOSE item_cursor;
    DEALLOCATE item_cursor;

    -- 2. Update customer balance if credit sale
    IF @CustomerId IS NOT NULL AND @RemainingAmt > 0
    BEGIN
        UPDATE dbo.Customers
        SET CurrentBalance = CurrentBalance + @RemainingAmt
        WHERE Id = @CustomerId;
    END

    -- 3. Mark invoice as completed
    UPDATE dbo.SalesInvoices
    SET Status = 2, UpdatedAt = GETUTCDATE()   -- 2=Completed
    WHERE Id = @InvoiceId;

    COMMIT TRANSACTION;
END;
GO

-- ══════════════════════════════════════════════════════════════
-- SP: usp_ProcessSaleReturn
-- Reverses stock and treasury for a sale return
-- ══════════════════════════════════════════════════════════════
CREATE OR ALTER PROCEDURE dbo.usp_ProcessSaleReturn
    @ReturnId   INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    DECLARE @WarehouseId    INT;
    DECLARE @UserId         INT;
    DECLARE @CustomerId     INT;
    DECLARE @TotalAmount    DECIMAL(18,4);
    DECLARE @RefundMethod   TINYINT;

    SELECT
        @WarehouseId  = WarehouseId,
        @UserId       = UserId,
        @CustomerId   = CustomerId,
        @TotalAmount  = TotalAmount,
        @RefundMethod = RefundMethod
    FROM dbo.SalesReturns
    WHERE Id = @ReturnId;

    -- Return stock for each item
    DECLARE @ProductId  INT;
    DECLARE @Quantity   DECIMAL(18,3);
    DECLARE @UnitPrice  DECIMAL(18,4);

    DECLARE ret_cursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT ri.ProductId, ri.Quantity, si_item.PurchasePrice
        FROM dbo.SalesReturnItems ri
        JOIN dbo.SalesInvoiceItems si_item ON ri.InvoiceItemId = si_item.Id
        WHERE ri.ReturnId = @ReturnId;

    OPEN ret_cursor;
    FETCH NEXT FROM ret_cursor INTO @ProductId, @Quantity, @UnitPrice;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC dbo.usp_UpdateProductStock
            @ProductId       = @ProductId,
            @WarehouseId     = @WarehouseId,
            @Quantity        = @Quantity,
            @Direction       = 0,               -- In (return to stock)
            @TransactionType = 2,               -- SaleReturnIn
            @UnitCost        = @UnitPrice,
            @ReferenceId     = @ReturnId,
            @ReferenceType   = 'SalesReturn',
            @UserId          = @UserId;

        FETCH NEXT FROM ret_cursor INTO @ProductId, @Quantity, @UnitPrice;
    END

    CLOSE ret_cursor;
    DEALLOCATE ret_cursor;

    -- If credit refund: reduce customer balance
    IF @CustomerId IS NOT NULL AND @RefundMethod = 1
    BEGIN
        UPDATE dbo.Customers
        SET CurrentBalance = CurrentBalance - @TotalAmount
        WHERE Id = @CustomerId;
    END

    -- Update returned quantities on original items
    UPDATE si_item
    SET ReturnedQty = ReturnedQty + ri.Quantity
    FROM dbo.SalesInvoiceItems si_item
    JOIN dbo.SalesReturnItems ri ON si_item.Id = ri.InvoiceItemId
    WHERE ri.ReturnId = @ReturnId;

    COMMIT TRANSACTION;
END;
GO

-- ══════════════════════════════════════════════════════════════
-- SP: usp_ApprovePurchaseInvoice
-- Approves purchase: adds stock + updates supplier balance
-- ══════════════════════════════════════════════════════════════
CREATE OR ALTER PROCEDURE dbo.usp_ApprovePurchaseInvoice
    @InvoiceId  INT,
    @ApprovedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    DECLARE @WarehouseId    INT;
    DECLARE @SupplierId     INT;
    DECLARE @TotalAmount    DECIMAL(18,4);
    DECLARE @PaidAmount     DECIMAL(18,4);
    DECLARE @RemainingAmt   DECIMAL(18,4);

    SELECT
        @WarehouseId    = WarehouseId,
        @SupplierId     = SupplierId,
        @TotalAmount    = TotalAmount,
        @PaidAmount     = PaidAmount,
        @RemainingAmt   = RemainingAmount
    FROM dbo.PurchaseInvoices
    WHERE Id = @InvoiceId AND Status = 0;   -- must be Draft

    IF @@ROWCOUNT = 0
    BEGIN
        RAISERROR('Invoice not found or already approved.', 16, 1);
        ROLLBACK;
        RETURN;
    END

    -- Add stock for each item
    DECLARE @ProductId  INT;
    DECLARE @Quantity   DECIMAL(18,3);
    DECLARE @UnitCost   DECIMAL(18,4);

    DECLARE purch_cursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT ProductId, Quantity, UnitCost
        FROM dbo.PurchaseInvoiceItems
        WHERE InvoiceId = @InvoiceId;

    OPEN purch_cursor;
    FETCH NEXT FROM purch_cursor INTO @ProductId, @Quantity, @UnitCost;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC dbo.usp_UpdateProductStock
            @ProductId       = @ProductId,
            @WarehouseId     = @WarehouseId,
            @Quantity        = @Quantity,
            @Direction       = 0,               -- In
            @TransactionType = 3,               -- PurchaseIn
            @UnitCost        = @UnitCost,
            @ReferenceId     = @InvoiceId,
            @ReferenceType   = 'PurchaseInvoice',
            @UserId          = @ApprovedBy;

        FETCH NEXT FROM purch_cursor INTO @ProductId, @Quantity, @UnitCost;
    END

    CLOSE purch_cursor;
    DEALLOCATE purch_cursor;

    -- Update supplier balance
    IF @SupplierId IS NOT NULL AND @RemainingAmt > 0
    BEGIN
        UPDATE dbo.Suppliers
        SET CurrentBalance = CurrentBalance + @RemainingAmt
        WHERE Id = @SupplierId;
    END

    -- Mark as approved
    UPDATE dbo.PurchaseInvoices
    SET Status = 1, ApprovedAt = GETUTCDATE(), ApprovedBy = @ApprovedBy
    WHERE Id = @InvoiceId;

    COMMIT TRANSACTION;
END;
GO

-- ══════════════════════════════════════════════════════════════
-- SP: usp_ApproveInventorySession
-- Approves inventory count and adjusts stock differences
-- ══════════════════════════════════════════════════════════════
CREATE OR ALTER PROCEDURE dbo.usp_ApproveInventorySession
    @SessionId  INT,
    @ApprovedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    DECLARE @WarehouseId INT;

    SELECT @WarehouseId = WarehouseId
    FROM dbo.InventorySessions
    WHERE Id = @SessionId AND Status = 0;   -- must be Open

    IF @@ROWCOUNT = 0
    BEGIN
        RAISERROR('Session not found or already approved.', 16, 1);
        ROLLBACK;
        RETURN;
    END

    -- Apply differences
    DECLARE @ProductId  INT;
    DECLARE @Diff       DECIMAL(18,3);
    DECLARE @ActualQty  DECIMAL(18,3);
    DECLARE @UnitCost   DECIMAL(18,4);

    DECLARE inv_cursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT ProductId,
               (ActualQuantity - SystemQuantity) AS Diff,
               ActualQuantity,
               UnitCost
        FROM dbo.InventorySessionItems
        WHERE SessionId = @SessionId
          AND (ActualQuantity - SystemQuantity) <> 0;

    OPEN inv_cursor;
    FETCH NEXT FROM inv_cursor INTO @ProductId, @Diff, @ActualQty, @UnitCost;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        IF @Diff > 0
            EXEC dbo.usp_UpdateProductStock
                @ProductId = @ProductId, @WarehouseId = @WarehouseId,
                @Quantity = @Diff, @Direction = 0,
                @TransactionType = 9, @UnitCost = @UnitCost,
                @ReferenceId = @SessionId, @ReferenceType = 'InventorySession',
                @UserId = @ApprovedBy;
        ELSE IF @Diff < 0
            EXEC dbo.usp_UpdateProductStock
                @ProductId = @ProductId, @WarehouseId = @WarehouseId,
                @Quantity = ABS(@Diff), @Direction = 1,
                @TransactionType = 9, @UnitCost = @UnitCost,
                @ReferenceId = @SessionId, @ReferenceType = 'InventorySession',
                @UserId = @ApprovedBy;

        FETCH NEXT FROM inv_cursor INTO @ProductId, @Diff, @ActualQty, @UnitCost;
    END

    CLOSE inv_cursor;
    DEALLOCATE inv_cursor;

    -- Mark session as approved
    UPDATE dbo.InventorySessions
    SET Status = 1, ApprovedBy = @ApprovedBy, ApprovedAt = GETUTCDATE()
    WHERE Id = @SessionId;

    COMMIT TRANSACTION;
END;
GO

-- ══════════════════════════════════════════════════════════════
-- SP: usp_CloseShift
-- Closes shift with closing balance calculation
-- ══════════════════════════════════════════════════════════════
CREATE OR ALTER PROCEDURE dbo.usp_CloseShift
    @ShiftId        INT,
    @ActualBalance  DECIMAL(18,4),
    @Notes          NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CashBoxId      INT;
    DECLARE @CalcBalance    DECIMAL(18,4);

    SELECT @CashBoxId = CashBoxId FROM dbo.Shifts WHERE Id = @ShiftId AND Status = 0;

    IF @CashBoxId IS NULL
    BEGIN
        RAISERROR('Shift not found or already closed.', 16, 1);
        RETURN;
    END

    -- Calculate expected balance from transactions
    SELECT @CalcBalance = ISNULL(SUM(CASE WHEN Direction = 0 THEN Amount ELSE -Amount END), 0)
    FROM dbo.CashBoxTransactions
    WHERE ShiftId = @ShiftId;

    UPDATE dbo.Shifts
    SET Status         = 1,
        EndTime        = GETUTCDATE(),
        ClosingBalance = @CalcBalance,
        ActualBalance  = @ActualBalance,
        Notes          = ISNULL(@Notes, Notes)
    WHERE Id = @ShiftId;
END;
GO

-- ══════════════════════════════════════════════════════════════
-- SP: usp_GetDashboardData
-- Returns dashboard KPIs for today + comparison
-- ══════════════════════════════════════════════════════════════
CREATE OR ALTER PROCEDURE dbo.usp_GetDashboardData
    @BranchId   INT = NULL,
    @Date       DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @Date = ISNULL(@Date, CAST(GETDATE() AS DATE));

    -- Today sales
    SELECT
        COUNT(*)                                            AS TodayInvoiceCount,
        ISNULL(SUM(TotalAmount), 0)                        AS TodayRevenue,
        ISNULL(SUM(PaidAmount), 0)                         AS TodayPaid,
        ISNULL(SUM(RemainingAmount), 0)                    AS TodayCredit
    FROM dbo.SalesInvoices
    WHERE CAST(InvoiceDate AS DATE) = @Date
      AND Status = 2              -- Completed
      AND IsDeleted = 0
      AND (@BranchId IS NULL OR BranchId = @BranchId);

    -- Today profit
    SELECT
        ISNULL(SUM(ii.TotalPrice - (ii.Quantity * ii.PurchasePrice)), 0) AS TodayGrossProfit
    FROM dbo.SalesInvoices si
    JOIN dbo.SalesInvoiceItems ii ON si.Id = ii.InvoiceId
    WHERE CAST(si.InvoiceDate AS DATE) = @Date
      AND si.Status = 2
      AND si.IsDeleted = 0
      AND (@BranchId IS NULL OR si.BranchId = @BranchId);

    -- Active customers count
    SELECT COUNT(*) AS TotalCustomers
    FROM dbo.Customers
    WHERE IsActive = 1 AND IsDeleted = 0;

    -- Low stock products
    SELECT COUNT(*) AS LowStockCount
    FROM dbo.ProductStock ps
    JOIN dbo.Products p ON ps.ProductId = p.Id
    WHERE ps.Quantity <= p.MinStock
      AND p.MinStock > 0
      AND p.IsActive = 1
      AND p.IsDeleted = 0;

    -- Top 5 products today
    SELECT TOP 5
        p.NameAr,
        SUM(ii.Quantity) AS TotalSold,
        SUM(ii.TotalPrice) AS TotalRevenue
    FROM dbo.SalesInvoiceItems ii
    JOIN dbo.SalesInvoices si ON ii.InvoiceId = si.Id
    JOIN dbo.Products p ON ii.ProductId = p.Id
    WHERE CAST(si.InvoiceDate AS DATE) = @Date
      AND si.Status = 2
      AND si.IsDeleted = 0
      AND (@BranchId IS NULL OR si.BranchId = @BranchId)
    GROUP BY p.Id, p.NameAr
    ORDER BY TotalSold DESC;

    -- Monthly sales trend (last 12 months)
    SELECT
        YEAR(InvoiceDate)   AS SaleYear,
        MONTH(InvoiceDate)  AS SaleMonth,
        COUNT(*)            AS InvoiceCount,
        SUM(TotalAmount)    AS Revenue,
        SUM(TotalAmount - ISNULL((
            SELECT SUM(ii2.Quantity * ii2.PurchasePrice)
            FROM dbo.SalesInvoiceItems ii2
            WHERE ii2.InvoiceId = si.Id
        ), 0))              AS GrossProfit
    FROM dbo.SalesInvoices si
    WHERE InvoiceDate >= DATEADD(MONTH, -11, DATEFROMPARTS(YEAR(@Date), MONTH(@Date), 1))
      AND Status = 2
      AND IsDeleted = 0
      AND (@BranchId IS NULL OR BranchId = @BranchId)
    GROUP BY YEAR(InvoiceDate), MONTH(InvoiceDate)
    ORDER BY SaleYear, SaleMonth;
END;
GO

PRINT '✅ Script 08: All Stored Procedures created.';
GO
