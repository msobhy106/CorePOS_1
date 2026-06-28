using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Customers.DTOs;

namespace CorePOS.Application.Features.Customers.Queries;

public record GetCustomersQuery(
    string? Search    = null,
    int?    GroupId   = null,
    bool?   IsActive  = null,
    bool    DebtOnly  = false,
    int     PageNumber= 1,
    int     PageSize  = 50
) : IRequest<Result<PagedResult<CustomerListDto>>>;
