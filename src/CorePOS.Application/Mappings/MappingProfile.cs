using AutoMapper;
using CorePOS.Application.Features.Products.DTOs;
using CorePOS.Application.Features.Customers.DTOs;
using CorePOS.Application.Features.Suppliers.DTOs;
using CorePOS.Application.Features.Sales.DTOs;
using CorePOS.Application.Features.Purchases.DTOs;
using CorePOS.Application.Features.Inventory.DTOs;
using CorePOS.Application.Features.Users.DTOs;
using CorePOS.Application.Features.Branches.DTOs;
using CorePOS.Application.Features.Warehouses.DTOs;
using CorePOS.Application.Features.Employees.DTOs;
using CorePOS.Application.Features.Categories.DTOs;
using CorePOS.Application.Features.Units.DTOs;
using CorePOS.Application.Features.CashBoxes.DTOs;
using CorePOS.Application.Features.Shifts.DTOs;
using CorePOS.Domain.Entities;
using CorePOS.Domain.Enums;

namespace CorePOS.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ── Products ──────────────────────────────────────
        CreateMap<Product, ProductDto>()
            .ForMember(d => d.CategoryName,
                o => o.MapFrom(s => s.Category != null ? s.Category.NameAr : string.Empty))
            .ForMember(d => d.BaseUnitName,
                o => o.MapFrom(s => s.BaseUnit != null ? s.BaseUnit.NameAr : string.Empty))
            .ForMember(d => d.SaleUnitName,
                o => o.MapFrom(s => s.SaleUnit != null ? s.SaleUnit.NameAr : string.Empty))
            .ForMember(d => d.ImagePath,
                o => o.MapFrom(s => s.Images.FirstOrDefault(i => i.IsMain) != null
                    ? s.Images.First(i => i.IsMain).ImagePath : null))
            .ForMember(d => d.CurrentStock, o => o.Ignore())
            .ForMember(d => d.AverageCost,  o => o.Ignore())
            .ForMember(d => d.ProfitMargin, o => o.Ignore());

        CreateMap<Product, ProductListDto>()
            .ForMember(d => d.CategoryName,
                o => o.MapFrom(s => s.Category != null ? s.Category.NameAr : string.Empty))
            .ForMember(d => d.UnitName,
                o => o.MapFrom(s => s.SaleUnit != null ? s.SaleUnit.NameAr : string.Empty))
            .ForMember(d => d.CurrentStock, o => o.Ignore())
            .ForMember(d => d.IsLowStock,   o => o.Ignore())
            .ForMember(d => d.ImagePath,
                o => o.MapFrom(s => s.Images.FirstOrDefault(i => i.IsMain) != null
                    ? s.Images.First(i => i.IsMain).ImagePath : null));

        CreateMap<Product, ProductSearchResultDto>()
            .ForMember(d => d.UnitName,
                o => o.MapFrom(s => s.SaleUnit != null ? s.SaleUnit.NameAr : string.Empty))
            .ForMember(d => d.CurrentStock, o => o.Ignore())
            .ForMember(d => d.ImagePath,
                o => o.MapFrom(s => s.Images.FirstOrDefault(i => i.IsMain) != null
                    ? s.Images.First(i => i.IsMain).ImagePath : null));

        // ── Customers ─────────────────────────────────────
        CreateMap<Customer, CustomerDto>()
            .ForMember(d => d.GroupName,
                o => o.MapFrom(s => s.Group != null ? s.Group.Name : null))
            .ForMember(d => d.PriceListName,
                o => o.MapFrom(s => s.PriceList != null ? s.PriceList.NameAr : null))
            .ForMember(d => d.IsOverCreditLimit,
                o => o.MapFrom(s => s.IsOverCreditLimit()));

        CreateMap<Customer, CustomerListDto>()
            .ForMember(d => d.GroupName,
                o => o.MapFrom(s => s.Group != null ? s.Group.Name : null))
            .ForMember(d => d.IsOverLimit,
                o => o.MapFrom(s => s.IsOverCreditLimit()));

        // ── Suppliers ─────────────────────────────────────
        CreateMap<Supplier, SupplierDto>();
        CreateMap<Supplier, SupplierListDto>();

        // ── Sales ─────────────────────────────────────────
        CreateMap<SalesInvoice, SalesInvoiceDto>()
            .ForMember(d => d.CustomerName,
                o => o.MapFrom(s => s.Customer != null ? s.Customer.Name : null))
            .ForMember(d => d.CustomerPhone,
                o => o.MapFrom(s => s.Customer != null ? s.Customer.Phone : null))
            .ForMember(d => d.BranchName,
                o => o.MapFrom(s => s.Branch != null ? s.Branch.NameAr : string.Empty))
            .ForMember(d => d.WarehouseName,
                o => o.MapFrom(s => s.Warehouse != null ? s.Warehouse.NameAr : string.Empty))
            .ForMember(d => d.CashierName,
                o => o.MapFrom(s => s.User != null ? s.User.FullName : string.Empty))
            .ForMember(d => d.Items,
                o => o.MapFrom(s => s.Items));

        CreateMap<SalesInvoiceItem, SalesInvoiceItemDto>()
            .ForMember(d => d.UnitName,
                o => o.MapFrom(s => s.Unit != null ? s.Unit.NameAr : string.Empty))
            .ForMember(d => d.Profit,
                o => o.MapFrom(s => s.ProfitOnItem));

        CreateMap<SalesInvoice, SalesInvoiceListDto>()
            .ForMember(d => d.CustomerName,
                o => o.MapFrom(s => s.Customer != null ? s.Customer.Name : null))
            .ForMember(d => d.CashierName,
                o => o.MapFrom(s => s.User != null ? s.User.FullName : string.Empty))
            .ForMember(d => d.ItemsCount,
                o => o.MapFrom(s => s.Items.Count));

        // ── Purchases ─────────────────────────────────────
        CreateMap<PurchaseInvoice, PurchaseInvoiceDto>()
            .ForMember(d => d.SupplierName,
                o => o.MapFrom(s => s.Supplier != null ? s.Supplier.Name : null))
            .ForMember(d => d.BranchName,
                o => o.MapFrom(s => s.Branch != null ? s.Branch.NameAr : string.Empty))
            .ForMember(d => d.WarehouseName,
                o => o.MapFrom(s => s.Warehouse != null ? s.Warehouse.NameAr : string.Empty))
            .ForMember(d => d.Items,
                o => o.MapFrom(s => s.Items));

        CreateMap<PurchaseInvoiceItem, PurchaseItemDto>()
            .ForMember(d => d.UnitName,
                o => o.MapFrom(s => s.Unit != null ? s.Unit.NameAr : string.Empty));

        CreateMap<PurchaseInvoice, PurchaseInvoiceListDto>()
            .ForMember(d => d.SupplierName,
                o => o.MapFrom(s => s.Supplier != null ? s.Supplier.Name : null))
            .ForMember(d => d.ItemsCount,
                o => o.MapFrom(s => s.Items.Count));

        // ── Inventory ─────────────────────────────────────
        CreateMap<InventoryTransaction, StockMovementDto>()
            .ForMember(d => d.TransactionType,
                o => o.MapFrom(s => MapTransactionType(s.TransactionType)))
            .ForMember(d => d.Direction,
                o => o.MapFrom(s => s.Direction == StockDirection.In ? "وارد" : "صادر"))
            .ForMember(d => d.CreatedByName,
                o => o.MapFrom(s => s.User != null ? s.User.FullName : null));

        CreateMap<InventorySession, InventorySessionDto>()
            .ForMember(d => d.WarehouseName,
                o => o.MapFrom(s => s.Warehouse != null ? s.Warehouse.NameAr : string.Empty))
            .ForMember(d => d.CountType,
                o => o.MapFrom(s => s.CountType == Domain.Enums.ReturnType.Full ? "جرد كامل" : "جرد جزئي"))
            .ForMember(d => d.Status,
                o => o.MapFrom(s => MapSessionStatus(s.Status)))
            .ForMember(d => d.ItemsCount,
                o => o.MapFrom(s => s.Items.Count))
            .ForMember(d => d.TotalDifference,
                o => o.MapFrom(s => s.Items.Sum(i => i.DifferenceValue)))
            .ForMember(d => d.Items,
                o => o.MapFrom(s => s.Items));

        CreateMap<InventorySessionItem, InventorySessionItemDto>()
            .ForMember(d => d.ProductName,
                o => o.MapFrom(s => s.Product != null ? s.Product.NameAr : string.Empty))
            .ForMember(d => d.Barcode,
                o => o.MapFrom(s => s.Product != null ? s.Product.Barcode : null))
            .ForMember(d => d.Difference,
                o => o.MapFrom(s => s.Difference))
            .ForMember(d => d.DifferenceValue,
                o => o.MapFrom(s => s.DifferenceValue));

        // ── Users ─────────────────────────────────────────
        CreateMap<User, UserDto>()
            .ForMember(d => d.RoleName,
                o => o.MapFrom(s => s.Role != null ? s.Role.Name : string.Empty))
            .ForMember(d => d.RoleNameAr,
                o => o.MapFrom(s => s.Role != null ? s.Role.NameAr : string.Empty))
            .ForMember(d => d.BranchName,
                o => o.MapFrom(s => s.Branch != null ? s.Branch.NameAr : null))
            .ForMember(d => d.WarehouseName,
                o => o.MapFrom(s => s.Warehouse != null ? s.Warehouse.NameAr : null));

        // ── Branches & Warehouses ─────────────────────────
        CreateMap<Branch, BranchDto>()
            .ForMember(d => d.NameAr, o => o.MapFrom(s => s.NameAr));

        CreateMap<Warehouse, WarehouseDto>()
            .ForMember(d => d.BranchName,
                o => o.MapFrom(s => s.Branch != null ? s.Branch.NameAr : string.Empty));

        // ── Employees ─────────────────────────────────────
        CreateMap<Employee, EmployeeDto>()
            .ForMember(d => d.BranchName,
                o => o.MapFrom(s => s.Branch != null ? s.Branch.NameAr : null))
            .ForMember(d => d.TotalAdvances,
                o => o.MapFrom(s => s.Transactions
                    .Where(t => t.Type == EmployeeTransactionType.Advance)
                    .Sum(t => t.Amount)))
            .ForMember(d => d.TotalDeductions,
                o => o.MapFrom(s => s.Transactions
                    .Where(t => t.Type == EmployeeTransactionType.Deduction)
                    .Sum(t => t.Amount)))
            .ForMember(d => d.TotalBonuses,
                o => o.MapFrom(s => s.Transactions
                    .Where(t => t.Type == EmployeeTransactionType.Bonus)
                    .Sum(t => t.Amount)));

        // ── Categories & Units ────────────────────────────
        CreateMap<Category, CategoryDto>()
            .ForMember(d => d.NameEn,   o => o.MapFrom(s => s.Name))
            .ForMember(d => d.ParentName,
                o => o.MapFrom(s => s.Parent != null ? s.Parent.NameAr : null))
            .ForMember(d => d.Level,    o => o.MapFrom(s => (int)s.Level))
            .ForMember(d => d.Children, o => o.MapFrom(s => s.Children));

        CreateMap<Unit, UnitDto>()
            .ForMember(d => d.NameEn, o => o.MapFrom(s => s.Name));

        // ── CashBoxes & Shifts ────────────────────────────
        CreateMap<CashBox, CashBoxDto>()
            .ForMember(d => d.BranchName,
                o => o.MapFrom(s => s.Branch != null ? s.Branch.NameAr : string.Empty))
            .ForMember(d => d.HasOpenShift,   o => o.Ignore())
            .ForMember(d => d.CurrentCashier, o => o.Ignore());

        CreateMap<Shift, ShiftDto>()
            .ForMember(d => d.CashierName,
                o => o.MapFrom(s => s.User != null ? s.User.FullName : string.Empty))
            .ForMember(d => d.BranchName,
                o => o.MapFrom(s => s.Branch != null ? s.Branch.NameAr : string.Empty))
            .ForMember(d => d.CashBoxName,
                o => o.MapFrom(s => s.CashBox != null ? s.CashBox.NameAr : string.Empty))
            .ForMember(d => d.Status,
                o => o.MapFrom(s => s.IsOpen ? "مفتوحة" : "مغلقة"))
            .ForMember(d => d.SalesCount,   o => o.Ignore())
            .ForMember(d => d.SalesRevenue, o => o.Ignore())
            .ForMember(d => d.TotalExpenses,o => o.Ignore());
    }

        // ── Expenses ──────────────────────────────────────────────── (BUG-049)
        CreateMap<Expense, Features.Expenses.DTOs.ExpenseDto>()
            .ForMember(d => d.CategoryName,
                o => o.MapFrom(s => s.Category != null ? s.Category.NameAr : string.Empty))
            .ForMember(d => d.BranchName,
                o => o.MapFrom(s => s.Branch != null ? s.Branch.NameAr : string.Empty))
            .ForMember(d => d.CreatedBy, o => o.Ignore());

        // ── Branches ──────────────────────────────────────────────── (BUG-049)
        CreateMap<Branch, Features.Branches.DTOs.BranchDto>();

        // ── Warehouses ────────────────────────────────────────────── (BUG-049)
        CreateMap<Warehouse, Features.Warehouses.DTOs.WarehouseDto>()
            .ForMember(d => d.BranchName,
                o => o.MapFrom(s => s.Branch != null ? s.Branch.NameAr : string.Empty));

        // ── Backup ────────────────────────────────────────────────── (BUG-049)
        CreateMap<Backup, Features.Backup.DTOs.BackupFileDto>()
            .ForMember(d => d.DateDisplay,
                o => o.MapFrom(s => s.CreatedAt.ToString("dd/MM/yyyy HH:mm")))
            .ForMember(d => d.FileSizeDisplay,
                o => o.MapFrom(s => s.FileSizeBytes > 1_048_576
                    ? $"{s.FileSizeBytes / 1_048_576.0:N1} MB"
                    : $"{s.FileSizeBytes / 1024.0:N1} KB"))
            .ForMember(d => d.IsEncrypted, o => o.Ignore())
            .ForMember(d => d.BackupType,
                o => o.MapFrom(s => s.BackupType.ToString()))
            .ForMember(d => d.Label, o => o.Ignore());
    }

    private static string MapTransactionType(InventoryTransactionType t) => t switch
    {
        InventoryTransactionType.OpeningBalance       => "رصيد افتتاحي",
        InventoryTransactionType.SaleOut              => "بيع",
        InventoryTransactionType.SaleReturnIn         => "مرتجع بيع",
        InventoryTransactionType.PurchaseIn           => "شراء",
        InventoryTransactionType.PurchaseReturnOut    => "مرتجع شراء",
        InventoryTransactionType.TransferOut          => "تحويل خروج",
        InventoryTransactionType.TransferIn           => "تحويل دخول",
        InventoryTransactionType.AdjustmentPlus       => "تسوية زيادة",
        InventoryTransactionType.AdjustmentMinus      => "تسوية نقص",
        InventoryTransactionType.InventoryCountAdjust => "جرد",
        _                                             => "أخرى"
    };

    private static string MapSessionStatus(int status) => status switch
    {
        0 => "مفتوحة",
        1 => "معتمدة",
        2 => "ملغاة",
        _ => "غير معروف"
    };
}
