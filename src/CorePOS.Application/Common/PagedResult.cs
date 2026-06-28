namespace CorePOS.Application.Common;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items      { get; }
    public int              TotalCount { get; }
    public int              PageNumber { get; }
    public int              PageSize   { get; }
    public int              TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool             HasNext    => PageNumber < TotalPages;
    public bool             HasPrev    => PageNumber > 1;

    public PagedResult(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items      = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize   = pageSize;
    }

    public static PagedResult<T> Empty(int pageNumber = 1, int pageSize = 20)
        => new([], 0, pageNumber, pageSize);
}
