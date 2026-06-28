using MediatR;
using CorePOS.Application.Common;
using CorePOS.Domain.Enums;

namespace CorePOS.Application.Features.Customers.Commands;

public record RecordCustomerPaymentCommand(
    int           CustomerId,
    decimal       Amount,
    PaymentMethod PaymentMethod = PaymentMethod.Cash,
    int?          CashBoxId     = null,
    int?          ShiftId       = null,
    string?       Notes         = null
) : IRequest<Result<int>>;
