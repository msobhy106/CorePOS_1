using CorePOS.Domain.Common;

namespace CorePOS.Domain.Entities;

public class Unit : BaseEntity
{
    public string  Code         { get; private set; } = string.Empty;
    public string  Name         { get; private set; } = string.Empty;
    public string  NameAr       { get; private set; } = string.Empty;
    public string? Abbreviation { get; private set; }
    public bool    IsActive     { get; private set; } = true;

    protected Unit() { }

    public static Unit Create(string code, string name, string nameAr, string? abbreviation = null)
    {
        if (string.IsNullOrWhiteSpace(code))  throw new ArgumentException("Unit code is required.");
        if (string.IsNullOrWhiteSpace(name))  throw new ArgumentException("Unit name is required.");
        if (string.IsNullOrWhiteSpace(nameAr))throw new ArgumentException("Arabic unit name is required.");
        return new Unit { Code = code.Trim().ToUpperInvariant(), Name = name.Trim(),
                          NameAr = nameAr.Trim(), Abbreviation = abbreviation?.Trim() };
    }

    public void Update(string name, string nameAr, string? abbreviation)
    {
        Name = name.Trim(); NameAr = nameAr.Trim(); Abbreviation = abbreviation?.Trim();
    }

    public void Activate()   => IsActive = true;
    public void Deactivate() => IsActive = false;

    public override string ToString() => NameAr;
}
