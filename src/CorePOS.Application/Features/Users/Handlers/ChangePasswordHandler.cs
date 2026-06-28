using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Users.Commands;
using CorePOS.Application.Interfaces;
using CorePOS.Domain.Interfaces;

namespace CorePOS.Application.Features.Users.Handlers;

public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IUnitOfWork     _uow;
    private readonly IPasswordHasher _hasher;

    public ChangePasswordHandler(IUnitOfWork uow, IPasswordHasher hasher)
    {
        _uow = uow; _hasher = hasher;
    }

    public async Task<Result> Handle(ChangePasswordCommand cmd, CancellationToken ct)
    {
        var user = await _uow.Users.GetByIdAsync(cmd.UserId, ct);
        if (user is null) return Result.NotFound("المستخدم غير موجود");

        if (!_hasher.Verify(cmd.CurrentPassword, user.PasswordHash))
            return Result.Failure("كلمة المرور الحالية غير صحيحة");

        user.ChangePassword(_hasher.Hash(cmd.NewPassword));
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
