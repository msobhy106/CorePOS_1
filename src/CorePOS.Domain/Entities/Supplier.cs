using CorePOS.Domain.Common;

namespace CorePOS.Domain.Entities;

public class Supplier : AuditableEntity
{
    public string  Code              { get; private set; } = string.Empty;
    public string  Name              { get; private set; } = string.Empty;
    public string? Phone             { get; private set; }
    public string? Phone2            { get; private set; }
    public string? Address           { get; private set; }
    public string? Email             { get; private set; }
    public string? TaxNumber         { get; private set; }
    public string? ContactPerson     { get; private set; }
    public decimal OpeningBalance    { get; private set; }
    public decimal CurrentBalance    { get; private set; }
    public decimal CreditLimit       { get; private set; }
    public int     PaymentPeriodDays { get; private set; }
    public bool    IsActive          { get; private set; } = true;

    protected Supplier() { }

    public static Supplier Create(string code, string name, string? phone = null,
        string? contactPerson = null, decimal creditLimit = 0)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Supplier code is required.");
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Supplier name is required.");
        return new Supplier { Code = code.Trim().ToUpperInvariant(), Name = name.Trim(),
                              Phone = phone?.Trim(), ContactPerson = contactPerson?.Trim(),
                              CreditLimit = creditLimit };
    }

    public void UpdateContact(string name, string? phone, string? phone2, string? address,
        string? email, string? taxNumber, string? contactPerson)
    {
        Name          = name.Trim();
        Phone         = phone?.Trim();
        Phone2        = phone2?.Trim();
        Address       = address;
        Email         = email?.Trim().ToLowerInvariant();
        TaxNumber     = taxNumber?.Trim();
        ContactPerson = contactPerson?.Trim();
        UpdatedAt     = DateTime.UtcNow;
    }

    /// <summary>We owe supplier more (purchase approved).</summary>
    public void AddPayable(decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be positive.");
        CurrentBalance += amount;
        UpdatedAt       = DateTime.UtcNow;
    }

    /// <summary>We paid the supplier.</summary>
    public void ReducePayable(decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be positive.");
        CurrentBalance -= amount;
        UpdatedAt       = DateTime.UtcNow;
    }

    public void Activate()   { IsActive = true;  UpdatedAt = DateTime.UtcNow; }
    public void Deactivate() { IsActive = false; UpdatedAt = DateTime.UtcNow; }

    public override string ToString() => $"[{Code}] {Name}";
}
