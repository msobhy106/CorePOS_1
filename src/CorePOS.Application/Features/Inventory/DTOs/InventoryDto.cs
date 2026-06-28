namespace CorePOS.Application.Features.Inventory.DTOs;

public class StockBalanceDto
{
    public int     ProductId     { get; set; }
    public string  ProductCode   { get; set; } = string.Empty;
    public string? Barcode       { get; set; }
    public string  ProductName   { get; set; } = string.Empty;
    public string  CategoryName  { get; set; } = string.Empty;
    public string  WarehouseName { get; set; } = string.Empty;
    public decimal CurrentStock  { get; set; }
    public decimal AverageCost   { get; set; }
    public decimal LastCost      { get; set; }
    public decimal StockValue    { get; set; }
    public decimal MinStock      { get; set; }
    public decimal ReorderLevel  { get; set; }
    public decimal SalePrice     { get; set; }
    public string  StockStatus   { get; set; } = string.Empty;
    public bool    IsLowStock    { get; set; }
}

public class InventorySessionDto
{
    public int     Id            { get; set; }
    public string  SessionNo     { get; set; } = string.Empty;
    public DateTime SessionDate  { get; set; }
    public string  WarehouseName { get; set; } = string.Empty;
    public string  CountType     { get; set; } = string.Empty;
    public string  Status        { get; set; } = string.Empty;
    public int     ItemsCount    { get; set; }
    public decimal TotalDifference { get; set; }
    public List<InventorySessionItemDto> Items { get; set; } = [];
}

public class InventorySessionItemDto
{
    public int     ProductId      { get; set; }
    public string  ProductName    { get; set; } = string.Empty;
    public string? Barcode        { get; set; }
    public decimal SystemQuantity { get; set; }
    public decimal ActualQuantity { get; set; }
    public decimal Difference     { get; set; }
    public decimal UnitCost       { get; set; }
    public decimal DifferenceValue{ get; set; }
}

public class StockMovementDto
{
    public DateTime TransactionDate    { get; set; }
    public string   TransactionType    { get; set; } = string.Empty;
    public string   Direction          { get; set; } = string.Empty;
    public decimal  Quantity           { get; set; }
    public decimal  UnitCost           { get; set; }
    public decimal  BalanceAfter       { get; set; }
    public string?  ReferenceType      { get; set; }
    public int?     ReferenceId        { get; set; }
    public string?  Notes              { get; set; }
    public string?  CreatedByName      { get; set; }
}
