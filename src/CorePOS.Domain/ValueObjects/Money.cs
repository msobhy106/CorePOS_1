using CorePOS.Domain.Common;

namespace CorePOS.Domain.ValueObjects;

/// <summary>Represents a monetary amount with currency symbol.</summary>
public sealed class Money : ValueObject
{
    public decimal Amount   { get; }
    public string  Currency { get; }

    private Money() { Amount = 0; Currency = "EGP"; }

    public Money(decimal amount, string currency = "EGP")
    {
        if (amount < 0)
            throw new ArgumentException("Money amount cannot be negative.", nameof(amount));
        Amount   = Math.Round(amount, 4);
        Currency = currency ?? "EGP";
    }

    public static Money Zero(string currency = "EGP") => new(0, currency);

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal factor) => new(Amount * factor, Currency);

    public Money ApplyDiscountPercent(decimal percent)
    {
        if (percent < 0 || percent > 100)
            throw new ArgumentOutOfRangeException(nameof(percent), "Discount must be 0-100.");
        return new Money(Amount * (1 - percent / 100), Currency);
    }

    public Money ApplyTaxPercent(decimal taxPercent)
        => new(Amount * (1 + taxPercent / 100), Currency);

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot operate on different currencies: {Currency} and {other.Currency}");
    }

    public override string ToString() => $"{Amount:N2} {Currency}";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public static implicit operator decimal(Money money) => money.Amount;
}
