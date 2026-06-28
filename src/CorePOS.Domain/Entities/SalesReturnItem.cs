using CorePOS.Domain.Common;

namespace CorePOS.Domain.Entities;

public class SalesReturnItem : BaseEntity
{
    public int     ReturnId      { get; private set; }
    public int     InvoiceItemId { get; private set; }
    public int     ProductId     { get; private set; }
    public int     UnitId        { get; private set; }
    public string  ProductNameAr { get; private set; } = string.Empty;
    public decimal Quantity      { get; private set; }
    public decimal UnitPrice     { get; private set; }
    public decimal TotalPrice    { get; private set; }

    protected SalesReturnItem() { }

    public static SalesReturnItem Create(int invoiceItemId, int productId, int unitId,
        string productNameAr, decimal quantity, decimal unitPrice)
    {
        if (quantity <= 0) throw new ArgumentException("Return quantity must be positive.");
        return new SalesReturnItem
        {
            InvoiceItemId = invoiceItemId, ProductId = productId, UnitId = unitId,
            ProductNameAr = productNameAr.Trim(), Quantity = quantity,
            UnitPrice = unitPrice, TotalPrice = Math.Round(quantity * unitPrice, 4)
        };
    }
}
