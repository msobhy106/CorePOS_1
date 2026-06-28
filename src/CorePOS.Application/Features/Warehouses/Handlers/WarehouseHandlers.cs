using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Warehouses.Commands;
using CorePOS.Application.Features.Warehouses.Queries;
using CorePOS.Application.Features.Warehouses.DTOs;

namespace CorePOS.Application.Features.Warehouses.Handlers;

public class GetWarehousesHandler : IRequestHandler<GetWarehousesQuery, Result<IReadOnlyList<WarehouseDto>>>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public GetWarehousesHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result<IReadOnlyList<WarehouseDto>>> Handle(GetWarehousesQuery request, CancellationToken ct)
    {
        try
        {
            var warehouses = request.BranchId.HasValue
                ? await _uow.Warehouses.GetByBranchAsync(request.BranchId.Value, ct)
                : await _uow.Warehouses.GetActiveAsync(ct);

            var dtos = warehouses
                .Where(w => !request.IsActive.HasValue || w.IsActive == request.IsActive.Value)
                .Select(w => new WarehouseDto
                {
                    Id          = w.Id,
                    Code        = w.Code,
                    NameAr      = w.NameAr,
                    BranchId    = w.BranchId,
                    BranchName  = w.Branch?.NameAr ?? "",
                    Address     = w.Address,
                    ManagerName = w.ManagerName,
                    IsMain      = w.IsMain,
                    IsActive    = w.IsActive
                }).ToList();

            return Result<IReadOnlyList<WarehouseDto>>.Success(dtos);
        }
        catch (Exception ex) { return Result<IReadOnlyList<WarehouseDto>>.Failure(ex.Message); }
    }
}

public class CreateWarehouseHandler : IRequestHandler<CreateWarehouseCommand, Result<int>>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public CreateWarehouseHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result<int>> Handle(CreateWarehouseCommand request, CancellationToken ct)
    {
        try
        {
            if (await _uow.Warehouses.CodeExistsAsync(request.Code, null, ct))
                return Result<int>.Failure($"كود المستودع '{request.Code}' مستخدم بالفعل");

            var wh = Domain.Entities.Warehouse.Create(
                request.Code, request.Name, request.NameAr, request.BranchId,
                request.Address, request.ManagerName, request.IsMain);

            await _uow.Warehouses.AddAsync(wh, ct);
            await _uow.SaveChangesAsync(ct);
            return Result<int>.Success(wh.Id);
        }
        catch (Exception ex) { return Result<int>.Failure(ex.Message); }
    }
}
