// ════════════════════════════════════════════════════════════════════
// APPLICATION LAYER STUBS FOR PHASE 9
// These are the MediatR commands/queries that Phase 9 UI calls.
// The actual handler implementations belong in Phase 7 Application layer.
// This file documents the full contract so handlers can be implemented.
// ════════════════════════════════════════════════════════════════════

namespace CorePOS.Application;

/// <summary>Assembly marker for MediatR registration</summary>
public sealed class AssemblyMarker { }

// ── Shared Result type ─────────────────────────────────────────────
namespace CorePOS.Application.Common;

public sealed class Result<T>
{
    public bool     IsSuccess { get; private init; }
    public T?       Value     { get; private init; }
    public string   Error     { get; private init; } = string.Empty;

    public static Result<T> Success(T value)         => new() { IsSuccess = true,  Value = value };
    public static Result<T> Failure(string error)    => new() { IsSuccess = false, Error = error };
}

public sealed class Result
{
    public bool   IsSuccess { get; private init; }
    public string Error     { get; private init; } = string.Empty;

    public static Result Success()               => new() { IsSuccess = true };
    public static Result Failure(string error)   => new() { IsSuccess = false, Error = error };
}

// ════════════════════════════════════════════════════════════════════
// AUTH
// ════════════════════════════════════════════════════════════════════
namespace CorePOS.Application.Features.Auth.Commands;
using MediatR;
using CorePOS.Application.Common;

public record LoginCommand(string Username, string Password) : IRequest<Result<LoginResultDto>>;
public record LoginResultDto(bool Success, string Error, int UserId, string Username, string FullName, string FullNameAr, string RoleName, string RoleNameAr, int BranchId, string BranchName, int WarehouseId, string? PhotoPath, IEnumerable<(string Module, string Action)> Permissions);

public record OpenShiftCommand(int UserId, int BranchId, decimal OpeningBalance) : IRequest<Result<OpenShiftResultDto>>;
public record OpenShiftResultDto(int ShiftId, string ShiftNo);

// ════════════════════════════════════════════════════════════════════
// DASHBOARD
// ════════════════════════════════════════════════════════════════════
namespace CorePOS.Application.Features.Dashboard.Queries;
using MediatR;
using CorePOS.Application.Common;

public record GetDashboardSummaryQuery(int BranchId, DateTime Date) : IRequest<Result<DashboardSummaryDto>>;
public record DashboardSummaryDto(
    decimal TodaySales, decimal TodayProfit,
    int TodayInvoiceCount, int NewCustomersToday,
    List<RecentInvoiceDto> RecentInvoices);
public record RecentInvoiceDto(string InvoiceNo, string CustomerName, DateTime InvoiceDate, decimal Total, decimal Paid, string PaymentMethodAr);

// ════════════════════════════════════════════════════════════════════
// PRODUCTS
// ════════════════════════════════════════════════════════════════════
namespace CorePOS.Application.Features.Products.Queries;
using MediatR;
using CorePOS.Application.Common;

public record GetProductByBarcodeQuery(string Barcode, int WarehouseId) : IRequest<Result<ProductLookupDto>>;
public record SearchProductsQuery(string Search, int BranchId, int MaxResults) : IRequest<Result<List<ProductSearchDto>>>;
public record GetProductsListQuery(string Search, int? CategoryId, int BranchId, int PageSize) : IRequest<Result<PagedResult<ProductListDto>>>;
public record GetProductByIdQuery(int ProductId) : IRequest<Result<ProductDetailDto>>;
public record GetProductStockQuery(int ProductId) : IRequest<Result<List<ProductStockDto>>>;
public record GetUnitsQuery() : IRequest<Result<List<UnitDto>>>;

