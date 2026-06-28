using CorePOS.Domain.Common;
using CorePOS.Domain.Events;

namespace CorePOS.Domain.Entities;

public class ProductStock : BaseEntity
{
    public int     ProductId   { get; private set; }
    public int     WarehouseId { get; private set; }
    public decimal Quantity    { get; private set; }
    public decimal AverageCost { get; private set; }
    public decimal LastCost    { get; private set; }
    public DateTime LastUpdated{ get; private set; } = DateTime.UtcNow;

    public Product?   Product   { get; private set; }
    public Warehouse? Warehouse { get; private set; }

    protected ProductStock() { }

    public static ProductStock Create(int productId, int warehouseId, decimal quantity = 0, decimal averageCost = 0)
        => new() { ProductId = productId, WarehouseId = warehouseId,
                   Quantity = quantity, AverageCost = averageCost, LastCost = averageCost };

    public void AddStock(decimal quantity, decimal unitCost)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive.");
        if (unitCost  < 0) throw new ArgumentException("Unit cost cannot be negative.");

        // Weighted average cost
        if (unitCost > 0)
        {
            AverageCost = (Quantity + quantity) > 0
                ? Math.Round((Quantity * AverageCost + quantity * unitCost) / (Quantity + quantity), 4)
                : unitCost;
            LastCost = unitCost;
        }

        Quantity    += quantity;
        LastUpdated  = DateTime.UtcNow;
        AddDomainEvent(new StockUpdatedEvent(ProductId, WarehouseId, Quantity));
    }

    public void RemoveStock(decimal quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive.");
        Quantity    -= quantity;
        LastUpdated  = DateTime.UtcNow;
        AddDomainEvent(new StockUpdatedEvent(ProductId, WarehouseId, Quantity));
    }

    public void SetQuantity(decimal quantity, decimal? unitCost = null)
    {
        Quantity = quantity;
        if (unitCost.HasValue && unitCost.Value >= 0)
        {
            AverageCost = unitCost.Value;
            LastCost    = unitCost.Value;
        }
        LastUpdated = DateTime.UtcNow;
    }

    public bool IsLowStock(decimal minStock) => Quantity <= minStock;
    public bool IsOutOfStock()               => Quantity <= 0;
    public decimal StockValue                => Quantity * AverageCost;
}
