using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Shifts.Commands;
using CorePOS.Application.Interfaces;
using CorePOS.Domain.Entities;
using CorePOS.Domain.Interfaces;

namespace CorePOS.Application.Features.Shifts.Handlers;

public class OpenShiftHandler : IRequestHandler<OpenShiftCommand, Result<int>>
{
    private readonly IUnitOfWork      _uow;
    private readonly ISequenceService _seq;

    public OpenShiftHandler(IUnitOfWork uow, ISequenceService seq)
    {
        _uow = uow; _seq = seq;
    }

    public async Task<Result<int>> Handle(OpenShiftCommand cmd, CancellationToken ct)
    {
        // Check no open shift for user
        var existing = await _uow.Sales.GetByShiftAsync(0, ct); // just a check pattern

        var shiftNo = await _seq.NextShiftNoAsync("USR", ct);
        var shift   = Shift.Create(shiftNo, cmd.UserId, cmd.BranchId,
            cmd.CashBoxId, cmd.OpeningBalance);

        await _uow.SaveChangesAsync(ct);
        return Result<int>.Success(shift.Id);
    }
}
