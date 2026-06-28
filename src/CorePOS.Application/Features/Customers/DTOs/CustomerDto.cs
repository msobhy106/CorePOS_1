namespace CorePOS.Application.Features.Customers.DTOs;

public class CustomerDto
{
    public int     Id               { get; set; }
    public string  Code             { get; set; } = string.Empty;
    public string  Name             { get; set; } = string.Empty;
    public string? Phone            { get; set; }
    public string? Phone2           { get; set; }
    public string? Address          { get; set; }
    public string? Email            { get; set; }
    public string? InstapayNumber   { get; set; }
    public string? TaxNumber        { get; set; }
    public int?    GroupId          { get; set; }
    public string? GroupName        { get; set; }
    public int?    PriceListId      { get; set; }
    public string? PriceListName    { get; set; }
    public decimal CreditLimit      { get; set; }
    public decimal CurrentBalance   { get; set; }
    public decimal TotalPoints      { get; set; }
    public int     PaymentPeriodDays{ get; set; }
    public bool    IsActive         { get; set; }
    public bool    IsOverCreditLimit{ get; set; }
}

public class CustomerListDto
{
    public int     Id             { get; set; }
    public string  Code           { get; set; } = string.Empty;
    public string  Name           { get; set; } = string.Empty;
    public string? Phone          { get; set; }
    public string? GroupName      { get; set; }
    public decimal CurrentBalance { get; set; }
    public decimal TotalPoints    { get; set; }
    public bool    IsActive       { get; set; }
    public bool    IsOverLimit    { get; set; }
}
