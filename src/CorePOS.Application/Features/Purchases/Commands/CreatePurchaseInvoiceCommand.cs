using MediatR;
using CorePOS.Application.Common;
using CorePOS.Domain.Enums;

namespace CorePOS.Application.Features.Purchases.Commands;

public record PurchaseItemRequest(
    int      ProductId,
    int      UnitId,
    string   ProductNameAr,
    decimal  Quantity,
    decimal  UnitCost,
    decimal  DiscountPercent = 0,
    decimal  TaxPercent      = 0,
    decimal? SalePriceAfter  = null
);

public record CreatePurchaseInvoiceCommand(
    int                      BranchId,
    int                      WarehouseId,
    int                      UserId,
    int?                     SupplierId,
    string?                  SupplierInvoiceNo,
    PaymentMethod            PaymentMethod,
    List<PurchaseItemRequest> Items,
    decimal                  DiscountPercent = 0,
    decimal                  TaxPercent      = 0,
    decimal                  PaidAmount      = 0,
    string?                  Notes           = null,
    bool                     AutoApprove     = false
) : IRequest<Result<int>>;
