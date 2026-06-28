using CorePOS.Domain.Common;

namespace CorePOS.Domain.Entities;

public class PurchaseReturnItem : BaseEntity
{
    public int     ReturnId      { get; private set; }
    public int     InvoiceItemId { get; private set; }
    public int     ProductId     { get; private set; }
    public int     UnitId        { get; private set; }
    public string  ProductNameAr { get; private set; } = string.Empty;
    public decimal Quantity      { get; private set; }
    public decimal UnitCost      { get; private set; }
    public decimal TotalCost     { get; private set; }

    protected PurchaseReturnItem() { }

    public static PurchaseReturnItem Create(int invoiceItemId, int productId, int unitId,
        string productNameAr, decimal quantity, decimal unitCost)
    {
        if (quantity <= 0) throw new ArgumentException("Return quantity must be positive.");
        return new PurchaseReturnItem
        {
            InvoiceItemId = invoiceItemId, ProductId = productId, UnitId = unitId,
            ProductNameAr = productNameAr.Trim(), Quantity = quantity,
            UnitCost = unitCost, TotalCost = Math.Round(quantity * unitCost, 4)
        };
    }
}
