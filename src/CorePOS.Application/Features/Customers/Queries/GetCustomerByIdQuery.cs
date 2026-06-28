using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Customers.DTOs;

namespace CorePOS.Application.Features.Customers.Queries;

public record GetCustomerByIdQuery(int Id) : IRequest<Result<CustomerDto>>;
