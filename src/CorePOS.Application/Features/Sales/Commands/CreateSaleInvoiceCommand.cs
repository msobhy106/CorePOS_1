using MediatR;
using CorePOS.Application.Common;
using CorePOS.Domain.Enums;

namespace CorePOS.Application.Features.Sales.Commands;

public record SaleItemRequest(
    int     ProductId,
    int     UnitId,
    string  ProductNameAr,
    string? Barcode,
    decimal Quantity,
    decimal UnitPrice,
    decimal PurchasePrice,
    decimal DiscountPercent = 0,
    decimal TaxPercent      = 0
);

public record CreateSaleInvoiceCommand(
    int                  BranchId,
    int                  WarehouseId,
    int                  UserId,
    int?                 ShiftId,
    int?                 CustomerId,
    InvoiceType          InvoiceType,
    PaymentMethod        PaymentMethod,
    List<SaleItemRequest> Items,
    decimal              DiscountPercent    = 0,
    decimal              DiscountAmount     = 0,
    decimal              TaxPercent         = 0,
    decimal              DeliveryCost       = 0,
    int?                 DeliveryAgentId    = null,
    decimal              PaidAmount         = 0,
    decimal              VisaAmount         = 0,
    decimal              BankTransferAmount = 0,
    decimal              EWalletAmount      = 0,
    string?              Notes              = null
) : IRequest<Result<int>>;
