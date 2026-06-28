using MediatR;
using CorePOS.Application.Common;
using CorePOS.Domain.Enums;

namespace CorePOS.Application.Features.Purchases.Commands;

public record PurchaseReturnItemRequest(
    int     InvoiceItemId,
    int     ProductId,
    int     UnitId,
    string  ProductNameAr,
    decimal Quantity,
    decimal UnitCost
);

public record CreatePurchaseReturnCommand(
    int                          OriginalInvoiceId,
    ReturnType                   ReturnType,
    List<PurchaseReturnItemRequest> Items,
    int                          BranchId,
    int                          WarehouseId,
    int                          UserId,
    string?                      Notes = null
) : IRequest<Result<int>>;
