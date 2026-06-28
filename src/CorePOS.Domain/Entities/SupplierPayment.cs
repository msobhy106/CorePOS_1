using CorePOS.Domain.Common;
using CorePOS.Domain.Enums;

namespace CorePOS.Domain.Entities;

public class SupplierPayment : AuditableEntity
{
    public string        PaymentNo    { get; private set; } = string.Empty;
    public DateOnly      PaymentDate  { get; private set; }
    public int           SupplierId   { get; private set; }
    public int?          CashBoxId    { get; private set; }
    public int?          ShiftId      { get; private set; }
    public int           UserId       { get; private set; }
    public PaymentMethod PaymentMethod{ get; private set; }
    public decimal       Amount       { get; private set; }
    public string?       Notes        { get; private set; }

    public Supplier? Supplier { get; private set; }
    public CashBox?  CashBox  { get; private set; }

    protected SupplierPayment() { }

    public static SupplierPayment Create(string paymentNo, int supplierId,
        decimal amount, PaymentMethod method, int userId,
        int? cashBoxId = null, int? shiftId = null, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(paymentNo)) throw new ArgumentException("Payment number is required.");
        if (amount <= 0) throw new ArgumentException("Amount must be positive.");
        return new SupplierPayment
        {
            PaymentNo = paymentNo.Trim(), PaymentDate = DateOnly.FromDateTime(DateTime.Today),
            SupplierId = supplierId, Amount = amount, PaymentMethod = method,
            UserId = userId, CashBoxId = cashBoxId, ShiftId = shiftId, Notes = notes
        };
    }
}
