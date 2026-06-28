using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Users.Commands;
using CorePOS.Application.Features.Users.DTOs;
using CorePOS.Application.Interfaces;
using CorePOS.Domain.Interfaces;

namespace CorePOS.Application.Features.Users.Handlers;

public class LoginHandler : IRequestHandler<LoginCommand, Result<LoginResultDto>>
{
    private readonly IUserRepository     _users;
    private readonly IPasswordHasher     _hasher;
    private readonly ICurrentUserService _session;
    private readonly ILicenseService     _license;

    public LoginHandler(IUserRepository users, IPasswordHasher hasher,
        ICurrentUserService session, ILicenseService license)
    {
        _users   = users;
        _hasher  = hasher;
        _session = session;
        _license = license;
    }

    public async Task<Result<LoginResultDto>> Handle(LoginCommand cmd, CancellationToken ct)
    {
        // 1. Check license
        if (!await _license.IsValidAsync(ct))
            return Result<LoginResultDto>.Failure("انتهت صلاحية الترخيص. يرجى تفعيل البرنامج.", 403);

        // 2. Find user
        var user = await _users.GetByUsernameAsync(cmd.Username.Trim().ToLowerInvariant(), ct);
        if (user is null || !user.IsActive)
            return Result<LoginResultDto>.Failure("اسم المستخدم أو كلمة المرور غير صحيحة", 401);

        // 3. Verify password
        if (!_hasher.Verify(cmd.Password, user.PasswordHash))
            return Result<LoginResultDto>.Failure("اسم المستخدم أو كلمة المرور غير صحيحة", 401);

        // 4. Load permissions
        var permissions = await _users.GetPermissionKeysForUserAsync(user.Id, ct);

        // 5. Set session
        _session.SetUser(user.Id, user.Username, user.FullName, user.RoleId,
            user.BranchId, user.WarehouseId, permissions);

        // 6. Record last login
        user.RecordLogin();
        _users.Update(user);
        await _users.LogAuditAsync(user.Id, "Login", "User", user.Id.ToString(),
            null, null, Environment.MachineName, ct);

        var role = await _users.GetRoleByIdAsync(user.RoleId, ct);

        return Result<LoginResultDto>.Success(new LoginResultDto
        {
            Success = true,
            User    = new UserDto
            {
                Id          = user.Id,
                Username    = user.Username,
                FullName    = user.FullName,
                FullNameAr  = user.FullNameAr,
                Email       = user.Email,
                Phone       = user.Phone,
                PhotoPath   = user.PhotoPath,
                RoleId      = user.RoleId,
                RoleName    = role?.Name ?? string.Empty,
                RoleNameAr  = role?.NameAr ?? string.Empty,
                BranchId    = user.BranchId,
                WarehouseId = user.WarehouseId,
                IsActive    = user.IsActive,
                LastLogin   = user.LastLogin
            },
            Permissions = permissions
        });
    }
}
