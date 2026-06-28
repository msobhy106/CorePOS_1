using CorePOS.Domain.Common;
using CorePOS.Domain.Enums;

namespace CorePOS.Domain.Entities;

public class PurchaseReturn : AuditableEntity
{
    public string     ReturnNo          { get; private set; } = string.Empty;
    public DateTime   ReturnDate        { get; private set; }
    public int        OriginalInvoiceId { get; private set; }
    public int?       SupplierId        { get; private set; }
    public int        BranchId          { get; private set; }
    public int        WarehouseId       { get; private set; }
    public int        UserId            { get; private set; }
    public ReturnType ReturnType        { get; private set; }
    public decimal    TotalAmount       { get; private set; }
    public string?    Notes             { get; private set; }

    private readonly List<PurchaseReturnItem> _items = [];
    public IReadOnlyCollection<PurchaseReturnItem> Items => _items.AsReadOnly();

    protected PurchaseReturn() { }

    public static PurchaseReturn Create(string returnNo, int originalInvoiceId,
        int branchId, int warehouseId, int userId, ReturnType returnType,
        int? supplierId = null, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(returnNo)) throw new ArgumentException("Return number is required.");
        return new PurchaseReturn
        {
            ReturnNo = returnNo.Trim(), ReturnDate = DateTime.Now,
            OriginalInvoiceId = originalInvoiceId, SupplierId = supplierId,
            BranchId = branchId, WarehouseId = warehouseId,
            UserId = userId, ReturnType = returnType, Notes = notes
        };
    }

    public PurchaseReturnItem AddItem(int invoiceItemId, int productId, int unitId,
        string productNameAr, decimal quantity, decimal unitCost)
    {
        var item = PurchaseReturnItem.Create(invoiceItemId, productId, unitId,
            productNameAr, quantity, unitCost);
        _items.Add(item);
        TotalAmount = _items.Sum(i => i.TotalCost);
        return item;
    }
}
