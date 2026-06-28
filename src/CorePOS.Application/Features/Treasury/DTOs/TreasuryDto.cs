namespace CorePOS.Application.Features.Treasury.DTOs;

public class TreasuryTransactionDto
{
    public DateTime TransactionDate { get; set; }
    public string   Type            { get; set; } = string.Empty;
    public string   Direction       { get; set; } = string.Empty;
    public decimal  Amount          { get; set; }
    public decimal  BalanceAfter    { get; set; }
    public string?  Description     { get; set; }
    public string?  ReferenceType   { get; set; }
    public int?     ReferenceId     { get; set; }
}
