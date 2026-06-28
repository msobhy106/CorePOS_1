using MediatR;
using AutoMapper;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Products.DTOs;
using CorePOS.Application.Features.Products.Queries;
using CorePOS.Domain.Interfaces;

namespace CorePOS.Application.Features.Products.Handlers;

public class SearchProductsHandler
    : IRequestHandler<SearchProductsQuery, Result<IReadOnlyList<ProductSearchResultDto>>>
{
    private readonly IProductRepository _repo;
    private readonly IMapper            _mapper;

    public SearchProductsHandler(IProductRepository repo, IMapper mapper)
    {
        _repo   = repo;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<ProductSearchResultDto>>> Handle(
        SearchProductsQuery query, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query.Term))
            return Result<IReadOnlyList<ProductSearchResultDto>>.Success([]);

        var products = await _repo.SearchAsync(query.Term.Trim(), query.MaxResults, ct);
        var dtos = new List<ProductSearchResultDto>();

        foreach (var p in products)
        {
            var dto = _mapper.Map<ProductSearchResultDto>(p);
            if (query.WarehouseId.HasValue)
            {
                var stock = await _repo.GetStockAsync(p.Id, query.WarehouseId.Value, ct);
                dto.CurrentStock = stock?.Quantity ?? 0;
            }
            dtos.Add(dto);
        }

        return Result<IReadOnlyList<ProductSearchResultDto>>.Success(dtos.AsReadOnly());
    }
}
