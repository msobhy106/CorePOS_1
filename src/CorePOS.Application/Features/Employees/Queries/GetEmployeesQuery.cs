using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Employees.DTOs;

namespace CorePOS.Application.Features.Employees.Queries;

public record GetEmployeesQuery(bool? IsActive = null)
    : IRequest<Result<IReadOnlyList<EmployeeDto>>>;

public record GetEmployeeByIdQuery(int Id) : IRequest<Result<EmployeeDto>>;
