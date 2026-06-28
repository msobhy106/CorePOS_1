using CorePOS.Domain.Common;

namespace CorePOS.Domain.Events;

public sealed class SaleCompletedEvent : BaseDomainEvent
{
    public int     InvoiceId   { get; }
    public string  InvoiceNo   { get; }
    public decimal TotalAmount { get; }
    public int?    CustomerId  { get; }

    public SaleCompletedEvent(int invoiceId, string invoiceNo, decimal totalAmount, int? customerId)
    {
        InvoiceId   = invoiceId;
        InvoiceNo   = invoiceNo;
        TotalAmount = totalAmount;
        CustomerId  = customerId;
    }
}
