using MediatR;
using CorePOS.Application.Common;
using CorePOS.Domain.Entities;
using CorePOS.Domain.Interfaces;

namespace CorePOS.Application.Features.Sales.Queries;

// ════════════════════════════════════════════════════════════════════
// GET SALE INVOICE FOR PRINT — Handler
// ════════════════════════════════════════════════════════════════════
public class GetSaleInvoiceForPrintHandler
    : IRequestHandler<GetSaleInvoiceForPrintQuery, Result<SalesInvoice>>
{
    private readonly IUnitOfWork _uow;

    public GetSaleInvoiceForPrintHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<SalesInvoice>> Handle(
        GetSaleInvoiceForPrintQuery request, CancellationToken ct)
    {
        try
        {
            var invoice = await _uow.Sales.GetByIdWithItemsAsync(request.InvoiceId, ct);
            if (invoice == null)
                return Result<SalesInvoice>.Failure($"الفاتورة رقم {request.InvoiceId} غير موجودة");

            return Result<SalesInvoice>.Success(invoice);
        }
        catch (Exception ex)
        {
            return Result<SalesInvoice>.Failure(ex.Message);
        }
    }
}

// ════════════════════════════════════════════════════════════════════
// GET SALES REPORT QUERY — Handler
// ════════════════════════════════════════════════════════════════════
public class GetSalesReportHandler
    : IRequestHandler<GetSalesReportQuery, Result<List<SalesReportRowDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetSalesReportHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<List<SalesReportRowDto>>> Handle(
        GetSalesReportQuery request, CancellationToken ct)
    {
        try
        {
            var invoices = await _uow.Sales.GetByDateRangeAsync(
                request.From, request.To, request.BranchId, ct);

            var rows = invoices
                .Where(inv => inv.Status != Domain.Enums.SaleInvoiceStatus.Cancelled)
                .Select(inv => new SalesReportRowDto(
                    InvoiceNo:   inv.InvoiceNo,
                    Date:        inv.InvoiceDate,
                    CustomerName:inv.Customer?.Name ?? "نقدي",
                    ItemsCount:  inv.Items.Count,
                    Total:       inv.TotalAmount,
                    Paid:        inv.PaidAmount,
                    PayMethodAr: MapPayMethod(inv.PaymentMethod.ToString())))
                .OrderByDescending(r => r.Date)
                .ToList();

            return Result<List<SalesReportRowDto>>.Success(rows);
        }
        catch (Exception ex)
        {
            return Result<List<SalesReportRowDto>>.Failure(ex.Message);
        }
    }

    private static string MapPayMethod(string m) => m switch
    {
        "Cash"         => "نقدي",
        "Visa"         => "فيزا",
        "BankTransfer" => "تحويل بنكي",
        "EWallet"      => "محفظة",
        "Credit"       => "آجل",
        "Mixed"        => "مختلط",
        _              => m
    };
}

// ════════════════════════════════════════════════════════════════════
// GET PROFIT REPORT — Handler
// ════════════════════════════════════════════════════════════════════
public class GetProfitReportHandler
    : IRequestHandler<GetProfitReportQuery, Result<List<ProfitReportRowDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetProfitReportHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<List<ProfitReportRowDto>>> Handle(
        GetProfitReportQuery request, CancellationToken ct)
    {
        try
        {
            var invoices = await _uow.Sales.GetByDateRangeAsync(
                request.From, request.To, request.BranchId, ct);

            var rows = invoices
                .Where(inv => inv.Status != Domain.Enums.SaleInvoiceStatus.Cancelled)
                .SelectMany(inv => inv.Items.Select(item => new { inv, item }))
                .GroupBy(x => x.item.ProductId)
                .Select(g =>
                {
                    var sales  = g.Sum(x => x.item.TotalPrice);
                    var cost   = g.Sum(x => x.item.Quantity * x.item.PurchasePrice);
                    var profit = sales - cost;
                    return new ProfitReportRowDto(
                        ProductName: g.First().item.ProductNameAr,
                        QtySold:     g.Sum(x => x.item.Quantity),
                        Sales:       sales,
                        Cost:        cost,
                        Profit:      profit);
                })
                .OrderByDescending(r => r.Profit)
                .ToList();

            return Result<List<ProfitReportRowDto>>.Success(rows);
        }
        catch (Exception ex)
        {
            return Result<List<ProfitReportRowDto>>.Failure(ex.Message);
        }
    }
}

