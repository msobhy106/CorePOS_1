using CorePOS.Domain.Common;

namespace CorePOS.Domain.Entities;

public class StockAdjustment : AuditableEntity
{
    public string   AdjustmentNo   { get; private set; } = string.Empty;
    public DateTime AdjustmentDate { get; private set; }
    public int      WarehouseId    { get; private set; }
    public int      Type           { get; private set; }  // 0=Increase,1=Decrease
    public int      UserId         { get; private set; }
    public string?  Notes          { get; private set; }

    public Warehouse? Warehouse { get; private set; }

    private readonly List<StockAdjustmentItem> _items = [];
    public IReadOnlyCollection<StockAdjustmentItem> Items => _items.AsReadOnly();

    protected StockAdjustment() { }

    public static StockAdjustment Create(string adjustmentNo, int warehouseId,
        int type, int userId, string? notes = null)
    {
        if (type != 0 && type != 1) throw new ArgumentException("Type must be 0=Increase or 1=Decrease.");
        return new StockAdjustment { AdjustmentNo = adjustmentNo.Trim(),
            AdjustmentDate = DateTime.Now, WarehouseId = warehouseId,
            Type = type, UserId = userId, Notes = notes };
    }

    public StockAdjustmentItem AddItem(int productId, string productNameAr,
        decimal quantity, decimal unitCost = 0, string? reason = null)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive.");
        var item = StockAdjustmentItem.Create(productId, productNameAr, quantity, unitCost, reason);
        _items.Add(item);
        return item;
    }

    public bool IsIncrease => Type == 0;
    public bool IsDecrease => Type == 1;
}
