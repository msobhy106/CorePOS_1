using CorePOS.Domain.Common;

namespace CorePOS.Domain.Entities;

public class WarehouseTransfer : AuditableEntity
{
    public string    TransferNo      { get; private set; } = string.Empty;
    public DateTime  TransferDate    { get; private set; }
    public int       FromWarehouseId { get; private set; }
    public int       ToWarehouseId   { get; private set; }
    public int       FromBranchId    { get; private set; }
    public int       ToBranchId      { get; private set; }
    public int       Status          { get; private set; }  // 0=Draft,1=Approved,2=Cancelled
    public int       UserId          { get; private set; }
    public string?   Notes           { get; private set; }
    public DateTime? ApprovedAt      { get; private set; }

    public Warehouse? FromWarehouse { get; private set; }
    public Warehouse? ToWarehouse   { get; private set; }

    private readonly List<WarehouseTransferItem> _items = [];
    public IReadOnlyCollection<WarehouseTransferItem> Items => _items.AsReadOnly();

    protected WarehouseTransfer() { }

    public static WarehouseTransfer Create(string transferNo, int fromWarehouseId, int toWarehouseId,
        int fromBranchId, int toBranchId, int userId, string? notes = null)
    {
        if (fromWarehouseId == toWarehouseId)
            throw new ArgumentException("Source and destination warehouses must be different.");
        return new WarehouseTransfer
        {
            TransferNo = transferNo.Trim(), TransferDate = DateTime.Now,
            FromWarehouseId = fromWarehouseId, ToWarehouseId = toWarehouseId,
            FromBranchId = fromBranchId, ToBranchId = toBranchId,
            UserId = userId, Notes = notes, Status = 0
        };
    }

    public WarehouseTransferItem AddItem(int productId, string productNameAr, decimal quantity, decimal unitCost = 0)
    {
        if (Status != 0) throw new InvalidOperationException("Cannot modify approved/cancelled transfer.");
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive.");
        var item = WarehouseTransferItem.Create(productId, productNameAr, quantity, unitCost);
        _items.Add(item);
        return item;
    }

    public void Approve() { Status = 1; ApprovedAt = DateTime.UtcNow; UpdatedAt = DateTime.UtcNow; }
    public void Cancel()  { Status = 2; UpdatedAt = DateTime.UtcNow; }

    public bool IsDraft    => Status == 0;
    public bool IsApproved => Status == 1;
}
