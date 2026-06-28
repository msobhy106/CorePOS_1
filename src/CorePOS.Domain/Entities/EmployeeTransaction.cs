using CorePOS.Domain.Common;
using CorePOS.Domain.Enums;

namespace CorePOS.Domain.Entities;

public class EmployeeTransaction : BaseEntity
{
    public int                    EmployeeId      { get; private set; }
    public DateOnly               TransactionDate { get; private set; }
    public EmployeeTransactionType Type           { get; private set; }
    public decimal                Amount          { get; private set; }
    public string?                Notes           { get; private set; }
    public int?                   CreatedBy       { get; private set; }
    public DateTime               CreatedAt       { get; private set; } = DateTime.UtcNow;

    public Employee? Employee { get; private set; }

    protected EmployeeTransaction() { }

    public static EmployeeTransaction Create(int employeeId, EmployeeTransactionType type,
        decimal amount, DateOnly date, string? notes = null, int? createdBy = null)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be positive.");
        return new EmployeeTransaction { EmployeeId = employeeId, Type = type,
            Amount = amount, TransactionDate = date, Notes = notes, CreatedBy = createdBy };
    }
}
