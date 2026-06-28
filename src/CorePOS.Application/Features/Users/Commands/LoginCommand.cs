using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Users.DTOs;

namespace CorePOS.Application.Features.Users.Commands;

public record LoginCommand(string Username, string Password) : IRequest<Result<LoginResultDto>>;
