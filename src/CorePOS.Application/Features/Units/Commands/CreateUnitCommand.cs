using MediatR;
using CorePOS.Application.Common;

namespace CorePOS.Application.Features.Units.Commands;

public record CreateUnitCommand(
    string  Code,
    string  Name,
    string  NameAr,
    string? Abbreviation = null
) : IRequest<Result<int>>;
