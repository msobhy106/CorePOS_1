using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Customers.Commands;
using CorePOS.Application.Interfaces;
using CorePOS.Domain.Interfaces;

namespace CorePOS.Application.Features.Customers.Handlers;

public class RecordCustomerPaymentHandler : IRequestHandler<RecordCustomerPaymentCommand, Result<int>>
{
    private readonly IUnitOfWork      _uow;
    private readonly ISequenceService _seq;

    public RecordCustomerPaymentHandler(IUnitOfWork uow, ISequenceService seq)
    {
        _uow = uow;
        _seq = seq;
    }

    public async Task<Result<int>> Handle(RecordCustomerPaymentCommand cmd, CancellationToken ct)
    {
        var customer = await _uow.Customers.GetByIdAsync(cmd.CustomerId, ct);
        if (customer is null) return Result<int>.NotFound("العميل غير موجود");

        if (cmd.Amount > customer.CurrentBalance)
            return Result<int>.Failure("المبلغ المدفوع أكبر من رصيد العميل");

        var paymentNo = await _seq.NextCustomerPaymentNoAsync(ct);

        var payment = CustomerPayment.Create(paymentNo, cmd.CustomerId,
            cmd.Amount, cmd.PaymentMethod, cmd.CashBoxId,
            cmd.ShiftId, cmd.Notes);

        await _uow.SaveChangesAsync(ct);
        customer.ReduceDebt(cmd.Amount);
        _uow.Customers.Update(customer);
        await _uow.SaveChangesAsync(ct);

        return Result<int>.Success(payment.Id);
    }
}
