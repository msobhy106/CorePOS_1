using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.CashBoxes.Commands;
using CorePOS.Application.Features.CashBoxes.Queries;
using CorePOS.Application.Features.CashBoxes.DTOs;

namespace CorePOS.Application.Features.CashBoxes.Handlers;

// ── GET CASHBOXES ───────────────────────────────────────────────
public class GetCashBoxesHandler : IRequestHandler<GetCashBoxesQuery, Result<IReadOnlyList<CashBoxDto>>>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public GetCashBoxesHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result<IReadOnlyList<CashBoxDto>>> Handle(GetCashBoxesQuery request, CancellationToken ct)
    {
        try
        {
            var cashBoxes = request.BranchId.HasValue
                ? await _uow.CashBoxes.GetByBranchAsync(request.BranchId.Value, ct)
                : await _uow.CashBoxes.GetAllAsync(ct);

            var dtos = new List<CashBoxDto>();
            foreach (var c in cashBoxes)
            {
                var openShift = await _uow.Shifts.GetOpenShiftByBranchAsync(c.BranchId, ct);
                dtos.Add(new CashBoxDto
                {
                    Id             = c.Id,
                    Code           = c.Code,
                    NameAr         = c.NameAr,
                    BranchName     = c.Branch?.NameAr ?? "",
                    IsMain         = c.IsMain,
                    CurrentBalance = c.CurrentBalance,
                    IsActive       = c.IsActive,
                    HasOpenShift   = openShift?.CashBoxId == c.Id,
                    CurrentCashier = openShift?.CashBoxId == c.Id ? openShift.User?.FullName : null
                });
            }
            return Result<IReadOnlyList<CashBoxDto>>.Success(dtos);
        }
        catch (Exception ex) { return Result<IReadOnlyList<CashBoxDto>>.Failure(ex.Message); }
    }
}

// ── DEPOSIT ────────────────────────────────────────────────────
public class DepositToCashBoxHandler : IRequestHandler<DepositToCashBoxCommand, Result>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public DepositToCashBoxHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(DepositToCashBoxCommand request, CancellationToken ct)
    {
        try
        {
            var cb = await _uow.CashBoxes.GetByIdAsync(request.CashBoxId, ct);
            if (cb == null) return Result.Failure("الصندوق غير موجود");
            await _uow.CashBoxes.UpdateBalanceAsync(request.CashBoxId, cb.CurrentBalance + request.Amount, ct);
            await _uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}

// ── WITHDRAW ───────────────────────────────────────────────────
public class WithdrawFromCashBoxHandler : IRequestHandler<WithdrawFromCashBoxCommand, Result>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public WithdrawFromCashBoxHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(WithdrawFromCashBoxCommand request, CancellationToken ct)
    {
        try
        {
            var cb = await _uow.CashBoxes.GetByIdAsync(request.CashBoxId, ct);
            if (cb == null) return Result.Failure("الصندوق غير موجود");
            if (cb.CurrentBalance < request.Amount) return Result.Failure("رصيد الصندوق غير كافٍ");
            await _uow.CashBoxes.UpdateBalanceAsync(request.CashBoxId, cb.CurrentBalance - request.Amount, ct);
            await _uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}

// ── TRANSFER ───────────────────────────────────────────────────
public class TransferBetweenCashBoxesHandler : IRequestHandler<TransferBetweenCashBoxesCommand, Result>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public TransferBetweenCashBoxesHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(TransferBetweenCashBoxesCommand request, CancellationToken ct)
    {
        try
        {
            var from = await _uow.CashBoxes.GetByIdAsync(request.FromCashBoxId, ct);
            var to   = await _uow.CashBoxes.GetByIdAsync(request.ToCashBoxId, ct);
            if (from == null || to == null) return Result.Failure("صندوق غير موجود");
            if (from.CurrentBalance < request.Amount) return Result.Failure("رصيد الصندوق غير كافٍ");

            await _uow.BeginTransactionAsync(ct);
            await _uow.CashBoxes.UpdateBalanceAsync(request.FromCashBoxId, from.CurrentBalance - request.Amount, ct);
            await _uow.CashBoxes.UpdateBalanceAsync(request.ToCashBoxId, to.CurrentBalance + request.Amount, ct);
            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            await _uow.RollbackTransactionAsync(ct);
            return Result.Failure(ex.Message);
        }
    }
}
