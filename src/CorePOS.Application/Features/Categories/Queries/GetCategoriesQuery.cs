using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Categories.DTOs;

namespace CorePOS.Application.Features.Categories.Queries;

public record GetCategoriesQuery(bool? IsActive = null, bool TreeStructure = false)
    : IRequest<Result<IReadOnlyList<CategoryDto>>>;
