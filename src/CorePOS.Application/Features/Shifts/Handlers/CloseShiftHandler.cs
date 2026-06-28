using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Shifts.Commands;
using CorePOS.Domain.Interfaces;

namespace CorePOS.Application.Features.Shifts.Handlers;

public class CloseShiftHandler : IRequestHandler<CloseShiftCommand, Result>
{
    private readonly IUnitOfWork _uow;
    public CloseShiftHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(CloseShiftCommand cmd, CancellationToken ct)
    {
        var shift = await _uow.SaveChangesAsync(ct); // placeholder — implemented in Persistence
        return Result.Success();
    }
}
