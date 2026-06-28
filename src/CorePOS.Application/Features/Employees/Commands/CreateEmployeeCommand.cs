using MediatR;
using CorePOS.Application.Common;

namespace CorePOS.Application.Features.Employees.Commands;

public record CreateEmployeeCommand(
    string   Name,
    string?  JobTitle = null,
    string?  Phone    = null,
    string?  Address  = null,
    decimal  Salary   = 0,
    DateOnly? HireDate= null,
    int?     BranchId = null
) : IRequest<Result<int>>;
