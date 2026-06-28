using CorePOS.Domain.Common;

namespace CorePOS.Domain.Entities;

public class SalesInvoiceItem : BaseEntity
{
    public int     InvoiceId       { get; private set; }
    public int     ProductId       { get; private set; }
    public int     UnitId          { get; private set; }
    public string? Barcode         { get; private set; }
    public string  ProductNameAr   { get; private set; } = string.Empty;
    public decimal Quantity        { get; private set; }
    public decimal UnitPrice       { get; private set; }
    public decimal PurchasePrice   { get; private set; }
    public decimal DiscountPercent { get; private set; }
    public decimal DiscountAmount  { get; private set; }
    public decimal TaxPercent      { get; private set; }
    public decimal TaxAmount       { get; private set; }
    public decimal TotalPrice      { get; private set; }
    public decimal ReturnedQty     { get; private set; }
    public int     SortOrder       { get; private set; }

    public SalesInvoice? Invoice  { get; private set; }
    public Product?      Product  { get; private set; }
    public Unit?         Unit     { get; private set; }

    protected SalesInvoiceItem() { }

    public static SalesInvoiceItem Create(int productId, int unitId, string productNameAr,
        string? barcode, decimal quantity, decimal unitPrice, decimal purchasePrice,
        decimal discountPercent = 0, decimal taxPercent = 0)
    {
        if (quantity  <= 0) throw new ArgumentException("Quantity must be positive.");
        if (unitPrice < 0)  throw new ArgumentException("Unit price cannot be negative.");

        var item = new SalesInvoiceItem
        {
            ProductId = productId, UnitId = unitId,
            ProductNameAr = productNameAr.Trim(), Barcode = barcode?.Trim(),
            Quantity = quantity, UnitPrice = unitPrice, PurchasePrice = purchasePrice,
            DiscountPercent = discountPercent, TaxPercent = taxPercent
        };
        item.Recalculate();
        return item;
    }

    public void AddQuantity(decimal qty)
    {
        if (qty <= 0) throw new ArgumentException("Quantity must be positive.");
        Quantity += qty;
        Recalculate();
    }

    public void UpdateQuantityAndPrice(decimal quantity, decimal unitPrice, decimal discountPercent)
    {
        if (quantity  <= 0) throw new ArgumentException("Quantity must be positive.");
        if (unitPrice < 0)  throw new ArgumentException("Unit price cannot be negative.");
        Quantity        = quantity;
        UnitPrice       = unitPrice;
        DiscountPercent = discountPercent;
        Recalculate();
    }

    public void RecordReturn(decimal qty)
    {
        if (qty <= 0 || qty > Quantity - ReturnedQty)
            throw new ArgumentException("Invalid return quantity.");
        ReturnedQty += qty;
    }

    public decimal RemainingQty => Quantity - ReturnedQty;
    public decimal ProfitOnItem => TotalPrice - (Quantity * PurchasePrice);

    private void Recalculate()
    {
        DiscountAmount = DiscountPercent > 0
            ? Math.Round(Quantity * UnitPrice * DiscountPercent / 100, 4)
            : 0;
        var afterDiscount = Quantity * UnitPrice - DiscountAmount;
        TaxAmount  = TaxPercent > 0 ? Math.Round(afterDiscount * TaxPercent / 100, 4) : 0;
        TotalPrice = afterDiscount + TaxAmount;
    }
}
