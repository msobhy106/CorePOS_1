using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Customers.Commands;
using CorePOS.Application.Interfaces;
using CorePOS.Domain.Entities;
using CorePOS.Domain.Interfaces;

namespace CorePOS.Application.Features.Customers.Handlers;

public class CreateCustomerHandler : IRequestHandler<CreateCustomerCommand, Result<int>>
{
    private readonly IUnitOfWork         _uow;
    private readonly ISequenceService    _seq;

    public CreateCustomerHandler(IUnitOfWork uow, ISequenceService seq)
    {
        _uow = uow;
        _seq = seq;
    }

    public async Task<Result<int>> Handle(CreateCustomerCommand cmd, CancellationToken ct)
    {
        var code = await _seq.NextCustomerCodeAsync(ct);

        var customer = Customer.Create(code, cmd.Name, cmd.Phone,
            cmd.Address, cmd.GroupId, cmd.PriceListId,
            cmd.CreditLimit, cmd.PaymentPeriodDays);

        customer.UpdateContact(cmd.Name, cmd.Phone, cmd.Phone2, cmd.Address,
            cmd.Email, cmd.InstapayNumber, cmd.TaxNumber);

        if (cmd.OpeningBalance != 0)
            customer.AddDebt(cmd.OpeningBalance);

        await _uow.Customers.AddAsync(customer, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<int>.Success(customer.Id);
    }
}
