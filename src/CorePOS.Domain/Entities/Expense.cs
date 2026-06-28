using CorePOS.Domain.Common;

namespace CorePOS.Domain.Entities;

public class Expense : AuditableEntity
{
    public string   ExpenseNo   { get; private set; } = string.Empty;
    public DateOnly ExpenseDate { get; private set; }
    public int      CategoryId  { get; private set; }
    public int      BranchId    { get; private set; }
    public int?     CashBoxId   { get; private set; }
    public int?     ShiftId     { get; private set; }
    public int      UserId      { get; private set; }
    public decimal  Amount      { get; private set; }
    public string?  Description { get; private set; }

    public ExpenseCategory? Category { get; private set; }
    public Branch?          Branch   { get; private set; }
    public CashBox?         CashBox  { get; private set; }
    public User?            User     { get; private set; }

    protected Expense() { }

    public static Expense Create(string expenseNo, DateOnly date, int categoryId,
        int branchId, decimal amount, int userId, int? cashBoxId = null,
        int? shiftId = null, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(expenseNo)) throw new ArgumentException("Expense number is required.");
        if (amount <= 0) throw new ArgumentException("Amount must be positive.");
        return new Expense
        {
            ExpenseNo = expenseNo.Trim(), ExpenseDate = date,
            CategoryId = categoryId, BranchId = branchId,
            Amount = amount, UserId = userId,
            CashBoxId = cashBoxId, ShiftId = shiftId, Description = description
        };
    }
}
