using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Inventory.Commands;
using CorePOS.Domain.Interfaces;

namespace CorePOS.Application.Features.Inventory.Handlers;

public class CancelInventorySessionHandler : IRequestHandler<CancelInventorySessionCommand, Result>
{
    private readonly IUnitOfWork _uow;
    public CancelInventorySessionHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(CancelInventorySessionCommand cmd, CancellationToken ct)
    {
        var session = await _uow.Inventory.GetSessionByIdAsync(cmd.SessionId, ct);
        if (session is null) return Result.NotFound("جلسة الجرد غير موجودة");

        try { session.Cancel(); }
        catch (InvalidOperationException ex) { return Result.Failure(ex.Message); }

        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
