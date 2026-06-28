namespace CorePOS.Application.Features.Suppliers.DTOs;

public class SupplierDto
{
    public int     Id               { get; set; }
    public string  Code             { get; set; } = string.Empty;
    public string  Name             { get; set; } = string.Empty;
    public string? Phone            { get; set; }
    public string? Phone2           { get; set; }
    public string? Address          { get; set; }
    public string? Email            { get; set; }
    public string? TaxNumber        { get; set; }
    public string? ContactPerson    { get; set; }
    public decimal CurrentBalance   { get; set; }
    public decimal CreditLimit      { get; set; }
    public int     PaymentPeriodDays{ get; set; }
    public bool    IsActive         { get; set; }
}

public class SupplierListDto
{
    public int     Id             { get; set; }
    public string  Code           { get; set; } = string.Empty;
    public string  Name           { get; set; } = string.Empty;
    public string? Phone          { get; set; }
    public string? ContactPerson  { get; set; }
    public decimal CurrentBalance { get; set; }
    public bool    IsActive       { get; set; }
}
