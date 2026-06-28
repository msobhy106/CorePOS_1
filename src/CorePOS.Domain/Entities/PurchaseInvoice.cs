using CorePOS.Domain.Common;
using CorePOS.Domain.Enums;
using CorePOS.Domain.Events;

namespace CorePOS.Domain.Entities;

public class PurchaseInvoice : AuditableEntity
{
    public string                 InvoiceNo         { get; private set; } = string.Empty;
    public string?                SupplierInvoiceNo { get; private set; }
    public DateTime               InvoiceDate       { get; private set; }
    public int?                   SupplierId        { get; private set; }
    public int                    BranchId          { get; private set; }
    public int                    WarehouseId       { get; private set; }
    public int                    UserId            { get; private set; }
    public PurchaseInvoiceStatus  Status            { get; private set; } = PurchaseInvoiceStatus.Draft;
    public PaymentMethod          PaymentMethod     { get; private set; } = PaymentMethod.Cash;

    public decimal Subtotal         { get; private set; }
    public decimal DiscountPercent  { get; private set; }
    public decimal DiscountAmount   { get; private set; }
    public decimal TaxPercent       { get; private set; }
    public decimal TaxAmount        { get; private set; }
    public decimal TotalAmount      { get; private set; }
    public decimal PaidAmount       { get; private set; }
    public decimal RemainingAmount  { get; private set; }
    public string? Notes            { get; private set; }
    public DateTime? ApprovedAt     { get; private set; }
    public int?      ApprovedBy     { get; private set; }

    public Supplier?  Supplier  { get; private set; }
    public Branch?    Branch    { get; private set; }
    public Warehouse? Warehouse { get; private set; }
    public User?      User      { get; private set; }

    private readonly List<PurchaseInvoiceItem> _items = [];
    public IReadOnlyCollection<PurchaseInvoiceItem> Items => _items.AsReadOnly();

    protected PurchaseInvoice() { }

    public static PurchaseInvoice Create(string invoiceNo, int branchId, int warehouseId,
        int userId, int? supplierId = null, string? supplierInvoiceNo = null)
    {
        if (string.IsNullOrWhiteSpace(invoiceNo)) throw new ArgumentException("Invoice number is required.");
        return new PurchaseInvoice
        {
            InvoiceNo = invoiceNo.Trim(), InvoiceDate = DateTime.Now,
            BranchId = branchId, WarehouseId = warehouseId,
            UserId = userId, SupplierId = supplierId,
            SupplierInvoiceNo = supplierInvoiceNo?.Trim(),
            Status = PurchaseInvoiceStatus.Draft
        };
    }

    public PurchaseInvoiceItem AddItem(int productId, int unitId, string productNameAr,
        decimal quantity, decimal unitCost, decimal discountPercent = 0, decimal taxPercent = 0,
        decimal? salePriceAfter = null)
    {
        if (Status != PurchaseInvoiceStatus.Draft)
            throw new InvalidOperationException("Cannot modify an approved purchase invoice.");

        var item = PurchaseInvoiceItem.Create(productId, unitId, productNameAr,
            quantity, unitCost, discountPercent, taxPercent, salePriceAfter);
        _items.Add(item);
        RecalculateTotals();
        return item;
    }

    public void RemoveItem(int productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId)
            ?? throw new InvalidOperationException("Item not found.");
        _items.Remove(item);
        RecalculateTotals();
    }

    public void ApplyDiscount(decimal discountPercent)
    {
        DiscountPercent = discountPercent;
        RecalculateTotals();
    }

    public void SetPayment(PaymentMethod method, decimal paidAmount)
    {
        PaymentMethod   = method;
        PaidAmount      = method == PaymentMethod.Cash ? TotalAmount : paidAmount;
        RemainingAmount = Math.Max(0, TotalAmount - PaidAmount);
        UpdatedAt       = DateTime.UtcNow;
    }

    public void Approve(int approvedBy)
    {
        if (Status != PurchaseInvoiceStatus.Draft)
            throw new InvalidOperationException("Only draft invoices can be approved.");
        if (!_items.Any())
            throw new InvalidOperationException("Cannot approve empty purchase invoice.");

        Status     = PurchaseInvoiceStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        ApprovedBy = approvedBy;
        UpdatedAt  = DateTime.UtcNow;
        AddDomainEvent(new PurchaseApprovedEvent(Id, InvoiceNo, SupplierId, TotalAmount));
    }

    public void Cancel()
    {
        if (Status == PurchaseInvoiceStatus.Approved)
            throw new InvalidOperationException("Use return process to reverse an approved invoice.");
        Status    = PurchaseInvoiceStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    private void RecalculateTotals()
    {
        Subtotal       = _items.Sum(i => i.TotalCost);
        DiscountAmount = DiscountPercent > 0 ? Math.Round(Subtotal * DiscountPercent / 100, 4) : 0;
        var afterDiscount = Subtotal - DiscountAmount;
        TaxAmount      = TaxPercent > 0 ? Math.Round(afterDiscount * TaxPercent / 100, 4) : 0;
        TotalAmount    = afterDiscount + TaxAmount;
        RemainingAmount= Math.Max(0, TotalAmount - PaidAmount);
        UpdatedAt      = DateTime.UtcNow;
    }
}
