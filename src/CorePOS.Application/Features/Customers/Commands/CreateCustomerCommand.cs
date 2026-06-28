using MediatR;
using CorePOS.Application.Common;

namespace CorePOS.Application.Features.Customers.Commands;

public record CreateCustomerCommand(
    string  Name,
    string? Phone          = null,
    string? Phone2         = null,
    string? Address        = null,
    string? Email          = null,
    string? InstapayNumber = null,
    string? TaxNumber      = null,
    int?    GroupId        = null,
    int?    PriceListId    = null,
    decimal CreditLimit    = 0,
    int     PaymentPeriodDays = 0,
    decimal OpeningBalance = 0,
    int?    BranchId       = null
) : IRequest<Result<int>>;
