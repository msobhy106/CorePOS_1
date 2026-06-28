using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Products.Queries;
using CorePOS.Application.Features.Products.DTOs;

namespace CorePOS.Application.Features.Products.Handlers;

// ── GET PRODUCTS (paged) ────────────────────────────────────────
public class GetProductsHandler : IRequestHandler<GetProductsQuery, Result<PagedResult<ProductListDto>>>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public GetProductsHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result<PagedResult<ProductListDto>>> Handle(GetProductsQuery request, CancellationToken ct)
    {
        try
        {
            var products = request.CategoryId.HasValue
                ? await _uow.Products.GetByCategoryAsync(request.CategoryId.Value, true, ct)
                : await _uow.Products.GetActiveAsync(ct);

            var filtered = products.AsEnumerable();
            if (request.IsActive.HasValue)  filtered = filtered.Where(p => p.IsActive == request.IsActive.Value);
            if (!string.IsNullOrEmpty(request.Search))
            {
                var s = request.Search.ToLower();
                filtered = filtered.Where(p =>
                    p.NameAr.ToLower().Contains(s) ||
                    p.Code.ToLower().Contains(s) ||
                    (p.Barcode != null && p.Barcode.Contains(s)));
            }

            var list  = filtered.ToList();
            var total = list.Count;
            var items = list
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(p => new ProductListDto
                {
                    Id           = p.Id,
                    Code         = p.Code,
                    Barcode      = p.Barcode,
                    NameAr       = p.NameAr,
                    CategoryName = p.Category?.NameAr ?? "",
                    UnitName     = p.SaleUnit?.NameAr ?? "",
                    SalePrice    = p.SalePrice,
                    CurrentStock = 0,   // loaded separately per warehouse if needed
                    IsActive     = p.IsActive,
                    IsLowStock   = false,
                    ImagePath    = p.ImagePath
                }).ToList();

            return Result<PagedResult<ProductListDto>>.Success(
                new PagedResult<ProductListDto>(items, total, request.PageNumber, request.PageSize));
        }
        catch (Exception ex) { return Result<PagedResult<ProductListDto>>.Failure(ex.Message); }
    }
}

// ── GET LOW STOCK ───────────────────────────────────────────────
public class GetLowStockProductsHandler : IRequestHandler<GetLowStockProductsQuery, Result<IReadOnlyList<ProductListDto>>>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public GetLowStockProductsHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result<IReadOnlyList<ProductListDto>>> Handle(GetLowStockProductsQuery request, CancellationToken ct)
    {
        try
        {
            var products = await _uow.Products.GetLowStockAsync(request.WarehouseId, ct);
            var dtos = products.Select(p => new ProductListDto
            {
                Id           = p.Id,
                Code         = p.Code,
                Barcode      = p.Barcode,
                NameAr       = p.NameAr,
                CategoryName = p.Category?.NameAr ?? "",
                UnitName     = p.SaleUnit?.NameAr ?? "",
                SalePrice    = p.SalePrice,
                IsActive     = p.IsActive,
                IsLowStock   = true,
                ImagePath    = p.ImagePath
            }).ToList();
            return Result<IReadOnlyList<ProductListDto>>.Success(dtos);
        }
        catch (Exception ex) { return Result<IReadOnlyList<ProductListDto>>.Failure(ex.Message); }
    }
}
