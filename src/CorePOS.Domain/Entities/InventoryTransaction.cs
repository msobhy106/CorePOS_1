using CorePOS.Domain.Common;
using CorePOS.Domain.Enums;

namespace CorePOS.Domain.Entities;

public class InventoryTransaction : BaseEntity
{
    public int                       ProductId       { get; private set; }
    public int                       WarehouseId     { get; private set; }
    public DateTime                  TransactionDate { get; private set; }
    public InventoryTransactionType  TransactionType { get; private set; }
    public decimal                   Quantity        { get; private set; }
    public StockDirection            Direction       { get; private set; }
    public decimal                   UnitCost        { get; private set; }
    public decimal                   TotalCost       { get; private set; }
    public decimal                   BalanceAfter    { get; private set; }
    public int?                      ReferenceId     { get; private set; }
    public string?                   ReferenceType   { get; private set; }
    public string?                   Notes           { get; private set; }
    public int?                      UserId          { get; private set; }
    public DateTime                  CreatedAt       { get; private set; } = DateTime.UtcNow;

    public Product?   Product   { get; private set; }
    public Warehouse? Warehouse { get; private set; }
    public User?      User      { get; private set; }

    protected InventoryTransaction() { }

    public static InventoryTransaction Create(int productId, int warehouseId,
        InventoryTransactionType type, decimal quantity, StockDirection direction,
        decimal unitCost, decimal balanceAfter, int? referenceId = null,
        string? referenceType = null, string? notes = null, int? userId = null)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive.");
        return new InventoryTransaction
        {
            ProductId       = productId,
            WarehouseId     = warehouseId,
            TransactionDate = DateTime.Now,
            TransactionType = type,
            Quantity        = quantity,
            Direction       = direction,
            UnitCost        = unitCost,
            TotalCost       = Math.Round(quantity * unitCost, 4),
            BalanceAfter    = balanceAfter,
            ReferenceId     = referenceId,
            ReferenceType   = referenceType,
            Notes           = notes,
            UserId          = userId
        };
    }
}
