using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Products.DTOs;

namespace CorePOS.Application.Features.Products.Queries;

public record GetProductByIdQuery(int Id, int? WarehouseId = null)
    : IRequest<Result<ProductDto>>;
