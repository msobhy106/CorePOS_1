using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Products.Commands;
using CorePOS.Application.Interfaces;
using CorePOS.Domain.Entities;
using CorePOS.Domain.Interfaces;

namespace CorePOS.Application.Features.Products.Handlers;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, Result<int>>
{
    private readonly IUnitOfWork         _uow;
    private readonly ICurrentUserService _user;

    public CreateProductHandler(IUnitOfWork uow, ICurrentUserService user)
    {
        _uow  = uow;
        _user = user;
    }

    public async Task<Result<int>> Handle(CreateProductCommand cmd, CancellationToken ct)
    {
        var product = Product.Create(
            cmd.Code, cmd.NameAr, cmd.CategoryId,
            cmd.BaseUnitId, cmd.SaleUnitId, cmd.PurchaseUnitId,
            cmd.PurchasePrice, cmd.SalePrice, cmd.Barcode, cmd.NameEn);

        product.UpdateDetails(cmd.NameAr, cmd.NameEn, cmd.CategoryId,
            cmd.Barcode, cmd.SecondBarcode, cmd.Manufacturer,
            cmd.Description, cmd.DefaultSupplierId);

        product.UpdatePrices(cmd.PurchasePrice, cmd.SalePrice,
            cmd.WholesalePrice, cmd.HalfWholesalePrice,
            cmd.SpecialPrice, cmd.TaxPercent);

        product.SetStockThresholds(cmd.MinStock, cmd.ReorderLevel);
        if (cmd.ExpiryDate.HasValue) product.SetExpiry(cmd.ExpiryDate);

        await _uow.Products.AddAsync(product, ct);
        await _uow.SaveChangesAsync(ct);

        // Initialize stock if warehouse specified
        if (cmd.InitialStock > 0 && cmd.WarehouseId.HasValue)
        {
            var stock = ProductStock.Create(product.Id, cmd.WarehouseId.Value,
                cmd.InitialStock, cmd.PurchasePrice);
            await _uow.Inventory.UpsertStockAsync(stock, ct);
            await _uow.Inventory.LogTransactionAsync(
                product.Id, cmd.WarehouseId.Value,
                cmd.InitialStock, CorePOS.Domain.Enums.StockDirection.In,
                CorePOS.Domain.Enums.InventoryTransactionType.OpeningBalance,
                cmd.PurchasePrice, null, null, "رصيد افتتاحي", _user.UserId, ct);
            await _uow.SaveChangesAsync(ct);
        }

        return Result<int>.Success(product.Id);
    }
}
