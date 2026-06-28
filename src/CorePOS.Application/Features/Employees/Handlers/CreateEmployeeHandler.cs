using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Employees.Commands;
using CorePOS.Application.Interfaces;
using CorePOS.Domain.Entities;
using CorePOS.Domain.Interfaces;

namespace CorePOS.Application.Features.Employees.Handlers;

public class CreateEmployeeHandler : IRequestHandler<CreateEmployeeCommand, Result<int>>
{
    private readonly IUnitOfWork      _uow;
    private readonly ISequenceService _seq;

    public CreateEmployeeHandler(IUnitOfWork uow, ISequenceService seq)
    {
        _uow = uow; _seq = seq;
    }

    public async Task<Result<int>> Handle(CreateEmployeeCommand cmd, CancellationToken ct)
    {
        var code = await _seq.NextProductCodeAsync(ct); // reuses general sequence
        var employee = Employee.Create(code, cmd.Name, cmd.JobTitle,
            cmd.Phone, cmd.Salary, cmd.BranchId);
        employee.Update(cmd.Name, cmd.JobTitle, cmd.Phone, cmd.Address, cmd.Salary);
        await _uow.SaveChangesAsync(ct);
        return Result<int>.Success(employee.Id);
    }
}
