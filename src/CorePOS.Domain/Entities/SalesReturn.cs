using CorePOS.Domain.Common;
using CorePOS.Domain.Enums;
using CorePOS.Domain.Events;

namespace CorePOS.Domain.Entities;

public class SalesReturn : AuditableEntity
{
    public string       ReturnNo          { get; private set; } = string.Empty;
    public DateTime     ReturnDate        { get; private set; }
    public int          OriginalInvoiceId { get; private set; }
    public int?         CustomerId        { get; private set; }
    public int          BranchId          { get; private set; }
    public int          WarehouseId       { get; private set; }
    public int          UserId            { get; private set; }
    public int?         ShiftId           { get; private set; }
    public ReturnType   ReturnType        { get; private set; }
    public RefundMethod RefundMethod      { get; private set; }
    public decimal      TotalAmount       { get; private set; }
    public string?      Notes             { get; private set; }

    public SalesInvoice? OriginalInvoice { get; private set; }
    public Customer?     Customer        { get; private set; }

    private readonly List<SalesReturnItem> _items = [];
    public IReadOnlyCollection<SalesReturnItem> Items => _items.AsReadOnly();

    protected SalesReturn() { }

    public static SalesReturn Create(string returnNo, int originalInvoiceId, int branchId,
        int warehouseId, int userId, ReturnType returnType, RefundMethod refundMethod,
        int? customerId = null, int? shiftId = null, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(returnNo)) throw new ArgumentException("Return number is required.");
        return new SalesReturn
        {
            ReturnNo = returnNo.Trim(), ReturnDate = DateTime.Now,
            OriginalInvoiceId = originalInvoiceId, CustomerId = customerId,
            BranchId = branchId, WarehouseId = warehouseId,
            UserId = userId, ShiftId = shiftId,
            ReturnType = returnType, RefundMethod = refundMethod, Notes = notes
        };
    }

    public SalesReturnItem AddItem(int invoiceItemId, int productId, int unitId,
        string productNameAr, decimal quantity, decimal unitPrice)
    {
        var item = SalesReturnItem.Create(invoiceItemId, productId, unitId, productNameAr, quantity, unitPrice);
        _items.Add(item);
        TotalAmount = _items.Sum(i => i.TotalPrice);
        return item;
    }

    public void Confirm()
    {
        if (!_items.Any()) throw new InvalidOperationException("Cannot confirm empty return.");
        AddDomainEvent(new SaleReturnedEvent(Id, ReturnNo, OriginalInvoiceId, TotalAmount));
    }
}