public record ProductLookupDto(int ProductId, string Barcode, string NameAr, string SaleUnitName, string PurchaseUnitName, decimal SalePrice, decimal PurchasePrice, decimal Stock);
public record ProductSearchDto(int ProductId, string Barcode, string NameAr, decimal SalePrice, decimal PurchasePrice, string PurchaseUnitName, decimal Stock);
public record ProductListDto(int ProductId, string ProductCode, string Barcode, string NameAr, string NameEn, string CategoryName, decimal SalePrice, decimal Stock, string SaleUnitName, bool IsActive, decimal MinStock);
public record ProductDetailDto(int ProductId, string ProductCode, string Barcode, string NameAr, string NameEn, int CategoryId, int BaseUnitId, int SaleUnitId, int PurchaseUnitId, decimal PurchasePrice, decimal SalePrice, decimal WholesalePrice, decimal HalfWholesalePrice, decimal SpecialPrice, int MinStock, int ReorderLevel, string Manufacturer, DateTime? ExpiryDate, int? DefaultSupplierId, bool IsActive);
public record ProductStockDto(string WarehouseName, string BranchName, decimal Quantity, decimal AverageCost);
public record UnitDto(int UnitId, string Name);
public record PagedResult<T>(List<T> Items, int TotalCount);

namespace CorePOS.Application.Features.Products.Commands;
using MediatR;
using CorePOS.Application.Common;

public record CreateProductCommand(string ProductCode, string Barcode, string NameAr, string NameEn, int CategoryId, int BaseUnitId, int SaleUnitId, int PurchaseUnitId, decimal PurchasePrice, decimal SalePrice, decimal WholesalePrice, decimal HalfWholesalePrice, decimal SpecialPrice, int MinStock, int ReorderLevel, string Manufacturer, DateTime? ExpiryDate, int? DefaultSupplierId, bool IsActive, int CreatedBy) : IRequest<Result<int>>;
public record UpdateProductCommand(int ProductId, string ProductCode, string Barcode, string NameAr, string NameEn, int CategoryId, int BaseUnitId, int SaleUnitId, int PurchaseUnitId, decimal PurchasePrice, decimal SalePrice, decimal WholesalePrice, decimal HalfWholesalePrice, decimal SpecialPrice, int MinStock, int ReorderLevel, string Manufacturer, DateTime? ExpiryDate, int? DefaultSupplierId, bool IsActive, int ModifiedBy) : IRequest<Result>;
public record DeleteProductCommand(int ProductId, int DeletedBy) : IRequest<Result>;

// ════════════════════════════════════════════════════════════════════
// CATEGORIES
// ════════════════════════════════════════════════════════════════════
namespace CorePOS.Application.Features.Categories.Queries;
using MediatR;
using CorePOS.Application.Common;

public record GetCategoriesTreeQuery() : IRequest<Result<List<CategoryTreeDto>>>;
public record GetCategoriesListQuery() : IRequest<Result<List<CategoryDto>>>;
public record CategoryTreeDto(int CategoryId, string Name, List<CategoryDto> SubCategories);
public record CategoryDto(int CategoryId, string Name);

// ════════════════════════════════════════════════════════════════════
// CUSTOMERS
// ════════════════════════════════════════════════════════════════════
namespace CorePOS.Application.Features.Customers.Queries;
using MediatR;
using CorePOS.Application.Common;

public record GetCustomersListQuery(string Search, int BranchId, int PageSize) : IRequest<Result<PagedResult<CustomerListDto>>>;
public record SearchCustomersQuery(string Search, int MaxResults) : IRequest<Result<List<CustomerSearchDto>>>;
public record GetCustomerByIdQuery(int CustomerId) : IRequest<Result<CustomerDetailDto>>;
public record GetCustomerStatementQuery(int CustomerId) : IRequest<Result<CustomerStatementDto>>;

public record CustomerListDto(int CustomerId, string Name, string Phone, string Address, decimal Balance, decimal CreditLimit, int LoyaltyPoints, string BranchName);
public record CustomerSearchDto(int CustomerId, string Name, string Phone, decimal Balance);
public record CustomerDetailDto(int CustomerId, string Name, string Phone, string Address, string Email, string InstaPayNo, string TaxNo, decimal Balance, decimal CreditLimit, int PaymentPeriodDays, int BranchId);
public record CustomerStatementDto(decimal CurrentBalance, List<StatementTransactionDto> Transactions);
public record StatementTransactionDto(DateTime Date, string TypeAr, string Reference, decimal Debit, decimal Credit, decimal Balance);

namespace CorePOS.Application.Features.Customers.Commands;
using MediatR;
using CorePOS.Application.Common;

