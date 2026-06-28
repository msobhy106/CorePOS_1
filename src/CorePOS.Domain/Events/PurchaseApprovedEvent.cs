using CorePOS.Domain.Common;

namespace CorePOS.Domain.Events;

public sealed class PurchaseApprovedEvent : BaseDomainEvent
{
    public int     InvoiceId   { get; }
    public string  InvoiceNo   { get; }
    public int?    SupplierId  { get; }
    public decimal TotalAmount { get; }

    public PurchaseApprovedEvent(int invoiceId, string invoiceNo, int? supplierId, decimal totalAmount)
    {
        InvoiceId   = invoiceId;
        InvoiceNo   = invoiceNo;
        SupplierId  = supplierId;
        TotalAmount = totalAmount;
    }
}
