using CorePOS.Domain.Common;
using CorePOS.Domain.Enums;

namespace CorePOS.Domain.Entities;

public class LoyaltyPoint : BaseEntity
{
    public int                    CustomerId      { get; private set; }
    public DateTime               TransactionDate { get; private set; }
    public decimal                Points          { get; private set; }
    public LoyaltyTransactionType TransactionType { get; private set; }
    public int?                   ReferenceId     { get; private set; }
    public string?                ReferenceType   { get; private set; }
    public string?                Notes           { get; private set; }
    public int?                   CreatedBy       { get; private set; }

    public Customer? Customer { get; private set; }

    protected LoyaltyPoint() { }

    public static LoyaltyPoint Create(int customerId, decimal points,
        LoyaltyTransactionType type, int? referenceId = null,
        string? referenceType = null, string? notes = null, int? createdBy = null)
    {
        return new LoyaltyPoint
        {
            CustomerId = customerId, Points = points,
            TransactionDate = DateTime.Now, TransactionType = type,
            ReferenceId = referenceId, ReferenceType = referenceType,
            Notes = notes, CreatedBy = createdBy
        };
    }
}
