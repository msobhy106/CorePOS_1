using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Expenses.DTOs;

namespace CorePOS.Application.Features.Expenses.Queries;

public record GetExpensesQuery(
    DateOnly? From      = null,
    DateOnly? To        = null,
    int?      BranchId  = null,
    int?      CategoryId= null,
    int       PageNumber= 1,
    int       PageSize  = 30
) : IRequest<Result<PagedResult<ExpenseDto>>>;
