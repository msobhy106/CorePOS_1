using CorePOS.Domain.Common;

namespace CorePOS.Domain.Entities;

public class WarehouseTransferItem : BaseEntity
{
    public int     TransferId    { get; private set; }
    public int     ProductId     { get; private set; }
    public string  ProductNameAr { get; private set; } = string.Empty;
    public decimal Quantity      { get; private set; }
    public decimal UnitCost      { get; private set; }

    public Product? Product { get; private set; }

    protected WarehouseTransferItem() { }

    public static WarehouseTransferItem Create(int productId, string productNameAr,
        decimal quantity, decimal unitCost = 0)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive.");
        return new WarehouseTransferItem { ProductId = productId,
            ProductNameAr = productNameAr.Trim(), Quantity = quantity, UnitCost = unitCost };
    }
}
