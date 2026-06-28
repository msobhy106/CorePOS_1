using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Users.DTOs;

namespace CorePOS.Application.Features.Users.Queries;

public record GetUsersQuery(bool? IsActive = null) : IRequest<Result<IReadOnlyList<UserDto>>>;
public record GetUserByIdQuery(int Id)             : IRequest<Result<UserDto>>;
