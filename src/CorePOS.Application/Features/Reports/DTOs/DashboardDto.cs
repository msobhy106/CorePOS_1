namespace CorePOS.Application.Features.Reports.DTOs;

public class DashboardDto
{
    public decimal TodayRevenue    { get; set; }
    public decimal TodayProfit     { get; set; }
    public int     TodayInvoices   { get; set; }
    public int     TotalCustomers  { get; set; }
    public int     LowStockCount   { get; set; }
    public decimal OpenShiftBalance{ get; set; }
    public List<TopProductDto>    TopProducts    { get; set; } = [];
    public List<MonthlySalesDto>  MonthlySales   { get; set; } = [];
    public List<RecentInvoiceDto> RecentInvoices { get; set; } = [];
}

public class TopProductDto
{
    public string  ProductName  { get; set; } = string.Empty;
    public decimal TotalSold    { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalProfit  { get; set; }
}

public class MonthlySalesDto
{
    public int     Year         { get; set; }
    public int     Month        { get; set; }
    public string  MonthName    { get; set; } = string.Empty;
    public decimal Revenue      { get; set; }
    public decimal GrossProfit  { get; set; }
    public int     InvoiceCount { get; set; }
}

public class RecentInvoiceDto
{
    public string   InvoiceNo    { get; set; } = string.Empty;
    public DateTime InvoiceDate  { get; set; }
    public string?  CustomerName { get; set; }
    public decimal  TotalAmount  { get; set; }
    public string   PaymentMethod{ get; set; } = string.Empty;
}

public class SalesReportDto
{
    public DateTime From         { get; set; }
    public DateTime To           { get; set; }
    public int      InvoiceCount { get; set; }
    public decimal  TotalRevenue { get; set; }
    public decimal  TotalCost    { get; set; }
    public decimal  GrossProfit  { get; set; }
    public decimal  ProfitMargin { get; set; }
    public decimal  TotalDiscount{ get; set; }
    public decimal  TotalTax     { get; set; }
    public List<DailySalesRowDto> Rows { get; set; } = [];
}

public class DailySalesRowDto
{
    public DateTime Date         { get; set; }
    public int      InvoiceCount { get; set; }
    public decimal  Revenue      { get; set; }
    public decimal  Profit       { get; set; }
}
