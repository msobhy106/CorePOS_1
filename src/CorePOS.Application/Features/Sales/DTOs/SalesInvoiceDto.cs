using CorePOS.Domain.Enums;

namespace CorePOS.Application.Features.Sales.DTOs;

public class SalesInvoiceDto
{
    public int              Id                { get; set; }
    public string           InvoiceNo         { get; set; } = string.Empty;
    public DateTime         InvoiceDate       { get; set; }
    public int?             CustomerId        { get; set; }
    public string?          CustomerName      { get; set; }
    public string?          CustomerPhone     { get; set; }
    public string           BranchName        { get; set; } = string.Empty;
    public string           WarehouseName     { get; set; } = string.Empty;
    public string           CashierName       { get; set; } = string.Empty;
    public InvoiceType      InvoiceType       { get; set; }
    public PaymentMethod    PaymentMethod     { get; set; }
    public SaleInvoiceStatus Status           { get; set; }
    public decimal          Subtotal          { get; set; }
    public decimal          DiscountPercent   { get; set; }
    public decimal          DiscountAmount    { get; set; }
    public decimal          TaxPercent        { get; set; }
    public decimal          TaxAmount         { get; set; }
    public decimal          DeliveryCost      { get; set; }
    public decimal          TotalAmount       { get; set; }
    public decimal          PaidAmount        { get; set; }
    public decimal          RemainingAmount   { get; set; }
    public string?          Notes             { get; set; }
    public List<SalesInvoiceItemDto> Items    { get; set; } = [];
}

public class SalesInvoiceItemDto
{
    public int     Id              { get; set; }
    public int     ProductId       { get; set; }
    public string  ProductNameAr   { get; set; } = string.Empty;
    public string? Barcode         { get; set; }
    public string  UnitName        { get; set; } = string.Empty;
    public decimal Quantity        { get; set; }
    public decimal UnitPrice       { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount  { get; set; }
    public decimal TaxAmount       { get; set; }
    public decimal TotalPrice      { get; set; }
    public decimal PurchasePrice   { get; set; }
    public decimal Profit          { get; set; }
    public decimal ReturnedQty     { get; set; }
}

public class SalesInvoiceListDto
{
    public int              Id              { get; set; }
    public string           InvoiceNo       { get; set; } = string.Empty;
    public DateTime         InvoiceDate     { get; set; }
    public string?          CustomerName    { get; set; }
    public string           CashierName     { get; set; } = string.Empty;
    public PaymentMethod    PaymentMethod   { get; set; }
    public SaleInvoiceStatus Status         { get; set; }
    public decimal          TotalAmount     { get; set; }
    public decimal          PaidAmount      { get; set; }
    public decimal          RemainingAmount { get; set; }
    public int              ItemsCount      { get; set; }
}

public class CartItemDto
{
    public int     ProductId       { get; set; }
    public int     UnitId          { get; set; }
    public string  ProductNameAr   { get; set; } = string.Empty;
    public string? Barcode         { get; set; }
    public string  UnitName        { get; set; } = string.Empty;
    public decimal Quantity        { get; set; }
    public decimal UnitPrice       { get; set; }
    public decimal PurchasePrice   { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount  { get; set; }
    public decimal TaxPercent      { get; set; }
    public decimal TaxAmount       { get; set; }
    public decimal TotalPrice      { get; set; }
    public string? ImagePath       { get; set; }
}
