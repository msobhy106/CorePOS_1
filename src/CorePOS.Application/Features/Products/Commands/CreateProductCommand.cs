using MediatR;
using CorePOS.Application.Common;

namespace CorePOS.Application.Features.Products.Commands;

public record CreateProductCommand(
    string  Code,
    string  NameAr,
    string? NameEn,
    string? Barcode,
    string? SecondBarcode,
    int     CategoryId,
    int     BaseUnitId,
    int     SaleUnitId,
    int     PurchaseUnitId,
    decimal PurchasePrice,
    decimal SalePrice,
    decimal WholesalePrice      = 0,
    decimal HalfWholesalePrice  = 0,
    decimal SpecialPrice        = 0,
    decimal TaxPercent          = 0,
    decimal MinStock            = 0,
    decimal ReorderLevel        = 0,
    int?    DefaultSupplierId   = null,
    string? Manufacturer        = null,
    string? Description         = null,
    DateOnly? ExpiryDate        = null,
    int     InitialStock        = 0,
    int?    WarehouseId         = null
) : IRequest<Result<int>>;
