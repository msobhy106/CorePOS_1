using MediatR;
using CorePOS.Application.Common;

namespace CorePOS.Application.Features.Categories.Commands;

public record CreateCategoryCommand(
    string  Code,
    string  Name,
    string  NameAr,
    int?    ParentId  = null,
    int     SortOrder = 0
) : IRequest<Result<int>>;
