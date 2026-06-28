using MediatR;
using CorePOS.Application.Common;

namespace CorePOS.Application.Features.Sales.Commands;

public record HoldInvoiceCommand(int InvoiceId)  : IRequest<Result>;
public record RetrieveInvoiceCommand(int InvoiceId) : IRequest<Result>;
public record CancelInvoiceCommand(int InvoiceId)   : IRequest<Result>;
