using CorePOS.Domain.Common;
using CorePOS.Domain.Enums;
using CorePOS.Domain.Events;

namespace CorePOS.Domain.Entities;

public class SalesInvoice : AuditableEntity
{
    public string           InvoiceNo       { get; private set; } = string.Empty;
    public DateTime         InvoiceDate     { get; private set; }
    public int?             CustomerId      { get; private set; }
    public int              BranchId        { get; private set; }
    public int              WarehouseId     { get; private set; }
    public int              UserId          { get; private set; }
    public int?             ShiftId         { get; private set; }
    public InvoiceType      InvoiceType     { get; private set; } = InvoiceType.Retail;
    public PaymentMethod    PaymentMethod   { get; private set; } = PaymentMethod.Cash;
    public SaleInvoiceStatus Status         { get; private set; } = SaleInvoiceStatus.Draft;

    // Financials
    public decimal Subtotal           { get; private set; }
    public decimal DiscountPercent    { get; private set; }
    public decimal DiscountAmount     { get; private set; }
    public decimal TaxPercent         { get; private set; }
    public decimal TaxAmount          { get; private set; }
    public decimal DeliveryCost       { get; private set; }
    public int?    DeliveryAgentId    { get; private set; }
    public decimal TotalAmount        { get; private set; }
    public decimal PaidAmount         { get; private set; }
    public decimal VisaAmount         { get; private set; }
    public decimal BankTransferAmount { get; private set; }
    public decimal EWalletAmount      { get; private set; }
    public decimal RemainingAmount    { get; private set; }
    public string? Notes              { get; private set; }

    // Navigation
    public Customer?  Customer  { get; private set; }
    public Branch?    Branch    { get; private set; }
    public Warehouse? Warehouse { get; private set; }
    public User?      User      { get; private set; }

    private readonly List<SalesInvoiceItem> _items = [];
    public IReadOnlyCollection<SalesInvoiceItem> Items => _items.AsReadOnly();

    protected SalesInvoice() { }

    public static SalesInvoice Create(string invoiceNo, int branchId, int warehouseId,
        int userId, int? shiftId = null, int? customerId = null,
        InvoiceType invoiceType = InvoiceType.Retail)
    {
        if (string.IsNullOrWhiteSpace(invoiceNo)) throw new ArgumentException("Invoice number is required.");
        return new SalesInvoice
        {
            InvoiceNo   = invoiceNo.Trim(),
            InvoiceDate = DateTime.Now,
            BranchId    = branchId,
            WarehouseId = warehouseId,
            UserId      = userId,
            ShiftId     = shiftId,
            CustomerId  = customerId,
            InvoiceType = invoiceType,
            Status      = SaleInvoiceStatus.Draft
        };
    }

    /// <summary>Add item — merges quantity if product already exists.</summary>
    public SalesInvoiceItem AddOrUpdateItem(int productId, int unitId, string productNameAr,
        string? barcode, decimal quantity, decimal unitPrice, decimal purchasePrice,
        decimal discountPercent = 0, decimal taxPercent = 0)
    {
        if (Status != SaleInvoiceStatus.Draft && Status != SaleInvoiceStatus.Held)
            throw new InvalidOperationException("Cannot modify a completed invoice.");

        // Merge if same product+unit already in cart (SRS requirement)
        var existing = _items.FirstOrDefault(i => i.ProductId == productId && i.UnitId == unitId);
        if (existing != null)
        {
            existing.AddQuantity(quantity);
            RecalculateTotals();
            return existing;
        }

        var item = SalesInvoiceItem.Create(productId, unitId, productNameAr, barcode,
            quantity, unitPrice, purchasePrice, discountPercent, taxPercent);
        _items.Add(item);
        RecalculateTotals();
        return item;
    }

