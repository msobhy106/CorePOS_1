using Microsoft.EntityFrameworkCore;
using CorePOS.Domain.Entities;

namespace CorePOS.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User>                   Users                  { get; }
    DbSet<Role>                   Roles                  { get; }
    DbSet<Permission>             Permissions            { get; }
    DbSet<RolePermission>         RolePermissions        { get; }
    DbSet<AuditLog>               AuditLogs              { get; }
    DbSet<Branch>                 Branches               { get; }
    DbSet<Warehouse>              Warehouses             { get; }
    DbSet<Category>               Categories             { get; }
    DbSet<Unit>                   Units                  { get; }
    DbSet<Product>                Products               { get; }
    DbSet<ProductUnit>            ProductUnits           { get; }
    DbSet<ProductImage>           ProductImages          { get; }
    DbSet<ProductStock>           ProductStocks          { get; }
    DbSet<PriceList>              PriceLists             { get; }
    DbSet<Customer>               Customers              { get; }
    DbSet<CustomerGroup>          CustomerGroups         { get; }
    DbSet<LoyaltyPoint>           LoyaltyPoints          { get; }
    DbSet<Supplier>               Suppliers              { get; }
    DbSet<Employee>               Employees              { get; }
    DbSet<EmployeeTransaction>    EmployeeTransactions   { get; }
    DbSet<DeliveryAgent>          DeliveryAgents         { get; }
    DbSet<SalesInvoice>           SalesInvoices          { get; }
    DbSet<SalesInvoiceItem>       SalesInvoiceItems      { get; }
    DbSet<SalesReturn>            SalesReturns           { get; }
    DbSet<SalesReturnItem>        SalesReturnItems       { get; }
    DbSet<PurchaseInvoice>        PurchaseInvoices       { get; }
    DbSet<PurchaseInvoiceItem>    PurchaseInvoiceItems   { get; }
    DbSet<PurchaseReturn>         PurchaseReturns        { get; }
    DbSet<PurchaseReturnItem>     PurchaseReturnItems    { get; }
    DbSet<InventoryTransaction>   InventoryTransactions  { get; }
    DbSet<InventorySession>       InventorySessions      { get; }
    DbSet<InventorySessionItem>   InventorySessionItems  { get; }
    DbSet<WarehouseTransfer>      WarehouseTransfers     { get; }
    DbSet<WarehouseTransferItem>  WarehouseTransferItems { get; }
    DbSet<StockAdjustment>        StockAdjustments       { get; }
    DbSet<StockAdjustmentItem>    StockAdjustmentItems   { get; }
    DbSet<CashBox>                CashBoxes              { get; }
    DbSet<Shift>                  Shifts                 { get; }
    DbSet<Expense>                Expenses               { get; }
    DbSet<ExpenseCategory>        ExpenseCategories      { get; }
    DbSet<CustomerPayment>        CustomerPayments       { get; }
    DbSet<SupplierPayment>        SupplierPayments       { get; }
    DbSet<Setting>                Settings               { get; }
    DbSet<License>                Licenses               { get; }
    DbSet<Backup>                 Backups                { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
