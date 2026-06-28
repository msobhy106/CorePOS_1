using CorePOS.Domain.Common;
using CorePOS.Domain.Enums;

namespace CorePOS.Domain.Entities;

public class Employee : AuditableEntity
{
    public string   Code       { get; private set; } = string.Empty;
    public string   Name       { get; private set; } = string.Empty;
    public string?  JobTitle   { get; private set; }
    public string?  Phone      { get; private set; }
    public string?  Address    { get; private set; }
    public decimal  Salary     { get; private set; }
    public DateOnly? HireDate  { get; private set; }
    public int?     BranchId   { get; private set; }
    public bool     IsActive   { get; private set; } = true;

    public Branch? Branch { get; private set; }

    private readonly List<EmployeeTransaction> _transactions = [];
    public IReadOnlyCollection<EmployeeTransaction> Transactions => _transactions.AsReadOnly();

    protected Employee() { }

    public static Employee Create(string code, string name, string? jobTitle = null,
        string? phone = null, decimal salary = 0, int? branchId = null)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Employee code is required.");
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Employee name is required.");
        if (salary < 0)                      throw new ArgumentException("Salary cannot be negative.");
        return new Employee { Code = code.Trim().ToUpperInvariant(), Name = name.Trim(),
                              JobTitle = jobTitle?.Trim(), Phone = phone?.Trim(),
                              Salary = salary, BranchId = branchId };
    }

    public EmployeeTransaction AddTransaction(EmployeeTransactionType type, decimal amount,
        DateOnly date, string? notes = null, int? createdBy = null)
    {
        if (amount <= 0) throw new ArgumentException("Transaction amount must be positive.");
        var tx = EmployeeTransaction.Create(Id, type, amount, date, notes, createdBy);
        _transactions.Add(tx);
        return tx;
    }

    public void Update(string name, string? jobTitle, string? phone, string? address, decimal salary)
    {
        Name     = name.Trim();
        JobTitle = jobTitle?.Trim();
        Phone    = phone?.Trim();
        Address  = address;
        Salary   = salary >= 0 ? salary : throw new ArgumentException("Salary cannot be negative.");
        UpdatedAt= DateTime.UtcNow;
    }

    public void Activate()   { IsActive = true;  UpdatedAt = DateTime.UtcNow; }
    public void Deactivate() { IsActive = false; UpdatedAt = DateTime.UtcNow; }
}
