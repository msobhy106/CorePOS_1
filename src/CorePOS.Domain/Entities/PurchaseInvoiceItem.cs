using CorePOS.Domain.Common;

namespace CorePOS.Domain.Entities;

public class PurchaseInvoiceItem : BaseEntity
{
    public int     InvoiceId       { get; private set; }
    public int     ProductId       { get; private set; }
    public int     UnitId          { get; private set; }
    public string  ProductNameAr   { get; private set; } = string.Empty;
    public decimal Quantity        { get; private set; }
    public decimal UnitCost        { get; private set; }
    public decimal DiscountPercent { get; private set; }
    public decimal DiscountAmount  { get; private set; }
    public decimal TaxPercent      { get; private set; }
    public decimal TaxAmount       { get; private set; }
    public decimal TotalCost       { get; private set; }
    public decimal? SalePriceAfter { get; private set; }
    public decimal ReturnedQty     { get; private set; }

    protected PurchaseInvoiceItem() { }

    public static PurchaseInvoiceItem Create(int productId, int unitId, string productNameAr,
        decimal quantity, decimal unitCost, decimal discountPercent = 0,
        decimal taxPercent = 0, decimal? salePriceAfter = null)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive.");
        if (unitCost  < 0) throw new ArgumentException("Unit cost cannot be negative.");

        var item = new PurchaseInvoiceItem
        {
            ProductId = productId, UnitId = unitId,
            ProductNameAr = productNameAr.Trim(), Quantity = quantity,
            UnitCost = unitCost, DiscountPercent = discountPercent,
            TaxPercent = taxPercent, SalePriceAfter = salePriceAfter
        };
        item.Recalculate();
        return item;
    }

    public void RecordReturn(decimal qty)
    {
        if (qty <= 0 || qty > Quantity - ReturnedQty)
            throw new ArgumentException("Invalid return quantity.");
        ReturnedQty += qty;
    }

    public decimal RemainingQty => Quantity - ReturnedQty;

    private void Recalculate()
    {
        DiscountAmount = DiscountPercent > 0 ? Math.Round(Quantity * UnitCost * DiscountPercent / 100, 4) : 0;
        var afterDiscount = Quantity * UnitCost - DiscountAmount;
        TaxAmount  = TaxPercent > 0 ? Math.Round(afterDiscount * TaxPercent / 100, 4) : 0;
        TotalCost  = afterDiscount + TaxAmount;
    }
}
