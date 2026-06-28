using MediatR;
using CorePOS.Application.Common;

namespace CorePOS.Application.Features.Suppliers.Commands;

public record CreateSupplierCommand(
    string  Name,
    string? Phone          = null,
    string? Phone2         = null,
    string? Address        = null,
    string? Email          = null,
    string? TaxNumber      = null,
    string? ContactPerson  = null,
    decimal CreditLimit    = 0,
    int     PaymentPeriodDays = 0,
    decimal OpeningBalance = 0
) : IRequest<Result<int>>;
