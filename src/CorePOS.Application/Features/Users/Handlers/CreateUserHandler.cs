using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Users.Commands;
using CorePOS.Application.Interfaces;
using CorePOS.Domain.Entities;
using CorePOS.Domain.Interfaces;

namespace CorePOS.Application.Features.Users.Handlers;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, Result<int>>
{
    private readonly IUnitOfWork     _uow;
    private readonly IPasswordHasher _hasher;

    public CreateUserHandler(IUnitOfWork uow, IPasswordHasher hasher)
    {
        _uow    = uow;
        _hasher = hasher;
    }

    public async Task<Result<int>> Handle(CreateUserCommand cmd, CancellationToken ct)
    {
        var hash = _hasher.Hash(cmd.Password);
        var user = User.Create(cmd.Username, hash, cmd.FullName,
            cmd.RoleId, cmd.BranchId, cmd.WarehouseId,
            cmd.FullNameAr, cmd.Email, cmd.Phone);

        await _uow.Users.AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<int>.Success(user.Id);
    }
}
