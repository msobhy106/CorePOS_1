using CorePOS.Domain.Common;

namespace CorePOS.Domain.ValueObjects;

/// <summary>Invoice number value object — ensures format consistency.</summary>
public sealed class InvoiceNumber : ValueObject
{
    public string Value { get; }

    private InvoiceNumber() { Value = string.Empty; }

    public InvoiceNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Invoice number cannot be empty.", nameof(value));
        Value = value.Trim().ToUpperInvariant();
    }

    public static InvoiceNumber Create(string prefix, string branchCode, DateTime date, int sequence)
        => new($"{prefix}-{branchCode}-{date:yyyyMMdd}-{sequence:D5}");

    public override string ToString() => Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator string(InvoiceNumber inv) => inv.Value;
}
