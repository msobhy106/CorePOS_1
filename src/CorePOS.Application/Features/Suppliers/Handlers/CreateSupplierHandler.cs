using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Suppliers.Commands;
using CorePOS.Application.Interfaces;
using CorePOS.Domain.Entities;
using CorePOS.Domain.Interfaces;

namespace CorePOS.Application.Features.Suppliers.Handlers;

public class CreateSupplierHandler : IRequestHandler<CreateSupplierCommand, Result<int>>
{
    private readonly IUnitOfWork      _uow;
    private readonly ISequenceService _seq;

    public CreateSupplierHandler(IUnitOfWork uow, ISequenceService seq)
    {
        _uow = uow; _seq = seq;
    }

    public async Task<Result<int>> Handle(CreateSupplierCommand cmd, CancellationToken ct)
    {
        var code = await _seq.NextSupplierCodeAsync(ct);
        var supplier = Supplier.Create(code, cmd.Name, cmd.Phone,
            cmd.ContactPerson, cmd.CreditLimit);
        supplier.UpdateContact(cmd.Name, cmd.Phone, cmd.Phone2, cmd.Address,
            cmd.Email, cmd.TaxNumber, cmd.ContactPerson);
        if (cmd.OpeningBalance > 0)
            supplier.AddPayable(cmd.OpeningBalance);

        await _uow.Suppliers.AddAsync(supplier, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<int>.Success(supplier.Id);
    }
}
