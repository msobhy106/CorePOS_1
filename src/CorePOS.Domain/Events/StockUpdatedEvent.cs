using CorePOS.Domain.Common;

namespace CorePOS.Domain.Events;

public sealed class StockUpdatedEvent : BaseDomainEvent
{
    public int     ProductId    { get; }
    public int     WarehouseId  { get; }
    public decimal NewQuantity  { get; }

    public StockUpdatedEvent(int productId, int warehouseId, decimal newQuantity)
    {
        ProductId   = productId;
        WarehouseId = warehouseId;
        NewQuantity = newQuantity;
    }
}
