namespace CorePOS.Domain.Interfaces;

/// <summary>
/// Unit of Work — coordinates multiple repositories in a single transaction.
/// All repositories are exposed as properties so the UI/Application layer
/// accesses everything through one interface.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // Existing repositories
    IProductRepository     Products     { get; }
    ICustomerRepository    Customers    { get; }
    ISupplierRepository    Suppliers    { get; }
    ISalesRepository       Sales        { get; }
    IPurchaseRepository    Purchases    { get; }
    IInventoryRepository   Inventory    { get; }
    IUserRepository        Users        { get; }
    ISettingsRepository    Settings     { get; }
    ILicenseRepository     Licenses     { get; }

    // Added repositories (BUG-006)
    IBranchRepository      Branches     { get; }
    IWarehouseRepository   Warehouses   { get; }
    ICashBoxRepository     CashBoxes    { get; }
    IExpenseRepository     Expenses     { get; }
    IShiftRepository       Shifts       { get; }
    IEmployeeRepository    Employees    { get; }
    ICategoryRepository    Categories   { get; }
    IBackupRepository      Backups      { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
