using MediatR;
using CorePOS.Application.Common;

namespace CorePOS.Application.Features.Customers.Commands;

public record UpdateCustomerCommand(
    int     Id,
    string  Name,
    string? Phone,
    string? Phone2,
    string? Address,
    string? Email,
    string? InstapayNumber,
    string? TaxNumber,
    int?    GroupId,
    int?    PriceListId,
    decimal CreditLimit,
    int     PaymentPeriodDays
) : IRequest<Result>;
