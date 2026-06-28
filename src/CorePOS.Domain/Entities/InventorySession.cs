using CorePOS.Domain.Common;
using CorePOS.Domain.Enums;

namespace CorePOS.Domain.Entities;

public class InventorySession : AuditableEntity
{
    public string     SessionNo   { get; private set; } = string.Empty;
    public DateTime   SessionDate { get; private set; }
    public int        WarehouseId { get; private set; }
    public ReturnType CountType   { get; private set; }   // Full=0, Partial=1
    public int        Status      { get; private set; }   // 0=Open,1=Approved,2=Cancelled
    public int        UserId      { get; private set; }
    public int?       ApprovedBy  { get; private set; }
    public DateTime?  ApprovedAt  { get; private set; }
    public string?    Notes       { get; private set; }

    public Warehouse? Warehouse { get; private set; }

    private readonly List<InventorySessionItem> _items = [];
    public IReadOnlyCollection<InventorySessionItem> Items => _items.AsReadOnly();

    protected InventorySession() { }

    public static InventorySession Create(string sessionNo, int warehouseId, int userId,
        bool isFull = true, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(sessionNo)) throw new ArgumentException("Session number is required.");
        return new InventorySession
        {
            SessionNo = sessionNo.Trim(), SessionDate = DateTime.Now,
            WarehouseId = warehouseId, UserId = userId,
            CountType = isFull ? ReturnType.Full : ReturnType.Partial,
            Status = 0, Notes = notes
        };
    }

    public InventorySessionItem AddItem(int productId, decimal systemQty, decimal actualQty, decimal unitCost = 0)
    {
        if (Status != 0) throw new InvalidOperationException("Cannot modify an approved/cancelled session.");
        var existing = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existing != null) { existing.UpdateActualQty(actualQty); return existing; }
        var item = InventorySessionItem.Create(productId, systemQty, actualQty, unitCost);
        _items.Add(item);
        return item;
    }

    public void Approve(int approvedBy)
    {
        if (Status != 0) throw new InvalidOperationException("Session is not open.");
        Status = 1; ApprovedBy = approvedBy; ApprovedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel() { Status = 2; UpdatedAt = DateTime.UtcNow; }

    public IReadOnlyList<InventorySessionItem> GetDifferences()
        => _items.Where(i => i.Difference != 0).ToList().AsReadOnly();
}
