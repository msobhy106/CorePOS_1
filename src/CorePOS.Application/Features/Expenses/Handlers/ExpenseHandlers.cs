using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Expenses.Commands;
using CorePOS.Application.Features.Expenses.Queries;
using CorePOS.Application.Features.Expenses.DTOs;

namespace CorePOS.Application.Features.Expenses.Handlers;

public class GetExpensesHandler : IRequestHandler<GetExpensesQuery, Result<PagedResult<ExpenseDto>>>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public GetExpensesHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result<PagedResult<ExpenseDto>>> Handle(GetExpensesQuery request, CancellationToken ct)
    {
        try
        {
            var from = request.From.HasValue ? request.From.Value.ToDateTime(TimeOnly.MinValue) : DateTime.MinValue;
            var to   = request.To.HasValue   ? request.To.Value.ToDateTime(TimeOnly.MaxValue)   : DateTime.MaxValue;
            var all  = await _uow.Expenses.GetByDateRangeAsync(from, to, request.BranchId, ct);

            var filtered = all
                .Where(e => !request.CategoryId.HasValue || e.CategoryId == request.CategoryId.Value)
                .ToList();

            var items = filtered
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(e => new ExpenseDto
                {
                    Id           = e.Id,
                    ExpenseNo    = e.ExpenseNo,
                    ExpenseDate  = e.ExpenseDate,
                    CategoryName = e.Category?.NameAr ?? "",
                    BranchName   = e.Branch?.NameAr ?? "",
                    Amount       = e.Amount,
                    Description  = e.Description,
                    CreatedBy    = ""
                }).ToList();

            return Result<PagedResult<ExpenseDto>>.Success(
                new PagedResult<ExpenseDto>(items, filtered.Count, request.PageNumber, request.PageSize));
        }
        catch (Exception ex) { return Result<PagedResult<ExpenseDto>>.Failure(ex.Message); }
    }
}

public class CreateExpenseHandler : IRequestHandler<CreateExpenseCommand, Result<int>>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public CreateExpenseHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result<int>> Handle(CreateExpenseCommand request, CancellationToken ct)
    {
        try
        {
            var no      = await _uow.Expenses.GenerateExpenseNoAsync(ct);
            var expense = Domain.Entities.Expense.Create(no, request.ExpenseDate,
                request.CategoryId, request.BranchId, request.Amount, request.UserId,
                request.CashBoxId, request.ShiftId, request.Description);
            await _uow.Expenses.AddAsync(expense, ct);
            await _uow.SaveChangesAsync(ct);
            return Result<int>.Success(expense.Id);
        }
        catch (Exception ex) { return Result<int>.Failure(ex.Message); }
    }
}