public record CreateCustomerCommand(string Name, string Phone, string Address, string Email, string InstaPayNo, string TaxNo, decimal CreditLimit, int PaymentPeriodDays, int BranchId, int CreatedBy) : IRequest<Result<int>>;
public record UpdateCustomerCommand(int CustomerId, string Name, string Phone, string Address, string Email, string InstaPayNo, string TaxNo, decimal CreditLimit, int PaymentPeriodDays, int BranchId, int ModifiedBy) : IRequest<Result>;
public record DeleteCustomerCommand(int CustomerId, int DeletedBy) : IRequest<Result>;

// ════════════════════════════════════════════════════════════════════
// SUPPLIERS
// ════════════════════════════════════════════════════════════════════
namespace CorePOS.Application.Features.Suppliers.Queries;
using MediatR;
using CorePOS.Application.Common;

public record GetSuppliersListQuery(string Search = "", int PageSize = 200) : IRequest<Result<PagedResult<SupplierListDto>>>;
public record GetSupplierByIdQuery(int SupplierId) : IRequest<Result<SupplierDetailDto>>;

public record SupplierListDto(int SupplierId, string Name, string Phone, string Address, string TaxNo, decimal Balance);
public record SupplierDetailDto(int SupplierId, string Name, string Phone, string Address, string TaxNo, string Email, decimal Balance);

namespace CorePOS.Application.Features.Suppliers.Commands;
using MediatR;
using CorePOS.Application.Common;

public record CreateSupplierCommand(string Name, string Phone, string Address, string TaxNo, string Email, int CreatedBy) : IRequest<Result<int>>;
public record UpdateSupplierCommand(int SupplierId, string Name, string Phone, string Address, string TaxNo, string Email, int ModifiedBy) : IRequest<Result>;
public record DeleteSupplierCommand(int SupplierId, int DeletedBy) : IRequest<Result>;

// ════════════════════════════════════════════════════════════════════
// SALES
// ════════════════════════════════════════════════════════════════════
namespace CorePOS.Application.Features.Sales.Commands;
using MediatR;
using CorePOS.Application.Common;

public record SaleInvoiceItemDto(int ProductId, decimal Qty, decimal Price, decimal Discount);
public record CreateSaleInvoiceCommand(int BranchId, int WarehouseId, int CashierId, int ShiftId, int? CustomerId, List<SaleInvoiceItemDto> Items, decimal DiscountPercent, decimal TaxPercent, decimal Paid, string PaymentMethod, decimal DeliveryCost, string DeliveryAgent, string Notes) : IRequest<Result<string>>;
public record DeleteSaleInvoiceCommand(int InvoiceId, int DeletedBy) : IRequest<Result>;
public record CreateSaleReturnCommand(int InvoiceId, List<(int itemId, decimal qty)> Items, int CreatedBy) : IRequest<Result>;

namespace CorePOS.Application.Features.Sales.Queries;
using MediatR;
using CorePOS.Application.Common;

public record GetSalesInvoicesQuery(int BranchId, DateTime From, DateTime To, string Search, string? PaymentMethod) : IRequest<Result<List<SaleInvoiceListDto>>>;
public record GetSaleInvoiceDetailQuery(int InvoiceId) : IRequest<Result<SaleInvoiceDetailDto>>;

public record SaleInvoiceListDto(int InvoiceId, string InvoiceNo, DateTime InvoiceDate, string CustomerName, int ItemsCount, decimal Subtotal, decimal Discount, decimal Tax, decimal Total, decimal Paid, string PaymentMethodAr, string StatusAr, string Status, string CashierName);
public record SaleInvoiceDetailDto(string InvoiceNo, DateTime InvoiceDate, string CustomerName, string CashierName, string PaymentMethodAr, string Status, string StatusAr, decimal Subtotal, decimal Discount, decimal Tax, decimal Total, decimal Paid, string Notes, List<SaleInvoiceItemDetailDto> Items);
public record SaleInvoiceItemDetailDto(int InvoiceItemId, string Barcode, string NameAr, string UnitName, decimal Price, decimal Qty, decimal Discount, decimal LineTotal);

// ════════════════════════════════════════════════════════════════════
// PURCHASES
// ════════════════════════════════════════════════════════════════════
namespace CorePOS.Application.Features.Purchases.Queries;
using MediatR;
using CorePOS.Application.Common;

