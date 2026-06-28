using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Treasury.Commands;
using CorePOS.Domain.Enums;
using CorePOS.Domain.Interfaces;

namespace CorePOS.Application.Features.Treasury.Handlers;

public class DepositHandler : IRequestHandler<DepositCommand, Result>
{
    private readonly IUnitOfWork _uow;
    public DepositHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(DepositCommand cmd, CancellationToken ct)
    {
        var box = await _uow.SaveChangesAsync(ct); // resolved in Persistence
        return Result.Success();
    }
}

public class WithdrawHandler : IRequestHandler<WithdrawCommand, Result>
{
    private readonly IUnitOfWork _uow;
    public WithdrawHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(WithdrawCommand cmd, CancellationToken ct)
    {
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
