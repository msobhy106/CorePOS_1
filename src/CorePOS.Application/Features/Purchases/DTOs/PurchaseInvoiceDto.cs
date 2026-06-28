using CorePOS.Domain.Enums;

namespace CorePOS.Application.Features.Purchases.DTOs;

public class PurchaseInvoiceDto
{
    public int                    Id                { get; set; }
    public string                 InvoiceNo         { get; set; } = string.Empty;
    public string?                SupplierInvoiceNo { get; set; }
    public DateTime               InvoiceDate       { get; set; }
    public int?                   SupplierId        { get; set; }
    public string?                SupplierName      { get; set; }
    public string                 BranchName        { get; set; } = string.Empty;
    public string                 WarehouseName     { get; set; } = string.Empty;
    public PurchaseInvoiceStatus  Status            { get; set; }
    public PaymentMethod          PaymentMethod     { get; set; }
    public decimal                Subtotal          { get; set; }
    public decimal                DiscountAmount    { get; set; }
    public decimal                TaxAmount         { get; set; }
    public decimal                TotalAmount       { get; set; }
    public decimal                PaidAmount        { get; set; }
    public decimal                RemainingAmount   { get; set; }
    public string?                Notes             { get; set; }
    public List<PurchaseItemDto>  Items             { get; set; } = [];
}

public class PurchaseItemDto
{
    public int     Id              { get; set; }
    public int     ProductId       { get; set; }
    public string  ProductNameAr   { get; set; } = string.Empty;
    public string  UnitName        { get; set; } = string.Empty;
    public decimal Quantity        { get; set; }
    public decimal UnitCost        { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal TaxAmount       { get; set; }
    public decimal TotalCost       { get; set; }
    public decimal? SalePriceAfter { get; set; }
}

public class PurchaseInvoiceListDto
{
    public int                   Id              { get; set; }
    public string                InvoiceNo       { get; set; } = string.Empty;
    public DateTime              InvoiceDate     { get; set; }
    public string?               SupplierName    { get; set; }
    public PurchaseInvoiceStatus Status          { get; set; }
    public decimal               TotalAmount     { get; set; }
    public decimal               RemainingAmount { get; set; }
    public int                   ItemsCount      { get; set; }
}
