using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Warehouses.DTOs;

namespace CorePOS.Application.Features.Warehouses.Queries;

public record GetWarehousesQuery(int? BranchId = null, bool? IsActive = null)
    : IRequest<Result<IReadOnlyList<WarehouseDto>>>;
