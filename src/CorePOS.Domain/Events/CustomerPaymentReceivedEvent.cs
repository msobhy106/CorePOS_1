using CorePOS.Domain.Common;

namespace CorePOS.Domain.Events;

public sealed class CustomerPaymentReceivedEvent : BaseDomainEvent
{
    public int     PaymentId   { get; }
    public string  PaymentNo   { get; }
    public int     CustomerId  { get; }
    public decimal Amount      { get; }

    public CustomerPaymentReceivedEvent(int paymentId, string paymentNo, int customerId, decimal amount)
    {
        PaymentId  = paymentId;
        PaymentNo  = paymentNo;
        CustomerId = customerId;
        Amount     = amount;
    }
}
