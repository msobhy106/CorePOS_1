using CorePOS.Domain.Common;

namespace CorePOS.Domain.Entities;

public class CashBox : BaseEntity
{
    public string  Code           { get; private set; } = string.Empty;
    public string  Name           { get; private set; } = string.Empty;
    public string  NameAr         { get; private set; } = string.Empty;
    public int     BranchId       { get; private set; }
    public bool    IsMain         { get; private set; }
    public decimal OpeningBalance { get; private set; }
    public decimal CurrentBalance { get; private set; }
    public bool    IsActive       { get; private set; } = true;

    public Branch? Branch { get; private set; }

    protected CashBox() { }

    public static CashBox Create(string code, string name, string nameAr, int branchId,
        bool isMain = false, decimal openingBalance = 0)
    {
        if (string.IsNullOrWhiteSpace(code))   throw new ArgumentException("CashBox code is required.");
        if (string.IsNullOrWhiteSpace(name))   throw new ArgumentException("CashBox name is required.");
        if (string.IsNullOrWhiteSpace(nameAr)) throw new ArgumentException("Arabic CashBox name is required.");
        return new CashBox { Code = code.Trim().ToUpperInvariant(), Name = name.Trim(),
                             NameAr = nameAr.Trim(), BranchId = branchId,
                             IsMain = isMain, OpeningBalance = openingBalance,
                             CurrentBalance = openingBalance };
    }

    public void Deposit(decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("Deposit amount must be positive.");
        CurrentBalance += amount;
    }

    public void Withdraw(decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("Withdrawal amount must be positive.");
        if (amount > CurrentBalance) throw new InvalidOperationException("Insufficient balance.");
        CurrentBalance -= amount;
    }

    public void AdjustBalance(decimal newBalance) => CurrentBalance = newBalance;

    public void Activate()   => IsActive = true;
    public void Deactivate() => IsActive = false;
}
