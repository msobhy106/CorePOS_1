using MediatR;
using AutoMapper;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Products.DTOs;
using CorePOS.Application.Features.Products.Queries;
using CorePOS.Domain.Interfaces;

namespace CorePOS.Application.Features.Products.Handlers;

public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    private readonly IProductRepository _repo;
    private readonly IMapper            _mapper;

    public GetProductByIdHandler(IProductRepository repo, IMapper mapper)
    {
        _repo   = repo;
        _mapper = mapper;
    }

    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery query, CancellationToken ct)
    {
        var product = await _repo.GetByIdWithDetailsAsync(query.Id, ct);
        if (product is null) return Result<ProductDto>.NotFound("الصنف غير موجود");

        var dto = _mapper.Map<ProductDto>(product);

        if (query.WarehouseId.HasValue)
        {
            var stock = await _repo.GetStockAsync(product.Id, query.WarehouseId.Value, ct);
            dto.CurrentStock = stock?.Quantity ?? 0;
            dto.AverageCost  = stock?.AverageCost ?? 0;
        }

        dto.ProfitMargin = product.CalculateProfitMargin();
        return Result<ProductDto>.Success(dto);
    }
}
