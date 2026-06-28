using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Reports.DTOs;
using CorePOS.Application.Features.Reports.Queries;
using CorePOS.Application.Interfaces;

namespace CorePOS.Application.Features.Reports.Handlers;

public class GetDashboardHandler : IRequestHandler<GetDashboardQuery, Result<DashboardDto>>
{
    private readonly IApplicationDbContext _db;

    public GetDashboardHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<DashboardDto>> Handle(GetDashboardQuery query, CancellationToken ct)
    {
        var date = query.Date ?? DateTime.Today;

        // Today's sales
        var todaySales = _db.SalesInvoices
            .Where(s => s.InvoiceDate.Date == date.Date
                     && s.Status == CorePOS.Domain.Enums.SaleInvoiceStatus.Completed
                     && !s.IsDeleted
                     && (query.BranchId == null || s.BranchId == query.BranchId));

        var todayRevenue = todaySales.Sum(s => (decimal?)s.TotalAmount) ?? 0;
        var todayCount   = todaySales.Count();

        // Profit
        var todayProfit = _db.SalesInvoiceItems
            .Where(i => todaySales.Select(s => s.Id).Contains(i.InvoiceId))
            .Sum(i => (decimal?)(i.TotalPrice - i.Quantity * i.PurchasePrice)) ?? 0;

        // Low stock
        var lowStock = _db.ProductStocks
            .Join(_db.Products, ps => ps.ProductId, p => p.Id,
                  (ps, p) => new { ps.Quantity, p.MinStock, p.IsActive, p.IsDeleted })
            .Count(x => x.IsActive && !x.IsDeleted && x.MinStock > 0 && x.Quantity <= x.MinStock);

        // Top 5 products today
        var topProducts = _db.SalesInvoiceItems
            .Where(i => todaySales.Select(s => s.Id).Contains(i.InvoiceId))
            .GroupBy(i => new { i.ProductId, i.ProductNameAr })
            .Select(g => new TopProductDto
            {
                ProductName  = g.Key.ProductNameAr,
                TotalSold    = g.Sum(i => i.Quantity),
                TotalRevenue = g.Sum(i => i.TotalPrice),
                TotalProfit  = g.Sum(i => i.TotalPrice - i.Quantity * i.PurchasePrice)
            })
            .OrderByDescending(x => x.TotalSold)
            .Take(5)
            .ToList();

        // Recent 5 invoices
        var recent = _db.SalesInvoices
            .Where(s => !s.IsDeleted && (query.BranchId == null || s.BranchId == query.BranchId))
            .OrderByDescending(s => s.InvoiceDate)
            .Take(5)
            .Select(s => new RecentInvoiceDto
            {
                InvoiceNo    = s.InvoiceNo,
                InvoiceDate  = s.InvoiceDate,
                TotalAmount  = s.TotalAmount,
                PaymentMethod= s.PaymentMethod.ToString()
            })
            .ToList();

        // Monthly trend — last 12 months
        var monthly = _db.SalesInvoices
            .Where(s => s.Status == CorePOS.Domain.Enums.SaleInvoiceStatus.Completed
                     && !s.IsDeleted
                     && s.InvoiceDate >= date.AddMonths(-11)
                     && (query.BranchId == null || s.BranchId == query.BranchId))
            .GroupBy(s => new { s.InvoiceDate.Year, s.InvoiceDate.Month })
            .Select(g => new MonthlySalesDto
            {
                Year         = g.Key.Year,
                Month        = g.Key.Month,
                Revenue      = g.Sum(s => s.TotalAmount),
                InvoiceCount = g.Count()
            })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToList();

        string[] arabicMonths = ["يناير","فبراير","مارس","أبريل","مايو","يونيو",
                                  "يوليو","أغسطس","سبتمبر","أكتوبر","نوفمبر","ديسمبر"];
        foreach (var m in monthly)
            m.MonthName = arabicMonths[m.Month - 1];

        return Result<DashboardDto>.Success(new DashboardDto
        {
            TodayRevenue   = todayRevenue,
            TodayProfit    = todayProfit,
            TodayInvoices  = todayCount,
            TotalCustomers = _db.Customers.Count(c => c.IsActive && !c.IsDeleted),
            LowStockCount  = lowStock,
            TopProducts    = topProducts,
            MonthlySales   = monthly,
            RecentInvoices = recent
        });
    }
}