public record GetPurchaseInvoicesQuery(int BranchId, DateTime From, DateTime To, string Search, string? Status) : IRequest<Result<List<PurchaseInvoiceListDto>>>;
public record GetPurchaseInvoiceDetailQuery(int InvoiceId) : IRequest<Result<PurchaseInvoiceDetailDto>>;

public record PurchaseInvoiceListDto(int InvoiceId, string InvoiceNo, DateTime InvoiceDate, string SupplierName, int ItemsCount, decimal Total, decimal Paid, string StatusAr, string Status, string CreatedByName);
public record PurchaseInvoiceDetailDto(int SupplierId, string SupplierName, DateTime InvoiceDate, string SupplierRefNo, string Notes, decimal DiscountPercent, decimal Paid, List<PurchaseInvoiceItemDto> Items);
public record PurchaseInvoiceItemDto(int InvoiceItemId, int ProductId, string Barcode, string NameAr, string UnitName, decimal Price, decimal Qty, decimal Discount, decimal LineTotal);

namespace CorePOS.Application.Features.Purchases.Commands;
using MediatR;
using CorePOS.Application.Common;

public record PurchaseItemDto(int ProductId, decimal Qty, decimal Price, decimal Discount);
public record CreatePurchaseInvoiceCommand(int BranchId, int WarehouseId, int SupplierId, DateTime InvoiceDate, string SupplierRefNo, string Notes, List<PurchaseItemDto> Items, decimal DiscountPercent, decimal Paid, bool Approve, int CreatedBy) : IRequest<Result<int>>;
public record UpdatePurchaseInvoiceCommand(int InvoiceId, int SupplierId, DateTime InvoiceDate, string SupplierRefNo, string Notes, List<PurchaseItemDto> Items, decimal DiscountPercent, decimal Paid, bool Approve, int ModifiedBy) : IRequest<Result>;
public record ApprovePurchaseInvoiceCommand(int InvoiceId, int ApprovedBy) : IRequest<Result>;
public record CreatePurchaseReturnCommand(int InvoiceId, List<(int itemId, decimal qty)> Items, int CreatedBy) : IRequest<Result>;

// ════════════════════════════════════════════════════════════════════
// INVENTORY
// ════════════════════════════════════════════════════════════════════
namespace CorePOS.Application.Features.Inventory.Queries;
using MediatR;
using CorePOS.Application.Common;

public record GetStockListQuery(int BranchId, int? WarehouseId, string Search) : IRequest<Result<List<StockListDto>>>;
public record GetWarehousesQuery(int BranchId) : IRequest<Result<List<WarehouseDto>>>;

public record StockListDto(int ProductId, string Barcode, string NameAr, string CategoryName, string UnitName, decimal Quantity, decimal MinStock, decimal AverageCost, decimal LastCost);
public record WarehouseDto(int WarehouseId, string Name, string BranchName);

namespace CorePOS.Application.Features.Inventory.Commands;
using MediatR;
using CorePOS.Application.Common;

public record ApproveInventoryCountCommand(int WarehouseId, List<(int productId, decimal actualQty)> Adjustments, int ApprovedBy) : IRequest<Result>;
public record CreateTransferCommand(int FromWarehouseId, int ToWarehouseId, List<(int productId, decimal qty)> Items, string Notes, int CreatedBy) : IRequest<Result>;
public record CreateAdjustmentCommand(int WarehouseId, List<(int productId, decimal adjQty)> Items, string Reason, int CreatedBy) : IRequest<Result>;

// ════════════════════════════════════════════════════════════════════
// FINANCE
// ════════════════════════════════════════════════════════════════════
namespace CorePOS.Application.Features.Finance.Queries;
using MediatR;
using CorePOS.Application.Common;

public record GetCashboxesQuery(int BranchId) : IRequest<Result<List<CashboxDto>>>;
public record GetCashboxOperationsQuery(int BranchId, DateTime Date) : IRequest<Result<List<CashboxOperationDto>>>;
public record GetExpensesQuery(int BranchId, DateTime Date) : IRequest<Result<List<ExpenseDto>>>;
public record GetDailyCloseSummaryQuery(int BranchId, DateTime Date) : IRequest<Result<DailyCloseSummaryDto>>;

