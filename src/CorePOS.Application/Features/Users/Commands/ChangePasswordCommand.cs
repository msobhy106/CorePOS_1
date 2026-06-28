using MediatR;
using CorePOS.Application.Common;

namespace CorePOS.Application.Features.Users.Commands;

public record ChangePasswordCommand(
    int    UserId,
    string CurrentPassword,
    string NewPassword
) : IRequest<Result>;
