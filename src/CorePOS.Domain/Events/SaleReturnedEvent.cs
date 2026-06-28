using CorePOS.Domain.Common;

namespace CorePOS.Domain.Events;

public sealed class SaleReturnedEvent : BaseDomainEvent
{
    public int     ReturnId          { get; }
    public string  ReturnNo          { get; }
    public int     OriginalInvoiceId { get; }
    public decimal TotalAmount       { get; }

    public SaleReturnedEvent(int returnId, string returnNo, int originalInvoiceId, decimal totalAmount)
    {
        ReturnId          = returnId;
        ReturnNo          = returnNo;
        OriginalInvoiceId = originalInvoiceId;
        TotalAmount       = totalAmount;
    }
}
