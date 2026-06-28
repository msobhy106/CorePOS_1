using CorePOS.Domain.Common;

namespace CorePOS.Domain.Entities;

public class Product : AuditableEntity
{
    public string   Code                 { get; private set; } = string.Empty;
    public string?  Barcode              { get; private set; }
    public string?  SecondBarcode        { get; private set; }
    public string   NameAr               { get; private set; } = string.Empty;
    public string?  NameEn               { get; private set; }
    public int      CategoryId           { get; private set; }
    public int      BaseUnitId           { get; private set; }
    public int      SaleUnitId           { get; private set; }
    public int      PurchaseUnitId       { get; private set; }
    public int?     DefaultSupplierId    { get; private set; }

    // Prices
    public decimal PurchasePrice        { get; private set; }
    public decimal SalePrice            { get; private set; }
    public decimal WholesalePrice       { get; private set; }
    public decimal HalfWholesalePrice   { get; private set; }
    public decimal SpecialPrice         { get; private set; }
    public decimal TaxPercent           { get; private set; }

    // Stock thresholds
    public decimal MinStock             { get; private set; }
    public decimal ReorderLevel         { get; private set; }

    // Additional
    public DateOnly? ExpiryDate         { get; private set; }
    public bool      HasExpiry          { get; private set; }
    public string?   Manufacturer       { get; private set; }
    public string?   Description        { get; private set; }
    public bool      IsActive           { get; private set; } = true;

    // Navigation
    public Category?  Category  { get; private set; }
    public Unit?      BaseUnit  { get; private set; }
    public Unit?      SaleUnit  { get; private set; }
    public Unit?      PurchaseUnit { get; private set; }
    public Supplier?  DefaultSupplier { get; private set; }

    private readonly List<ProductUnit>  _units  = [];
    private readonly List<ProductImage> _images = [];
    private readonly List<ProductStock> _stock  = [];

    public IReadOnlyCollection<ProductUnit>  Units  => _units.AsReadOnly();
    public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();
    public IReadOnlyCollection<ProductStock> Stock  => _stock.AsReadOnly();

    protected Product() { }

    public static Product Create(string code, string nameAr, int categoryId,
        int baseUnitId, int saleUnitId, int purchaseUnitId,
        decimal purchasePrice, decimal salePrice,
        string? barcode = null, string? nameEn = null)
    {
        if (string.IsNullOrWhiteSpace(code))   throw new ArgumentException("Product code is required.");
        if (string.IsNullOrWhiteSpace(nameAr)) throw new ArgumentException("Arabic product name is required.");
        if (purchasePrice < 0) throw new ArgumentException("Purchase price cannot be negative.");
        if (salePrice < 0)     throw new ArgumentException("Sale price cannot be negative.");

        return new Product
        {
            Code = code.Trim().ToUpperInvariant(), NameAr = nameAr.Trim(), NameEn = nameEn?.Trim(),
            CategoryId = categoryId, BaseUnitId = baseUnitId,
            SaleUnitId = saleUnitId, PurchaseUnitId = purchaseUnitId,
            Barcode = barcode?.Trim(), PurchasePrice = purchasePrice, SalePrice = salePrice
        };
    }

    public void UpdatePrices(decimal purchasePrice, decimal salePrice,
        decimal wholesalePrice = 0, decimal halfWholesalePrice = 0,
        decimal specialPrice = 0, decimal taxPercent = 0)
    {
        if (purchasePrice < 0 || salePrice < 0)
            throw new ArgumentException("Prices cannot be negative.");
        if (taxPercent < 0 || taxPercent > 100)
            throw new ArgumentException("Tax percent must be 0-100.");

        PurchasePrice      = purchasePrice;
        SalePrice          = salePrice;
        WholesalePrice     = wholesalePrice;
        HalfWholesalePrice = halfWholesalePrice;
        SpecialPrice       = specialPrice;
        TaxPercent         = taxPercent;
        UpdatedAt          = DateTime.UtcNow;
    }

    public void UpdateDetails(string nameAr, string? nameEn, int categoryId,
        string? barcode, string? secondBarcode, string? manufacturer,
        string? description, int? defaultSupplierId)
    {
        NameAr            = nameAr.Trim();
        NameEn            = nameEn?.Trim();
        CategoryId        = categoryId;
        Barcode           = barcode?.Trim();
        SecondBarcode     = secondBarcode?.Trim();
        Manufacturer      = manufacturer?.Trim();
        Description       = description;
        DefaultSupplierId = defaultSupplierId;
        UpdatedAt         = DateTime.UtcNow;
    }

    public void SetStockThresholds(decimal minStock, decimal reorderLevel)
    {
        if (minStock < 0 || reorderLevel < 0)
            throw new ArgumentException("Stock thresholds cannot be negative.");
        MinStock     = minStock;
        ReorderLevel = reorderLevel;
        UpdatedAt    = DateTime.UtcNow;
    }

    public void SetExpiry(DateOnly? expiryDate)
    {
        ExpiryDate = expiryDate;
        HasExpiry  = expiryDate.HasValue;
        UpdatedAt  = DateTime.UtcNow;
    }

    /// <summary>Returns the correct price based on invoice type.</summary>
    public decimal GetPriceForInvoiceType(Enums.InvoiceType invoiceType) => invoiceType switch
    {
        Enums.InvoiceType.Wholesale     => WholesalePrice     > 0 ? WholesalePrice     : SalePrice,
        Enums.InvoiceType.HalfWholesale => HalfWholesalePrice > 0 ? HalfWholesalePrice : SalePrice,
        Enums.InvoiceType.Special       => SpecialPrice       > 0 ? SpecialPrice       : SalePrice,
        _                               => SalePrice
    };

    public decimal CalculateProfitMargin()
        => SalePrice > 0
            ? Math.Round((SalePrice - PurchasePrice) / SalePrice * 100, 2)
            : 0;

    public void Activate()   { IsActive = true;  UpdatedAt = DateTime.UtcNow; }
    public void Deactivate() { IsActive = false; UpdatedAt = DateTime.UtcNow; }

    public override string ToString() => $"[{Code}] {NameAr}";
}
