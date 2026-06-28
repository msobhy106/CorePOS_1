using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Users.Queries;
using CorePOS.Application.Features.Users.DTOs;

namespace CorePOS.Application.Features.Users.Handlers;

public class GetUsersHandler : IRequestHandler<GetUsersQuery, Result<IReadOnlyList<UserDto>>>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public GetUsersHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result<IReadOnlyList<UserDto>>> Handle(GetUsersQuery request, CancellationToken ct)
    {
        try
        {
            var users = request.IsActive.HasValue
                ? await _uow.Users.GetAsync(u => u.IsActive == request.IsActive.Value, ct)
                : await _uow.Users.GetAllAsync(ct);

            var dtos = users.Select(u => new UserDto
            {
                Id           = u.Id,
                Username     = u.Username,
                FullName     = u.FullName,
                FullNameAr   = u.FullNameAr,
                IsActive     = u.IsActive,
                RoleId       = u.RoleId,
                RoleName     = u.Role?.Name ?? "",
                RoleNameAr   = u.Role?.NameAr ?? "",
                BranchId     = u.BranchId,
                WarehouseId  = u.WarehouseId,
                LastLogin    = u.LastLogin
            }).ToList();

            return Result<IReadOnlyList<UserDto>>.Success(dtos);
        }
        catch (Exception ex) { return Result<IReadOnlyList<UserDto>>.Failure(ex.Message); }
    }
}

public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public GetUserByIdHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken ct)
    {
        try
        {
            var u = await _uow.Users.GetByIdAsync(request.Id, ct);
            if (u == null) return Result<UserDto>.Failure("المستخدم غير موجود");

            return Result<UserDto>.Success(new UserDto
            {
                Id            = u.Id,
                Username      = u.Username,
                FullName      = u.FullName,
                FullNameAr    = u.FullNameAr,
                Email         = u.Email,
                Phone         = u.Phone,
                PhotoPath     = u.PhotoPath,
                RoleId        = u.RoleId,
                RoleName      = u.Role?.Name ?? "",
                RoleNameAr    = u.Role?.NameAr ?? "",
                BranchId      = u.BranchId,
                BranchName    = u.Branch?.NameAr,
                WarehouseId   = u.WarehouseId,
                WarehouseName = u.Warehouse?.NameAr,
                IsActive      = u.IsActive,
                LastLogin     = u.LastLogin
            });
        }
        catch (Exception ex) { return Result<UserDto>.Failure(ex.Message); }
    }
}
