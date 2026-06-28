using CorePOS.Domain.Common;

namespace CorePOS.Domain.Entities;

public class PriceList : BaseEntity
{
    public string  Name            { get; private set; } = string.Empty;
    public string  NameAr          { get; private set; } = string.Empty;
    public decimal DiscountPercent { get; private set; }
    public bool    IsActive        { get; private set; } = true;

    protected PriceList() { }

    public static PriceList Create(string name, string nameAr, decimal discountPercent = 0)
    {
        if (string.IsNullOrWhiteSpace(name))   throw new ArgumentException("Price list name is required.");
        if (string.IsNullOrWhiteSpace(nameAr)) throw new ArgumentException("Arabic name is required.");
        return new PriceList { Name = name.Trim(), NameAr = nameAr.Trim(), DiscountPercent = discountPercent };
    }
}