public record CashboxDto(int CashboxId, string Name, bool IsMain, decimal CurrentBalance, decimal OpeningBalance, string BranchName);
public record CashboxOperationDto(DateTime OperationDate, string TypeAr, string Type, string CashboxName, decimal Amount, decimal BalanceAfter, string Notes, string ByUserName);
public record ExpenseDto(int ExpenseId, DateTime Date, string TypeAr, decimal Amount, string Description, string CreatedByName);
public record DailyCloseSummaryDto(decimal OpeningBalance, decimal TotalSales, decimal TotalExpenses, decimal ClosingBalance);

namespace CorePOS.Application.Features.Finance.Commands;
using MediatR;
using CorePOS.Application.Common;

public record CreateCashboxOperationCommand(int CashboxId, string OperationType, decimal Amount, int? TargetCashboxId, string Notes, int CreatedBy) : IRequest<Result>;
public record CreateExpenseCommand(int BranchId, string ExpenseType, decimal Amount, string Description, int CreatedBy) : IRequest<Result>;
public record DeleteExpenseCommand(int ExpenseId, int DeletedBy) : IRequest<Result>;
public record DailyCloseCommand(int BranchId, DateTime Date, int ClosedBy) : IRequest<Result>;
public record CreateCashboxCommand(int BranchId, string Name, bool IsMain, decimal OpeningBalance, int CreatedBy) : IRequest<Result<int>>;
public record UpdateCashboxCommand(int CashboxId, string Name, bool IsMain, decimal OpeningBalance, int ModifiedBy) : IRequest<Result>;

// ════════════════════════════════════════════════════════════════════
// EMPLOYEES
// ════════════════════════════════════════════════════════════════════
namespace CorePOS.Application.Features.Employees.Queries;
using MediatR;
using CorePOS.Application.Common;

public record GetEmployeesListQuery(string Search) : IRequest<Result<List<EmployeeDto>>>;
public record GetEmployeeByIdQuery(int EmployeeId) : IRequest<Result<EmployeeDetailDto>>;
public record EmployeeDto(int EmployeeId, string Name, string JobTitle, string Phone, decimal Salary, DateTime HireDate, bool IsActive);
public record EmployeeDetailDto(int EmployeeId, string Name, string JobTitle, string Phone, decimal Salary, DateTime HireDate, bool IsActive);

namespace CorePOS.Application.Features.Employees.Commands;
using MediatR;
using CorePOS.Application.Common;

public record CreateEmployeeCommand(string Name, string JobTitle, string Phone, decimal Salary, DateTime HireDate, int CreatedBy) : IRequest<Result<int>>;
public record UpdateEmployeeCommand(int EmployeeId, string Name, string JobTitle, string Phone, decimal Salary, DateTime HireDate, bool IsActive, int ModifiedBy) : IRequest<Result>;
public record DeleteEmployeeCommand(int EmployeeId, int DeletedBy) : IRequest<Result>;

// ════════════════════════════════════════════════════════════════════
// REPORTS
// ════════════════════════════════════════════════════════════════════
namespace CorePOS.Application.Features.Reports.Queries;
using MediatR;
using CorePOS.Application.Common;

public record GetSalesReportQuery(int BranchId, DateTime From, DateTime To)       : IRequest<Result<List<SalesReportRowDto>>>;
public record GetProfitReportQuery(int BranchId, DateTime From, DateTime To)       : IRequest<Result<List<ProfitReportRowDto>>>;
public record GetCustomerDebtsReportQuery(int BranchId)                            : IRequest<Result<List<CustomerDebtRowDto>>>;
public record GetSupplierDuesReportQuery(int BranchId)                             : IRequest<Result<List<SupplierDueRowDto>>>;
public record GetStockReportQuery(int BranchId)                                    : IRequest<Result<List<StockReportRowDto>>>;
public record GetLowStockReportQuery(int BranchId)                                 : IRequest<Result<List<LowStockRowDto>>>;
public record GetSlowMovingReportQuery(int BranchId, DateTime From, DateTime To)   : IRequest<Result<List<SlowMovingRowDto>>>;
public record GetExpensesReportQuery(int BranchId, DateTime From, DateTime To)     : IRequest<Result<List<ExpensesReportRowDto>>>;
public record GetCashboxMovementReportQuery(int BranchId, DateTime From, DateTime To) : IRequest<Result<List<CashboxMovementRowDto>>>;
public record GetCashierPerformanceReportQuery(int BranchId, DateTime From, DateTime To) : IRequest<Result<List<CashierPerformanceRowDto>>>;

