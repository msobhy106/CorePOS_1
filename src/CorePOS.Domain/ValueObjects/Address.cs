using CorePOS.Domain.Common;

namespace CorePOS.Domain.ValueObjects;

public sealed class Address : ValueObject
{
    public string? Street { get; }
    public string? City   { get; }
    public string? Region { get; }
    public string? Country{ get; }

    private Address() { }

    public Address(string? street, string? city = null, string? region = null, string? country = "Egypt")
    {
        Street  = street;
        City    = city;
        Region  = region;
        Country = country;
    }

    public override string ToString()
        => string.Join(", ", new[] { Street, City, Region, Country }
            .Where(s => !string.IsNullOrWhiteSpace(s)));

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return Region;
        yield return Country;
    }
}
