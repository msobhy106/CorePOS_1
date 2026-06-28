namespace CorePOS.Application.Common;

public class PaginationQuery
{
    public int    PageNumber { get; set; } = 1;
    public int    PageSize   { get; set; } = 20;
    public string? SortBy    { get; set; }
    public bool   SortDesc   { get; set; }
    public string? Search    { get; set; }

    public int Skip => (PageNumber - 1) * PageSize;
}