public record SalesReportRowDto(string InvoiceNo, DateTime Date, string CustomerName, int ItemsCount, decimal Total, decimal Paid, string PayMethodAr);
public record ProfitReportRowDto(string ProductName, decimal QtySold, decimal Sales, decimal Cost, decimal Profit);
public record CustomerDebtRowDto(string Name, string Phone, decimal TotalPurchases, decimal TotalPaid, decimal Balance, DateTime LastInvoiceDate);
public record SupplierDueRowDto(string Name, string Phone, decimal TotalPurchases, decimal TotalPaid, decimal Balance, DateTime LastInvoiceDate);
public record StockReportRowDto(string Barcode, string NameAr, string CategoryName, decimal Quantity, decimal AverageCost);
public record LowStockRowDto(string Barcode, string NameAr, decimal CurrentQty, decimal MinStock, string DefaultSupplier);
public record SlowMovingRowDto(string Barcode, string NameAr, decimal QtySold, decimal CurrentStock, DateTime? LastMovement);
public record ExpensesReportRowDto(DateTime Date, string TypeAr, decimal Amount, string Description, string CreatedByName);
public record CashboxMovementRowDto(DateTime Date, string CashboxName, string TypeAr, decimal Debit, decimal Credit, decimal Balance, string Notes);
public record CashierPerformanceRowDto(string CashierName, int InvoiceCount, decimal TotalSales, decimal AvgInvoice, decimal TotalReturns);

// ════════════════════════════════════════════════════════════════════
// SETTINGS
// ════════════════════════════════════════════════════════════════════
namespace CorePOS.Application.Features.Settings.Queries;
using MediatR;
using CorePOS.Application.Common;

public record GetSettingsQuery() : IRequest<Result<Dictionary<string, string>>>;

namespace CorePOS.Application.Features.Settings.Commands;
using MediatR;
using CorePOS.Application.Common;

public record SaveSettingsCommand(Dictionary<string, string> Settings, int SavedBy) : IRequest<Result>;

// ════════════════════════════════════════════════════════════════════
// USERS
// ════════════════════════════════════════════════════════════════════
namespace CorePOS.Application.Features.Users.Queries;
using MediatR;
using CorePOS.Application.Common;

public record GetUsersListQuery() : IRequest<Result<List<UserListDto>>>;
public record GetUserByIdQuery(int UserId) : IRequest<Result<UserDetailDto>>;
public record GetRolesQuery() : IRequest<Result<List<RoleDto>>>;
public record GetBranchesQuery() : IRequest<Result<List<BranchDto>>>;
public record GetUserPermissionsQuery(int UserId) : IRequest<Result<List<UserPermissionDto>>>;

public record UserListDto(int UserId, string Username, string FullName, string RoleName, string BranchName, bool IsActive, DateTime? LastLoginAt);
public record UserDetailDto(int UserId, string Username, string FullName, string FullNameAr, int RoleId, int BranchId, bool IsActive);
public record RoleDto(int RoleId, string Name, string NameAr);
public record BranchDto(int BranchId, string Name);
public record UserPermissionDto(string Module, string Action);

namespace CorePOS.Application.Features.Users.Commands;
using MediatR;
using CorePOS.Application.Common;

public record CreateUserCommand(string Username, string FullName, string FullNameAr, string Password, int RoleId, int BranchId, int CreatedBy) : IRequest<Result<int>>;
public record UpdateUserCommand(int UserId, string Username, string FullName, string FullNameAr, string? NewPassword, int RoleId, int BranchId, bool IsActive, int ModifiedBy) : IRequest<Result>;
public record SetUserPermissionsCommand(int UserId, List<string> Permissions, int ModifiedBy) : IRequest<Result>;

// ════════════════════════════════════════════════════════════════════
// SHIFTS
// ════════════════════════════════════════════════════════════════════
namespace CorePOS.Application.Features.Shifts.Commands;
using MediatR;
using CorePOS.Application.Common;
// Already defined above via OpenShiftCommand
