namespace CorePOS.Application.Features.Products.DTOs;

public class ProductDto
{
    public int     Id                  { get; set; }
    public string  Code                { get; set; } = string.Empty;
    public string? Barcode             { get; set; }
    public string? SecondBarcode       { get; set; }
    public string  NameAr              { get; set; } = string.Empty;
    public string? NameEn              { get; set; }
    public int     CategoryId          { get; set; }
    public string  CategoryName        { get; set; } = string.Empty;
    public int     BaseUnitId          { get; set; }
    public string  BaseUnitName        { get; set; } = string.Empty;
    public int     SaleUnitId          { get; set; }
    public string  SaleUnitName        { get; set; } = string.Empty;
    public int     PurchaseUnitId      { get; set; }
    public decimal PurchasePrice       { get; set; }
    public decimal SalePrice           { get; set; }
    public decimal WholesalePrice      { get; set; }
    public decimal HalfWholesalePrice  { get; set; }
    public decimal SpecialPrice        { get; set; }
    public decimal TaxPercent          { get; set; }
    public decimal MinStock            { get; set; }
    public decimal ReorderLevel        { get; set; }
    public DateOnly? ExpiryDate        { get; set; }
    public bool    HasExpiry           { get; set; }
    public string? Manufacturer        { get; set; }
    public string? Description         { get; set; }
    public string? ImagePath           { get; set; }
    public bool    IsActive            { get; set; }
    public decimal CurrentStock        { get; set; }   // from ProductStock
    public decimal AverageCost         { get; set; }
    public decimal ProfitMargin        { get; set; }
}

public class ProductListDto
{
    public int     Id           { get; set; }
    public string  Code         { get; set; } = string.Empty;
    public string? Barcode      { get; set; }
    public string  NameAr       { get; set; } = string.Empty;
    public string  CategoryName { get; set; } = string.Empty;
    public string  UnitName     { get; set; } = string.Empty;
    public decimal SalePrice    { get; set; }
    public decimal CurrentStock { get; set; }
    public bool    IsActive     { get; set; }
    public bool    IsLowStock   { get; set; }
    public string? ImagePath    { get; set; }
}

public class ProductSearchResultDto
{
    public int     Id           { get; set; }
    public string  Code         { get; set; } = string.Empty;
    public string? Barcode      { get; set; }
    public string  NameAr       { get; set; } = string.Empty;
    public string  UnitName     { get; set; } = string.Empty;
    public decimal SalePrice    { get; set; }
    public decimal WholesalePrice { get; set; }
    public decimal HalfWholesalePrice { get; set; }
    public decimal CurrentStock { get; set; }
    public decimal TaxPercent   { get; set; }
    public string? ImagePath    { get; set; }
    public int     SaleUnitId   { get; set; }
}
