using CorePOS.Domain.Common;

namespace CorePOS.Domain.Entities;

public class Customer : AuditableEntity
{
    public string  Code              { get; private set; } = string.Empty;
    public string  Name              { get; private set; } = string.Empty;
    public string? Phone             { get; private set; }
    public string? Phone2            { get; private set; }
    public string? Address           { get; private set; }
    public string? Email             { get; private set; }
    public string? InstapayNumber    { get; private set; }
    public string? TaxNumber         { get; private set; }
    public int?    BranchId          { get; private set; }
    public int?    GroupId           { get; private set; }
    public int?    PriceListId       { get; private set; }
    public decimal OpeningBalance    { get; private set; }
    public decimal CreditLimit       { get; private set; }
    public int     PaymentPeriodDays { get; private set; }
    public decimal CurrentBalance    { get; private set; }
    public decimal TotalPoints       { get; private set; }
    public bool    IsActive          { get; private set; } = true;

    public CustomerGroup? Group     { get; private set; }
    public PriceList?     PriceList { get; private set; }

    protected Customer() { }

    public static Customer Create(string code, string name, string? phone = null,
        string? address = null, int? groupId = null, int? priceListId = null,
        decimal creditLimit = 0, int paymentPeriodDays = 0)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Customer code is required.");
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Customer name is required.");
        if (creditLimit < 0)                 throw new ArgumentException("Credit limit cannot be negative.");

        return new Customer
        {
            Code = code.Trim().ToUpperInvariant(), Name = name.Trim(),
            Phone = phone?.Trim(), Address = address,
            GroupId = groupId, PriceListId = priceListId,
            CreditLimit = creditLimit, PaymentPeriodDays = paymentPeriodDays
        };
    }

    public void UpdateContact(string name, string? phone, string? phone2, string? address,
        string? email, string? instapayNumber, string? taxNumber)
    {
        Name           = name.Trim();
        Phone          = phone?.Trim();
        Phone2         = phone2?.Trim();
        Address        = address;
        Email          = email?.Trim().ToLowerInvariant();
        InstapayNumber = instapayNumber?.Trim();
        TaxNumber      = taxNumber?.Trim();
        UpdatedAt      = DateTime.UtcNow;
    }

    public void UpdateCredit(decimal creditLimit, int paymentPeriodDays)
    {
        if (creditLimit < 0) throw new ArgumentException("Credit limit cannot be negative.");
        CreditLimit       = creditLimit;
        PaymentPeriodDays = paymentPeriodDays;
        UpdatedAt         = DateTime.UtcNow;
    }

    /// <summary>Adds to balance (debit — customer owes us more).</summary>
    public void AddDebt(decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be positive.");
        CurrentBalance += amount;
        UpdatedAt       = DateTime.UtcNow;
    }

    /// <summary>Reduces balance (payment received).</summary>
    public void ReduceDebt(decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be positive.");
        CurrentBalance -= amount;
        UpdatedAt       = DateTime.UtcNow;
    }

    public void EarnPoints(decimal points)   { TotalPoints += points; UpdatedAt = DateTime.UtcNow; }
    public void RedeemPoints(decimal points)
    {
        if (points > TotalPoints) throw new InvalidOperationException("Insufficient loyalty points.");
        TotalPoints -= points;
        UpdatedAt    = DateTime.UtcNow;
    }

    public bool IsOverCreditLimit() => CreditLimit > 0 && CurrentBalance > CreditLimit;

    public void Activate()   { IsActive = true;  UpdatedAt = DateTime.UtcNow; }
    public void Deactivate() { IsActive = false; UpdatedAt = DateTime.UtcNow; }

    public override string ToString() => $"[{Code}] {Name}";
}