// ════════════════════════════════════════════════════════════════════
// GET CUSTOMER DEBTS REPORT — Handler
// ════════════════════════════════════════════════════════════════════
public class GetCustomerDebtsHandler
    : IRequestHandler<GetCustomerDebtsReportQuery, Result<List<CustomerDebtRowDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetCustomerDebtsHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<List<CustomerDebtRowDto>>> Handle(
        GetCustomerDebtsReportQuery request, CancellationToken ct)
    {
        try
        {
            var customers = await _uow.Customers.GetAllWithBalanceAsync(request.BranchId, ct);

            var rows = customers
                .Where(c => c.CurrentBalance > 0)
                .Select(c => new CustomerDebtRowDto(
                    Name:             c.Name,
                    Phone:            c.Phone ?? "",
                    TotalPurchases:   c.TotalPurchases,
                    TotalPaid:        c.TotalPaid,
                    Balance:          c.CurrentBalance,
                    LastInvoiceDate:  c.LastInvoiceDate ?? DateTime.Today))
                .OrderByDescending(r => r.Balance)
                .ToList();

            return Result<List<CustomerDebtRowDto>>.Success(rows);
        }
        catch (Exception ex)
        {
            return Result<List<CustomerDebtRowDto>>.Failure(ex.Message);
        }
    }
}

// ════════════════════════════════════════════════════════════════════
// GET SUPPLIER DUES REPORT — Handler
// ════════════════════════════════════════════════════════════════════
public class GetSupplierDuesHandler
    : IRequestHandler<GetSupplierDuesReportQuery, Result<List<SupplierDueRowDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetSupplierDuesHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<List<SupplierDueRowDto>>> Handle(
        GetSupplierDuesReportQuery request, CancellationToken ct)
    {
        try
        {
            var suppliers = await _uow.Suppliers.GetAllWithBalanceAsync(ct);

            var rows = suppliers
                .Where(s => s.CurrentBalance > 0)
                .Select(s => new SupplierDueRowDto(
                    Name:            s.Name,
                    Phone:           s.Phone ?? "",
                    TotalPurchases:  s.TotalPurchases,
                    TotalPaid:       s.TotalPaid,
                    Balance:         s.CurrentBalance,
                    LastInvoiceDate: s.LastInvoiceDate ?? DateTime.Today))
                .OrderByDescending(r => r.Balance)
                .ToList();

            return Result<List<SupplierDueRowDto>>.Success(rows);
        }
        catch (Exception ex)
        {
            return Result<List<SupplierDueRowDto>>.Failure(ex.Message);
        }
    }
}

// ════════════════════════════════════════════════════════════════════
// GET LOW STOCK REPORT — Handler
// ════════════════════════════════════════════════════════════════════
public class GetLowStockHandler
    : IRequestHandler<GetLowStockReportQuery, Result<List<LowStockRowDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetLowStockHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<List<LowStockRowDto>>> Handle(
        GetLowStockReportQuery request, CancellationToken ct)
    {
        try
        {
            var stocks = await _uow.Inventory.GetLowStockAsync(request.BranchId, ct);

            var rows = stocks.Select(s => new LowStockRowDto(
                Barcode:         s.Product?.Barcode ?? "",
                NameAr:          s.Product?.NameAr  ?? "",
                CurrentQty:      s.Quantity,
                MinStock:        s.Product?.MinStock ?? 0,
                DefaultSupplier: s.Product?.DefaultSupplier?.Name ?? "-"))
                .OrderBy(r => r.CurrentQty)
                .ToList();

            return Result<List<LowStockRowDto>>.Success(rows);
        }
        catch (Exception ex)
        {
            return Result<List<LowStockRowDto>>.Failure(ex.Message);
        }
    }
}

// ════════════════════════════════════════════════════════════════════
// GET SLOW MOVING — Handler
// ════════════════════════════════════════════════════════════════════
public class GetSlowMovingHandler
    : IRequestHandler<GetSlowMovingReportQuery, Result<List<SlowMovingRowDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetSlowMovingHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<List<SlowMovingRowDto>>> Handle(
        GetSlowMovingReportQuery request, CancellationToken ct)
    {
        try
        {
            var items = await _uow.Inventory.GetSlowMovingAsync(
                request.BranchId, request.From, request.To, ct);

            var rows = items.Select(i => new SlowMovingRowDto(
                Barcode:      i.Barcode      ?? "",
                NameAr:       i.NameAr       ?? "",
                QtySold:      i.QtySold,
                CurrentStock: i.CurrentStock,
                LastMovement: i.LastMovement))
                .OrderBy(r => r.QtySold)
                .ToList();

            return Result<List<SlowMovingRowDto>>.Success(rows);
        }
        catch (Exception ex)
        {
            return Result<List<SlowMovingRowDto>>.Failure(ex.Message);
        }
    }
}

