using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Products.Commands;
using CorePOS.Domain.Interfaces;

namespace CorePOS.Application.Features.Products.Handlers;

public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, Result>
{
    private readonly IUnitOfWork _uow;
    public UpdateProductHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(UpdateProductCommand cmd, CancellationToken ct)
    {
        var product = await _uow.Products.GetByIdAsync(cmd.Id, ct);
        if (product is null) return Result.NotFound("الصنف غير موجود");

        product.UpdateDetails(cmd.NameAr, cmd.NameEn, cmd.CategoryId,
            cmd.Barcode, cmd.SecondBarcode, cmd.Manufacturer,
            cmd.Description, cmd.DefaultSupplierId);

        product.UpdatePrices(cmd.PurchasePrice, cmd.SalePrice,
            cmd.WholesalePrice, cmd.HalfWholesalePrice,
            cmd.SpecialPrice, cmd.TaxPercent);

        product.SetStockThresholds(cmd.MinStock, cmd.ReorderLevel);
        product.SetExpiry(cmd.ExpiryDate);

        _uow.Products.Update(product);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
