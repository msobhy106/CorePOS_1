using CorePOS.Domain.Common;

namespace CorePOS.Domain.Entities;

public class StockAdjustmentItem : BaseEntity
{
    public int     AdjustmentId  { get; private set; }
    public int     ProductId     { get; private set; }
    public string  ProductNameAr { get; private set; } = string.Empty;
    public decimal Quantity      { get; private set; }
    public decimal UnitCost      { get; private set; }
    public string? Reason        { get; private set; }

    public Product? Product { get; private set; }

    protected StockAdjustmentItem() { }

    public static StockAdjustmentItem Create(int productId, string productNameAr,
        decimal quantity, decimal unitCost = 0, string? reason = null)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive.");
        return new StockAdjustmentItem { ProductId = productId,
            ProductNameAr = productNameAr.Trim(), Quantity = quantity,
            UnitCost = unitCost, Reason = reason };
    }
}
