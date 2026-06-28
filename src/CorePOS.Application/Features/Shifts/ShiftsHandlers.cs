using CorePOS.Application.Features.Shifts.DTOs;
using CorePOS.Application.Features.Shifts.Queries;
using MediatR;
using CorePOS.Application.Common;

namespace CorePOS.Application.Features.Shifts.Queries;

// ════════════════════════════════════════════════════════════════════
// GET RECENT SHIFTS
// ════════════════════════════════════════════════════════════════════
public record GetRecentShiftsQuery(int BranchId, int DaysBack = 30)
    : IRequest<Result<List<ShiftSummaryDto>>>;

public record ShiftSummaryDto(
    int       ShiftId,
    string    ShiftNo,
    string    CashierName,
    DateTime  OpenedAt,
    DateTime? ClosedAt,
    decimal   OpeningBalance,
    decimal   ClosingBalance,
    int       InvoiceCount,
    decimal   TotalSales,
    string    StatusAr);

// ── GET CURRENT SHIFT (BUG-014 / missing handler) ────────────────
public record GetCurrentShiftQuery(int UserId) : IRequest<Result<ShiftSummaryDto?>>;

public class GetCurrentShiftHandler : IRequestHandler<GetCurrentShiftQuery, Result<ShiftSummaryDto?>>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public GetCurrentShiftHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result<ShiftSummaryDto?>> Handle(GetCurrentShiftQuery request, CancellationToken ct)
    {
        try
        {
            var shift = await _uow.Shifts.GetOpenShiftAsync(request.UserId, ct);
            if (shift == null) return Result<ShiftSummaryDto?>.Success(null);

            var dto = new ShiftSummaryDto(
                shift.Id, shift.ShiftNo,
                shift.User?.FullName ?? "",
                shift.OpenedAt, shift.ClosedAt,
                shift.OpeningBalance, shift.ClosingBalance,
                shift.InvoiceCount, shift.TotalSales,
                shift.IsOpen ? "مفتوحة" : "مغلقة");

            return Result<ShiftSummaryDto?>.Success(dto);
        }
        catch (Exception ex) { return Result<ShiftSummaryDto?>.Failure(ex.Message); }
    }
}

public class GetRecentShiftsHandler : IRequestHandler<GetRecentShiftsQuery, Result<List<ShiftSummaryDto>>>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public GetRecentShiftsHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result<List<ShiftSummaryDto>>> Handle(
        GetRecentShiftsQuery request, CancellationToken ct)
    {
        try
        {
            var from   = DateTime.Today.AddDays(-request.DaysBack);
            var shifts = await _uow.Shifts.GetRecentAsync(request.BranchId, from, ct);

            var result = shifts.Select(s => new ShiftSummaryDto(
                ShiftId:        s.Id,
                ShiftNo:        s.ShiftNo,
                CashierName:    s.User?.FullName ?? "",
                OpenedAt:       s.OpenedAt,
                ClosedAt:       s.ClosedAt,
                OpeningBalance: s.OpeningBalance,
                ClosingBalance: s.ClosingBalance,
                InvoiceCount:   s.InvoiceCount,
                TotalSales:     s.TotalSales,
                StatusAr:       s.IsClosed ? "مغلقة" : "مفتوحة"
            )).ToList();

            return Result<List<ShiftSummaryDto>>.Success(result);
        }
        catch (Exception ex) { return Result<List<ShiftSummaryDto>>.Failure(ex.Message); }
    }
}

namespace CorePOS.Application.Features.Shifts.Commands;

// ════════════════════════════════════════════════════════════════════
// OPEN SHIFT COMMAND
// ════════════════════════════════════════════════════════════════════
public record OpenShiftCommand(int UserId, int BranchId, decimal OpeningBalance)
    : IRequest<Result<OpenShiftResultDto>>;

public record OpenShiftResultDto(int ShiftId, string ShiftNo);

