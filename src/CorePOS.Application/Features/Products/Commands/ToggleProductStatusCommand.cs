using MediatR;
using CorePOS.Application.Common;

namespace CorePOS.Application.Features.Products.Commands;

public record ToggleProductStatusCommand(int Id, bool IsActive) : IRequest<Result>;
