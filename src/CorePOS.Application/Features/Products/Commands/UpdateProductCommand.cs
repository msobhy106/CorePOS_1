using MediatR;
using CorePOS.Application.Common;

namespace CorePOS.Application.Features.Products.Commands;

public record UpdateProductCommand(
    int     Id,
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
    decimal WholesalePrice,
    decimal HalfWholesalePrice,
    decimal SpecialPrice,
    decimal TaxPercent,
    decimal MinStock,
    decimal ReorderLevel,
    int?    DefaultSupplierId,
    string? Manufacturer,
    string? Description,
    DateOnly? ExpiryDate
) : IRequest<Result>;
