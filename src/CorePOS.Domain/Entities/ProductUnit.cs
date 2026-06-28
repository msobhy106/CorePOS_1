using CorePOS.Domain.Common;

namespace CorePOS.Domain.Entities;

public class ProductUnit : BaseEntity
{
    public int     ProductId        { get; private set; }
    public int     UnitId           { get; private set; }
    public decimal ConversionFactor { get; private set; } = 1;
    public string? Barcode          { get; private set; }

    public Product? Product { get; private set; }
    public Unit?    Unit    { get; private set; }

    protected ProductUnit() { }

    public static ProductUnit Create(int productId, int unitId, decimal conversionFactor, string? barcode = null)
    {
        if (conversionFactor <= 0) throw new ArgumentException("Conversion factor must be positive.");
        return new ProductUnit { ProductId = productId, UnitId = unitId,
                                 ConversionFactor = conversionFactor, Barcode = barcode?.Trim() };
    }

    public void UpdateConversion(decimal factor) => ConversionFactor = factor > 0
        ? factor : throw new ArgumentException("Conversion factor must be positive.");
}
