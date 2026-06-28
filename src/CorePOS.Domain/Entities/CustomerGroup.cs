using CorePOS.Domain.Common;

namespace CorePOS.Domain.Entities;

public class CustomerGroup : BaseEntity
{
    public string  Name              { get; private set; } = string.Empty;
    public decimal DiscountPercent   { get; private set; }
    public decimal PointsMultiplier  { get; private set; } = 1;
    public bool    IsActive          { get; private set; } = true;

    protected CustomerGroup() { }

    public static CustomerGroup Create(string name, decimal discountPercent = 0, decimal pointsMultiplier = 1)
    {
        if (string.IsNullOrWhiteSpace(name))              throw new ArgumentException("Group name is required.");
        if (discountPercent < 0 || discountPercent > 100) throw new ArgumentException("Discount must be 0-100.");
        if (pointsMultiplier < 0)                         throw new ArgumentException("Points multiplier cannot be negative.");
        return new CustomerGroup { Name = name.Trim(), DiscountPercent = discountPercent, PointsMultiplier = pointsMultiplier };
    }

    public void Update(string name, decimal discountPercent, decimal pointsMultiplier)
    {
        Name             = name.Trim();
        DiscountPercent  = discountPercent;
        PointsMultiplier = pointsMultiplier;
    }
}