// ════════════════════════════════════════════════════════════════════
// GET EXPENSES REPORT — Handler
// ════════════════════════════════════════════════════════════════════
public class GetExpensesReportHandler
    : IRequestHandler<GetExpensesReportQuery, Result<List<ExpensesReportRowDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetExpensesReportHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<List<ExpensesReportRowDto>>> Handle(
        GetExpensesReportQuery request, CancellationToken ct)
    {
        try
        {
            var expenses = await _uow.Expenses.GetByDateRangeAsync(
                request.BranchId, request.From, request.To, ct);

            var rows = expenses.Select(e => new ExpensesReportRowDto(
                Date:          e.ExpenseDate,
                TypeAr:        e.ExpenseTypeAr,
                Amount:        e.Amount,
                Description:   e.Description ?? "",
                CreatedByName: e.CreatedBy?.FullName ?? ""))
                .OrderByDescending(r => r.Date)
                .ToList();

            return Result<List<ExpensesReportRowDto>>.Success(rows);
        }
        catch (Exception ex)
        {
            return Result<List<ExpensesReportRowDto>>.Failure(ex.Message);
        }
    }
}

// ════════════════════════════════════════════════════════════════════
// GET CASHBOX MOVEMENT — Handler
// ════════════════════════════════════════════════════════════════════
public class GetCashboxMovementHandler
    : IRequestHandler<GetCashboxMovementReportQuery, Result<List<CashboxMovementRowDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetCashboxMovementHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<List<CashboxMovementRowDto>>> Handle(
        GetCashboxMovementReportQuery request, CancellationToken ct)
    {
        try
        {
            // Build cashbox movement from Sales + Expenses in the date range (BUG-046 fix)
            var sales    = await _uow.Sales.GetByDateRangeAsync(request.From, request.To, request.BranchId, ct);
            var expenses = await _uow.Expenses.GetByDateRangeAsync(request.From, request.To, request.BranchId, ct);

            var rows = new List<CashboxMovementRowDto>();

            foreach (var s in sales.Where(s => s.Status != Domain.Enums.SaleInvoiceStatus.Cancelled))
                rows.Add(new CashboxMovementRowDto(
                    Date:        s.InvoiceDate,
                    CashboxName: "",
                    TypeAr:      "مبيعات",
                    Debit:       s.PaidAmount,
                    Credit:      0,
                    Balance:     0,
                    Notes:       s.InvoiceNo));

            foreach (var e in expenses)
                rows.Add(new CashboxMovementRowDto(
                    Date:        e.ExpenseDate.ToDateTime(TimeOnly.MinValue),
                    CashboxName: "",
                    TypeAr:      "مصروف - " + (e.Category?.NameAr ?? ""),
                    Debit:       0,
                    Credit:      e.Amount,
                    Balance:     0,
                    Notes:       e.Description ?? ""));

            return Result<List<CashboxMovementRowDto>>.Success(
                rows.OrderBy(r => r.Date).ToList());
        }
        catch (Exception ex)
        {
            return Result<List<CashboxMovementRowDto>>.Failure(ex.Message);
        }
    }
}

// ════════════════════════════════════════════════════════════════════
// GET CASHIER PERFORMANCE — Handler
// ════════════════════════════════════════════════════════════════════
public class GetCashierPerformanceHandler
    : IRequestHandler<GetCashierPerformanceReportQuery, Result<List<CashierPerformanceRowDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetCashierPerformanceHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<List<CashierPerformanceRowDto>>> Handle(
        GetCashierPerformanceReportQuery request, CancellationToken ct)
    {
        try
        {
            var invoices = await _uow.Sales.GetByDateRangeAsync(
                request.From, request.To, request.BranchId, ct);

            var rows = invoices
                .GroupBy(inv => inv.UserId)
                .Select(g =>
                {
                    var sold    = g.Where(i => i.Status != Domain.Enums.SaleInvoiceStatus.Cancelled).ToList();
                    var returns = g.Where(i => i.Status == Domain.Enums.SaleInvoiceStatus.Cancelled ||
                                               i.Status == Domain.Enums.SaleInvoiceStatus.Cancelled).ToList();
                    var totalSales = sold.Sum(i => i.TotalAmount);
                    return new CashierPerformanceRowDto(
                        CashierName:  g.First().User?.FullName ?? $"User {g.Key}",
                        InvoiceCount: sold.Count,
                        TotalSales:   totalSales,
                        AvgInvoice:   sold.Count > 0 ? totalSales / sold.Count : 0,
                        TotalReturns: returns.Sum(i => i.TotalAmount));
                })
                .OrderByDescending(r => r.TotalSales)
                .ToList();

            return Result<List<CashierPerformanceRowDto>>.Success(rows);
        }
        catch (Exception ex)
        {
            return Result<List<CashierPerformanceRowDto>>.Failure(ex.Message);
        }
    }
}
