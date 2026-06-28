using MediatR;
using Microsoft.EntityFrameworkCore;
using CorePOS.Application.Common;
using CorePOS.Application.Interfaces;
using CorePOS.Application.Features.Employees.Commands;
using CorePOS.Application.Features.Employees.Queries;
using CorePOS.Application.Features.Employees.DTOs;

namespace CorePOS.Application.Features.Employees.Handlers;

// ── GET EMPLOYEES ────────────────────────────────────────────────
public class GetEmployeesHandler : IRequestHandler<GetEmployeesQuery, Result<IReadOnlyList<EmployeeDto>>>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public GetEmployeesHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result<IReadOnlyList<EmployeeDto>>> Handle(GetEmployeesQuery request, CancellationToken ct)
    {
        try
        {
            var employees = request.IsActive.HasValue
                ? await _uow.Employees.GetAsync(e => e.IsActive == request.IsActive.Value, ct)
                : await _uow.Employees.GetAllAsync(ct);

            var dtos = employees.Select(e => new EmployeeDto
            {
                Id         = e.Id,
                Code       = e.Code,
                Name       = e.Name,
                JobTitle   = e.JobTitle,
                Phone      = e.Phone,
                Address    = e.Address,
                Salary     = e.Salary,
                HireDate   = e.HireDate,
                BranchName = e.Branch?.NameAr ?? "",
                IsActive   = e.IsActive
            }).ToList();

            return Result<IReadOnlyList<EmployeeDto>>.Success(dtos);
        }
        catch (Exception ex) { return Result<IReadOnlyList<EmployeeDto>>.Failure(ex.Message); }
    }
}

// ── ADD EMPLOYEE TRANSACTION ─────────────────────────────────────
public class AddEmployeeTransactionHandler : IRequestHandler<AddEmployeeTransactionCommand, Result>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    private readonly IApplicationDbContext          _db;

    public AddEmployeeTransactionHandler(Domain.Interfaces.IUnitOfWork uow, IApplicationDbContext db)
    {
        _uow = uow;
        _db  = db;
    }

    public async Task<Result> Handle(AddEmployeeTransactionCommand request, CancellationToken ct)
    {
        try
        {
            var emp = await _uow.Employees.GetByIdAsync(request.EmployeeId, ct);
            if (emp == null) return Result.Failure("الموظف غير موجود");

            var tx = Domain.Entities.EmployeeTransaction.Create(
                request.EmployeeId, request.Type, request.Amount,
                request.Date, request.Notes,
                request.CreatedBy == 0 ? null : (int?)request.CreatedBy);

            _db.EmployeeTransactions.Add(tx);
            await _db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}

// ── GET EMPLOYEE BY ID ───────────────────────────────────────────
public class GetEmployeeByIdHandler : IRequestHandler<GetEmployeeByIdQuery, Result<EmployeeDto>>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public GetEmployeeByIdHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result<EmployeeDto>> Handle(GetEmployeeByIdQuery request, CancellationToken ct)
    {
        try
        {
            var e = await _uow.Employees.GetByIdAsync(request.Id, ct);
            if (e == null) return Result<EmployeeDto>.Failure("الموظف غير موجود");

            return Result<EmployeeDto>.Success(new EmployeeDto
            {
                Id         = e.Id,
                Code       = e.Code,
                Name       = e.Name,
                JobTitle   = e.JobTitle,
                Phone      = e.Phone,
                Address    = e.Address,
                Salary     = e.Salary,
                HireDate   = e.HireDate,
                BranchName = e.Branch?.NameAr ?? "",
                IsActive   = e.IsActive
            });
        }
        catch (Exception ex) { return Result<EmployeeDto>.Failure(ex.Message); }
    }
}
