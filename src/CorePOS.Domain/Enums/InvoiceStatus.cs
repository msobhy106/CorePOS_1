namespace CorePOS.Domain.Enums;

public enum SaleInvoiceStatus
{
    Draft           = 0,
    Held            = 1,
    Completed       = 2,
    Cancelled       = 3,
    PartialReturn   = 4,
    FullReturn      = 5
}

public enum PurchaseInvoiceStatus
{
    Draft       = 0,
    Approved    = 1,
    Cancelled   = 2
}