public class OpenShiftHandler : IRequestHandler<OpenShiftCommand, Result<OpenShiftResultDto>>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public OpenShiftHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result<OpenShiftResultDto>> Handle(
        OpenShiftCommand request, CancellationToken ct)
    {
        try
        {
            var existing = await _uow.Shifts.GetOpenShiftAsync(request.UserId, ct);
            if (existing != null)
                return Result<OpenShiftResultDto>.Failure("توجد وردية مفتوحة بالفعل");

            var shiftNo = await _uow.Shifts.GenerateShiftNoAsync(request.BranchId, ct);

            // Use Shift.Create() factory method (BUG-013 fix)
            var shift = Domain.Entities.Shift.Create(
                shiftNo:        shiftNo,
                userId:         request.UserId,
                branchId:       request.BranchId,
                cashBoxId:      0,   // default, caller should pass CashBoxId
                openingBalance: request.OpeningBalance);

            await _uow.Shifts.AddAsync(shift, ct);
            await _uow.SaveChangesAsync(ct);

            return Result<OpenShiftResultDto>.Success(new OpenShiftResultDto(shift.Id, shift.ShiftNo));
        }
        catch (Exception ex) { return Result<OpenShiftResultDto>.Failure(ex.Message); }
    }
}

// ════════════════════════════════════════════════════════════════════
// CLOSE SHIFT COMMAND
// ════════════════════════════════════════════════════════════════════
public record CloseShiftCommand(int ShiftId, decimal ClosingBalance, string? Notes)
    : IRequest<Result>;

public class CloseShiftHandler : IRequestHandler<CloseShiftCommand, Result>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public CloseShiftHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(CloseShiftCommand request, CancellationToken ct)
    {
        try
        {
            var shift = await _uow.Shifts.GetByIdAsync(request.ShiftId, ct);
            if (shift == null)
                return Result.Failure("الوردية غير موجودة");

            if (shift.IsClosed)
                return Result.Failure("الوردية مغلقة بالفعل");

            // Calculate totals from invoices
            shift.TotalSales   = await _uow.Sales.GetTotalByShiftAsync(request.ShiftId, ct);
            shift.TotalReturns = await _uow.Sales.GetReturnsTotalByShiftAsync(request.ShiftId, ct);
            shift.InvoiceCount = await _uow.Sales.GetCountByShiftAsync(request.ShiftId, ct);

            shift.SetClosingBalance(request.ClosingBalance);

            // Use domain method to close (BUG-013 fix)
            shift.Close(request.ClosingBalance, request.Notes);

            await _uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}

// ── GET SHIFT BY ID ──────────────────────────────────────────────
namespace CorePOS.Application.Features.Shifts.Handlers;

public class GetShiftByIdHandler : IRequestHandler<GetShiftByIdQuery, Result<ShiftDto>>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public GetShiftByIdHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result<ShiftDto>> Handle(GetShiftByIdQuery request, CancellationToken ct)
    {
        try
        {
            var shift = await _uow.Shifts.GetByIdAsync(request.ShiftId, ct);
            if (shift == null) return Result<ShiftDto>.Failure("الوردية غير موجودة");

            var expenses = await _uow.Expenses.GetByShiftAsync(request.ShiftId, ct);

            return Result<ShiftDto>.Success(new ShiftDto
            {
                Id             = shift.Id,
                ShiftNo        = shift.ShiftNo,
                CashierName    = shift.User?.FullName ?? "",
                BranchName     = shift.Branch?.NameAr ?? "",
                CashBoxName    = shift.CashBox?.NameAr ?? "",
                OpeningBalance = shift.OpeningBalance,
                ClosingBalance = shift.ClosingBalance,
                ActualBalance  = shift.ActualBalance,
                StartTime      = shift.StartTime,
                EndTime        = shift.EndTime,
                Status         = shift.IsOpen ? "مفتوحة" : "مغلقة",
                SalesCount     = shift.InvoiceCount,
                SalesRevenue   = shift.TotalSales,
                TotalExpenses  = expenses.Sum(e => e.Amount),
                Notes          = shift.Notes
            });
        }
        catch (Exception ex) { return Result<ShiftDto>.Failure(ex.Message); }
    }
}
