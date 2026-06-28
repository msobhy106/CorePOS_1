using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Treasury.DTOs;

namespace CorePOS.Application.Features.Treasury.Queries;

public record GetCashBoxTransactionsQuery(
    int       CashBoxId,
    DateTime? From      = null,
    DateTime? To        = null,
    int       PageNumber= 1,
    int       PageSize  = 30
) : IRequest<Result<PagedResult<TreasuryTransactionDto>>>;
