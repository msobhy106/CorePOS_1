using CorePOS.Domain.Common;

namespace CorePOS.Domain.Entities;

public class ExpenseCategory : BaseEntity
{
    public string Name     { get; private set; } = string.Empty;
    public string NameAr   { get; private set; } = string.Empty;
    public bool   IsSystem { get; private set; }
    public bool   IsActive { get; private set; } = true;

    protected ExpenseCategory() { }

    public static ExpenseCategory Create(string name, string nameAr, bool isSystem = false)
    {
        if (string.IsNullOrWhiteSpace(name))   throw new ArgumentException("Expense category name is required.");
        if (string.IsNullOrWhiteSpace(nameAr)) throw new ArgumentException("Arabic name is required.");
        return new ExpenseCategory { Name = name.Trim(), NameAr = nameAr.Trim(), IsSystem = isSystem };
    }

    public void Update(string name, string nameAr)
    {
        if (IsSystem) throw new InvalidOperationException("System categories cannot be modified.");
        Name  = name.Trim();
        NameAr= nameAr.Trim();
    }
}