    public void RemoveItem(int productId, int unitId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId && i.UnitId == unitId)
            ?? throw new InvalidOperationException("Item not found in invoice.");
        _items.Remove(item);
        RecalculateTotals();
    }

    public void UpdateItem(int productId, int unitId, decimal quantity, decimal unitPrice, decimal discountPercent)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId && i.UnitId == unitId)
            ?? throw new InvalidOperationException("Item not found in invoice.");
        item.UpdateQuantityAndPrice(quantity, unitPrice, discountPercent);
        RecalculateTotals();
    }

    public void ApplyDiscount(decimal discountPercent = 0, decimal discountAmount = 0)
    {
        if (discountPercent < 0 || discountPercent > 100) throw new ArgumentException("Discount percent must be 0-100.");
        DiscountPercent = discountPercent;
        DiscountAmount  = discountPercent > 0
            ? Math.Round(Subtotal * discountPercent / 100, 4)
            : discountAmount;
        RecalculateTotals();
    }

    public void SetTax(decimal taxPercent)
    {
        if (taxPercent < 0 || taxPercent > 100) throw new ArgumentException("Tax percent must be 0-100.");
        TaxPercent = taxPercent;
        RecalculateTotals();
    }

    public void SetDelivery(decimal cost, int? agentId)
    {
        DeliveryCost    = cost >= 0 ? cost : 0;
        DeliveryAgentId = agentId;
        RecalculateTotals();
    }

    public void SetPayment(PaymentMethod method, decimal paidAmount,
        decimal visaAmount = 0, decimal bankAmount = 0, decimal eWalletAmount = 0)
    {
        PaymentMethod       = method;
        VisaAmount          = visaAmount;
        BankTransferAmount  = bankAmount;
        EWalletAmount       = eWalletAmount;

        // SRS: if cash → auto-fill paid = total, remaining = 0
        PaidAmount      = method == PaymentMethod.Cash ? TotalAmount : paidAmount;
        RemainingAmount = Math.Max(0, TotalAmount - PaidAmount);
        UpdatedAt       = DateTime.UtcNow;
    }

    public void SetCustomer(int? customerId)
    {
        CustomerId = customerId;
        UpdatedAt  = DateTime.UtcNow;
    }

    public void SetNotes(string? notes) { Notes = notes; UpdatedAt = DateTime.UtcNow; }

    public void Hold()
    {
        if (Status != SaleInvoiceStatus.Draft)
            throw new InvalidOperationException("Only draft invoices can be held.");
        Status    = SaleInvoiceStatus.Held;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Retrieve()
    {
        if (Status != SaleInvoiceStatus.Held)
            throw new InvalidOperationException("Only held invoices can be retrieved.");
        Status    = SaleInvoiceStatus.Draft;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (!_items.Any()) throw new InvalidOperationException("Cannot complete an empty invoice.");
        if (Status != SaleInvoiceStatus.Draft && Status != SaleInvoiceStatus.Held)
            throw new InvalidOperationException("Invoice cannot be completed in its current state.");
        Status    = SaleInvoiceStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new SaleCompletedEvent(Id, InvoiceNo, TotalAmount, CustomerId));
    }

    public void Cancel()
    {
        if (Status == SaleInvoiceStatus.Completed)
            throw new InvalidOperationException("Use return process to reverse a completed invoice.");
        Status    = SaleInvoiceStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkPartialReturn() { Status = SaleInvoiceStatus.PartialReturn; UpdatedAt = DateTime.UtcNow; }
    public void MarkFullReturn()    { Status = SaleInvoiceStatus.FullReturn;    UpdatedAt = DateTime.UtcNow; }

    private void RecalculateTotals()
    {
        Subtotal       = _items.Sum(i => i.TotalPrice);
        DiscountAmount = DiscountPercent > 0 ? Math.Round(Subtotal * DiscountPercent / 100, 4) : DiscountAmount;
        var afterDiscount = Subtotal - DiscountAmount;
        TaxAmount      = TaxPercent > 0 ? Math.Round(afterDiscount * TaxPercent / 100, 4) : 0;
        TotalAmount    = afterDiscount + TaxAmount + DeliveryCost;
        if (PaymentMethod == PaymentMethod.Cash)
        {
            PaidAmount      = TotalAmount;
            RemainingAmount = 0;
        }
        else
        {
            RemainingAmount = Math.Max(0, TotalAmount - PaidAmount);
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public decimal CalculateTotalCost()
        => _items.Sum(i => i.Quantity * i.PurchasePrice);

    public decimal CalculateGrossProfit()
        => TotalAmount - CalculateTotalCost();
}
