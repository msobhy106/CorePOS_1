using MediatR;
using CorePOS.Application.Common;

namespace CorePOS.Application.Features.Products.Commands;

public record DeleteProductCommand(int Id) : IRequest<Result>;
