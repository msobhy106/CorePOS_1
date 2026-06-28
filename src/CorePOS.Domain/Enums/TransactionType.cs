namespace CorePOS.Domain.Enums;

public enum InventoryTransactionType
{
    OpeningBalance      = 0,
    SaleOut             = 1,
    SaleReturnIn        = 2,
    PurchaseIn          = 3,
    PurchaseReturnOut   = 4,
    TransferOut         = 5,
    TransferIn          = 6,
    AdjustmentPlus      = 7,
    AdjustmentMinus     = 8,
    InventoryCountAdjust= 9
}

public enum StockDirection
{
    In  = 0,
    Out = 1
}

public enum CashBoxTransactionType
{
    OpenShift           = 0,
    CloseShift          = 1,
    Sale                = 2,
    SaleReturn          = 3,
    Purchase            = 4,
    PurchaseReturn      = 5,
    Deposit             = 6,
    Withdraw            = 7,
    Transfer            = 8,
    Expense             = 9,
    CustomerPayment     = 10,
    SupplierPayment     = 11
}
