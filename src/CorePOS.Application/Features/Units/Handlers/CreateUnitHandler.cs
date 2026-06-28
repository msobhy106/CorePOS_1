using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Units.Commands;
using CorePOS.Domain.Entities;
using CorePOS.Domain.Interfaces;

namespace CorePOS.Application.Features.Units.Handlers;

public class CreateUnitHandler : IRequestHandler<CreateUnitCommand, Result<int>>
{
    private readonly IUnitOfWork _uow;
    public CreateUnitHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<int>> Handle(CreateUnitCommand cmd, CancellationToken ct)
    {
        var unit = Unit.Create(cmd.Code, cmd.Name, cmd.NameAr, cmd.Abbreviation);
        await _uow.Users.AddAsync(null!, ct); // placeholder — impl in persistence
        await _uow.SaveChangesAsync(ct);
        return Result<int>.Success(unit.Id);
    }
}
