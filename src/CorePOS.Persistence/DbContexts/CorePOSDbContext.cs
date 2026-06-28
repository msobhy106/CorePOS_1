using Microsoft.EntityFrameworkCore;
using CorePOS.Application.Interfaces;
using CorePOS.Domain.Common;
using CorePOS.Domain.Entities;

namespace CorePOS.Persistence.DbContexts;

public class CorePOSDbContext : DbContext, IApplicationDbContext
{
    public CorePOSDbContext(DbContextOptions<CorePOSDbContext> options) : base(options) { }

    // ── System & Security ────────────────────────────────
    public DbSet<User>             Users             => Set<User>();
    public DbSet<Role>             Roles             => Set<Role>();
    public DbSet<Permission>       Permissions       => Set<Permission>();
    public DbSet<RolePermission>   RolePermissions   => Set<RolePermission>();
    public DbSet<AuditLog>         AuditLogs         => Set<AuditLog>();
    public DbSet<License>          Licenses          => Set<License>();

    // ── Master Data ──────────────────────────────────────
    public DbSet<Branch>           Branches          => Set<Branch>();
    public DbSet<Warehouse>        Warehouses        => Set<Warehouse>();
    public DbSet<Category>         Categories        => Set<Category>();
    public DbSet<Unit>             Units             => Set<Unit>();
    public DbSet<Product>          Products          => Set<Product>();
    public DbSet<ProductUnit>      ProductUnits      => Set<ProductUnit>();
    public DbSet<ProductImage>     ProductImages     => Set<ProductImage>();
    public DbSet<ProductStock>     ProductStocks     => Set<ProductStock>();
    public DbSet<PriceList>        PriceLists        => Set<PriceList>();

    // ── People ───────────────────────────────────────────
    public DbSet<Customer>         Customers         => Set<Customer>();
    public DbSet<CustomerGroup>    CustomerGroups    => Set<CustomerGroup>();
    public DbSet<LoyaltyPoint>     LoyaltyPoints     => Set<LoyaltyPoint>();
    public DbSet<Supplier>         Suppliers         => Set<Supplier>();
    public DbSet<Employee>         Employees         => Set<Employee>();
    public DbSet<EmployeeTransaction> EmployeeTransactions => Set<EmployeeTransaction>();
    public DbSet<DeliveryAgent>    DeliveryAgents    => Set<DeliveryAgent>();

    // ── Sales ────────────────────────────────────────────
    public DbSet<SalesInvoice>     SalesInvoices     => Set<SalesInvoice>();
    public DbSet<SalesInvoiceItem> SalesInvoiceItems => Set<SalesInvoiceItem>();
    public DbSet<SalesReturn>      SalesReturns      => Set<SalesReturn>();
    public DbSet<SalesReturnItem>  SalesReturnItems  => Set<SalesReturnItem>();

    // ── Purchases ────────────────────────────────────────
    public DbSet<PurchaseInvoice>     PurchaseInvoices     => Set<PurchaseInvoice>();
    public DbSet<PurchaseInvoiceItem> PurchaseInvoiceItems => Set<PurchaseInvoiceItem>();
    public DbSet<PurchaseReturn>      PurchaseReturns      => Set<PurchaseReturn>();
    public DbSet<PurchaseReturnItem>  PurchaseReturnItems  => Set<PurchaseReturnItem>();

    // ── Inventory ────────────────────────────────────────
    public DbSet<InventoryTransaction>   InventoryTransactions  => Set<InventoryTransaction>();
    public DbSet<InventorySession>       InventorySessions      => Set<InventorySession>();
    public DbSet<InventorySessionItem>   InventorySessionItems  => Set<InventorySessionItem>();
    public DbSet<WarehouseTransfer>      WarehouseTransfers     => Set<WarehouseTransfer>();
    public DbSet<WarehouseTransferItem>  WarehouseTransferItems => Set<WarehouseTransferItem>();
    public DbSet<StockAdjustment>        StockAdjustments       => Set<StockAdjustment>();
    public DbSet<StockAdjustmentItem>    StockAdjustmentItems   => Set<StockAdjustmentItem>();

    // ── Finance ──────────────────────────────────────────
    public DbSet<CashBox>           CashBoxes          => Set<CashBox>();
    public DbSet<Shift>             Shifts             => Set<Shift>();
    public DbSet<Expense>           Expenses           => Set<Expense>();
    public DbSet<ExpenseCategory>   ExpenseCategories  => Set<ExpenseCategory>();
    public DbSet<CustomerPayment>   CustomerPayments   => Set<CustomerPayment>();
    public DbSet<SupplierPayment>   SupplierPayments   => Set<SupplierPayment>();

    // ── Settings ─────────────────────────────────────────
    public DbSet<Setting>  Settings => Set<Setting>();
    public DbSet<Backup>   Backups  => Set<Backup>();

    // ── Model Builder ─────────────────────────────────────
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all IEntityTypeConfiguration<T> from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CorePOSDbContext).Assembly);

        // Global query filter: exclude soft-deleted entities
        modelBuilder.Entity<Product>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Customer>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Supplier>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Branch>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Warehouse>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Category>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Employee>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SalesInvoice>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<PurchaseInvoice>().HasQueryFilter(e => !e.IsDeleted);
    }

    // ── SaveChanges override: auto set UpdatedAt ──────────
    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
        return await base.SaveChangesAsync(ct);
    }

    public override int SaveChanges()
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
        return base.SaveChanges();
    }
}
// (SequenceRecord is mapped in OnModelCreating extension)
