using MediatR;
using CorePOS.Application.Common;
using CorePOS.Domain.Enums;

namespace CorePOS.Application.Features.Sales.Commands;

public record ReturnItemRequest(
    int     InvoiceItemId,
    int     ProductId,
    int     UnitId,
    string  ProductNameAr,
    decimal Quantity,
    decimal UnitPrice
);

public record CreateSaleReturnCommand(
    int                   OriginalInvoiceId,
    ReturnType            ReturnType,
    RefundMethod          RefundMethod,
    List<ReturnItemRequest> Items,
    int                   BranchId,
    int                   WarehouseId,
    int                   UserId,
    int?                  ShiftId   = null,
    string?               Notes     = null
) : IRequest<Result<int>>;
