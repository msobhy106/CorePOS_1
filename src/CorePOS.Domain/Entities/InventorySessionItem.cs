using CorePOS.Domain.Common;

namespace CorePOS.Domain.Entities;

public class InventorySessionItem : BaseEntity
{
    public int     SessionId      { get; private set; }
    public int     ProductId      { get; private set; }
    public decimal SystemQuantity { get; private set; }
    public decimal ActualQuantity { get; private set; }
    public decimal Difference     => ActualQuantity - SystemQuantity;
    public decimal UnitCost       { get; private set; }
    public string? Notes          { get; private set; }

    public Product? Product { get; private set; }

    protected InventorySessionItem() { }

    public static InventorySessionItem Create(int productId, decimal systemQty,
        decimal actualQty, decimal unitCost = 0, string? notes = null)
        => new() { ProductId = productId, SystemQuantity = systemQty,
                   ActualQuantity = actualQty >= 0 ? actualQty
                       : throw new ArgumentException("Actual quantity cannot be negative."),
                   UnitCost = unitCost, Notes = notes };

    public void UpdateActualQty(decimal qty)
    {
        if (qty < 0) throw new ArgumentException("Actual quantity cannot be negative.");
        ActualQuantity = qty;
    }

    public decimal DifferenceValue => Difference * UnitCost;
}
