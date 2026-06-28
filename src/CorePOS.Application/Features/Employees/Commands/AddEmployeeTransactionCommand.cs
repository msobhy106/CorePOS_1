using MediatR;
using CorePOS.Application.Common;
using CorePOS.Domain.Enums;

namespace CorePOS.Application.Features.Employees.Commands;

public record AddEmployeeTransactionCommand(
    int                    EmployeeId,
    EmployeeTransactionType Type,
    decimal                Amount,
    DateOnly               Date,
    string?                Notes     = null,
    int                    CreatedBy = 0
) : IRequest<Result>;
