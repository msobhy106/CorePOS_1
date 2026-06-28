namespace CorePOS.Application.Common;

public class DateRangeQuery
{
    public DateTime From { get; set; } = DateTime.Today;
    public DateTime To   { get; set; } = DateTime.Today.AddDays(1).AddSeconds(-1);
    public int?     BranchId    { get; set; }
    public int?     WarehouseId { get; set; }
}
