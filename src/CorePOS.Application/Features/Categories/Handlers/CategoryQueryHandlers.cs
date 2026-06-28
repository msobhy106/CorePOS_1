using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Categories.Queries;
using CorePOS.Application.Features.Categories.DTOs;

namespace CorePOS.Application.Features.Categories.Handlers;

public class GetCategoriesHandler : IRequestHandler<GetCategoriesQuery, Result<IReadOnlyList<CategoryDto>>>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public GetCategoriesHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result<IReadOnlyList<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken ct)
    {
        try
        {
            var cats = await _uow.Categories.GetActiveAsync(ct);
            var filtered = cats
                .Where(c => !request.IsActive.HasValue || c.IsActive == request.IsActive.Value)
                .ToList();

            IReadOnlyList<CategoryDto> dtos;
            if (request.TreeStructure)
            {
                // Build tree — only root categories, children nested
                var roots = filtered.Where(c => c.ParentId == null)
                    .Select(c => ToDto(c, filtered)).ToList();
                dtos = roots;
            }
            else
            {
                dtos = filtered.Select(c => ToDto(c, null)).ToList();
            }

            return Result<IReadOnlyList<CategoryDto>>.Success(dtos);
        }
        catch (Exception ex) { return Result<IReadOnlyList<CategoryDto>>.Failure(ex.Message); }
    }

    private static CategoryDto ToDto(Domain.Entities.Category c, List<Domain.Entities.Category>? all)
    {
        var dto = new CategoryDto
        {
            Id         = c.Id,
            Code       = c.Code,
            NameAr     = c.NameAr,
            ParentId   = c.ParentId,
            Level      = (int)c.Level,
            SortOrder  = c.SortOrder,
            IsActive   = c.IsActive
        };
        if (all != null)
            dto.Children = all.Where(x => x.ParentId == c.Id).Select(x => ToDto(x, all)).ToList();
        return dto;
    }
}
