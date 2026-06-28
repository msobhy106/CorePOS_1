using Microsoft.EntityFrameworkCore.Storage;
using CorePOS.Domain.Interfaces;
using CorePOS.Persistence.DbContexts;

namespace CorePOS.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly CorePOSDbContext _db;
    private IDbContextTransaction?   _transaction;

    // ── Lazy-initialized repositories ─────────────────────
    private IProductRepository?   _products;
    private ICustomerRepository?  _customers;
    private ISupplierRepository?  _suppliers;
    private ISalesRepository?     _sales;
    private IPurchaseRepository?  _purchases;
    private IInventoryRepository? _inventory;
    private IUserRepository?      _users;
    private ISettingsRepository?  _settings;
    private ILicenseRepository?   _licenses;
    // Added (BUG-067)
    private IBranchRepository?    _branches;
    private IWarehouseRepository? _warehouses;
    private ICashBoxRepository?   _cashBoxes;
    private IExpenseRepository?   _expenses;
    private IShiftRepository?     _shifts;
    private IEmployeeRepository?  _employees;
    private ICategoryRepository?  _categories;
    private IBackupRepository?    _backups;

    public UnitOfWork(CorePOSDbContext db) => _db = db;

    public IProductRepository   Products   => _products   ??= new ProductRepository(_db);
    public ICustomerRepository  Customers  => _customers  ??= new CustomerRepository(_db);
    public ISupplierRepository  Suppliers  => _suppliers  ??= new SupplierRepository(_db);
    public ISalesRepository     Sales      => _sales      ??= new SalesRepository(_db);
    public IPurchaseRepository  Purchases  => _purchases  ??= new PurchaseRepository(_db);
    public IInventoryRepository Inventory  => _inventory  ??= new InventoryRepository(_db);
    public IUserRepository      Users      => _users      ??= new UserRepository(_db);
    public ISettingsRepository  Settings   => _settings   ??= new SettingsRepository(_db);
    public ILicenseRepository   Licenses   => _licenses   ??= new LicenseRepository(_db);
    // Added (BUG-067)
    public IBranchRepository    Branches   => _branches   ??= new BranchRepository(_db);
    public IWarehouseRepository Warehouses => _warehouses ??= new WarehouseRepository(_db);
    public ICashBoxRepository   CashBoxes  => _cashBoxes  ??= new CashBoxRepository(_db);
    public IExpenseRepository   Expenses   => _expenses   ??= new ExpenseRepository(_db);
    public IShiftRepository     Shifts     => _shifts     ??= new ShiftRepository(_db);
    public IEmployeeRepository  Employees  => _employees  ??= new EmployeeRepository(_db);
    public ICategoryRepository  Categories => _categories ??= new CategoryRepository(_db);
    public IBackupRepository    Backups    => _backups    ??= new BackupRepository(_db);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);

    public async Task BeginTransactionAsync(CancellationToken ct = default)
        => _transaction = await _db.Database.BeginTransactionAsync(ct);

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is null) return;
        await _transaction.CommitAsync(ct);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is null) return;
        await _transaction.RollbackAsync(ct);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _db.Dispose();
    }
}
