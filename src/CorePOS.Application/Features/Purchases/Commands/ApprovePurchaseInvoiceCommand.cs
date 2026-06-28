using MediatR;
using CorePOS.Application.Common;

namespace CorePOS.Application.Features.Purchases.Commands;

public record ApprovePurchaseInvoiceCommand(int InvoiceId, int ApprovedBy)
    : IRequest<Result>;
