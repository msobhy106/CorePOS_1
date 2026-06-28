using MediatR;
using CorePOS.Application.Common;
using CorePOS.Domain.Enums;

namespace CorePOS.Application.Features.Suppliers.Commands;

public record RecordSupplierPaymentCommand(
    int           SupplierId,
    decimal       Amount,
    PaymentMethod PaymentMethod = PaymentMethod.Cash,
    int?          CashBoxId     = null,
    int?          ShiftId       = null,
    string?       Notes         = null,
    int           UserId        = 0
) : IRequest<Result<int>>;
