using MediatR;
using CorePOS.Application.Common;

namespace CorePOS.Application.Features.Expenses.Commands;

public record CreateExpenseCommand(
    DateOnly ExpenseDate,
    int      CategoryId,
    int      BranchId,
    decimal  Amount,
    string?  Description = null,
    int?     CashBoxId   = null,
    int?     ShiftId     = null,
    int      UserId      = 0
) : IRequest<Result<int>>;
