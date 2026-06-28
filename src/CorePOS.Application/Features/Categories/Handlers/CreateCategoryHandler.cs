using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Categories.Commands;
using CorePOS.Domain.Entities;
using CorePOS.Domain.Interfaces;

namespace CorePOS.Application.Features.Categories.Handlers;

public class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, Result<int>>
{
    private readonly IUnitOfWork _uow;
    public CreateCategoryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<int>> Handle(CreateCategoryCommand cmd, CancellationToken ct)
    {
        var category = cmd.ParentId.HasValue
            ? Category.CreateSub(cmd.Code, cmd.Name, cmd.NameAr, cmd.ParentId.Value, cmd.SortOrder)
            : Category.CreateMain(cmd.Code, cmd.Name, cmd.NameAr, cmd.SortOrder);

        await _uow.SaveChangesAsync(ct);
        return Result<int>.Success(category.Id);
    }
}
