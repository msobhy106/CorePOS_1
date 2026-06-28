using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Purchases.Queries;
using CorePOS.Application.Features.Purchases.DTOs;

namespace CorePOS.Application.Features.Purchases.Handlers;

// ── GET PURCHASE INVOICES (paged) ───────────────────────────────
public class GetPurchaseInvoicesHandler
    : IRequestHandler<GetPurchaseInvoicesQuery, Result<PagedResult<PurchaseInvoiceListDto>>>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public GetPurchaseInvoicesHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result<PagedResult<PurchaseInvoiceListDto>>> Handle(
        GetPurchaseInvoicesQuery request, CancellationToken ct)
    {
        try
        {
            var from = request.From ?? DateTime.MinValue;
            var to   = request.To   ?? DateTime.MaxValue;

            var invoices = request.SupplierId.HasValue
                ? await _uow.Purchases.GetBySupplierAsync(request.SupplierId.Value, from, to, ct)
                : await _uow.Purchases.GetByDateRangeAsync(from, to, request.BranchId, ct);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var s = request.Search.ToLower();
                invoices = invoices.Where(i =>
                    i.InvoiceNo.ToLower().Contains(s) ||
                    (i.Supplier?.Name?.ToLower().Contains(s) ?? false)).ToList();
            }

            var total = invoices.Count;
            var items = invoices
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(i => new PurchaseInvoiceListDto
                {
                    Id              = i.Id,
                    InvoiceNo       = i.InvoiceNo,
                    InvoiceDate     = i.InvoiceDate,
                    SupplierName    = i.Supplier?.Name,
                    Status          = i.Status,
                    TotalAmount     = i.TotalAmount,
                    RemainingAmount = i.RemainingAmount,
                    ItemsCount      = i.Items.Count
                }).ToList();

            return Result<PagedResult<PurchaseInvoiceListDto>>.Success(
                new PagedResult<PurchaseInvoiceListDto>(items, total, request.PageNumber, request.PageSize));
        }
        catch (Exception ex) { return Result<PagedResult<PurchaseInvoiceListDto>>.Failure(ex.Message); }
    }
}

// ── GET PURCHASE INVOICE BY ID ──────────────────────────────────
public class GetPurchaseInvoiceByIdHandler
    : IRequestHandler<GetPurchaseInvoiceByIdQuery, Result<PurchaseInvoiceDto>>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public GetPurchaseInvoiceByIdHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result<PurchaseInvoiceDto>> Handle(
        GetPurchaseInvoiceByIdQuery request, CancellationToken ct)
    {
        try
        {
            var inv = await _uow.Purchases.GetByIdWithItemsAsync(request.Id, ct);
            if (inv == null) return Result<PurchaseInvoiceDto>.Failure("الفاتورة غير موجودة");

            var dto = new PurchaseInvoiceDto
            {
                Id                = inv.Id,
                InvoiceNo         = inv.InvoiceNo,
                SupplierInvoiceNo = inv.SupplierInvoiceNo,
                InvoiceDate       = inv.InvoiceDate,
                SupplierId        = inv.SupplierId,
                SupplierName      = inv.Supplier?.Name,
                BranchName        = inv.Branch?.NameAr ?? "",
                WarehouseName     = inv.Warehouse?.NameAr ?? "",
                Status            = inv.Status,
                PaymentMethod     = inv.PaymentMethod,
                Subtotal          = inv.Subtotal,
                DiscountAmount    = inv.DiscountAmount,
                TaxAmount         = inv.TaxAmount,
                TotalAmount       = inv.TotalAmount,
                PaidAmount        = inv.PaidAmount,
                RemainingAmount   = inv.RemainingAmount,
                Notes             = inv.Notes,
                Items             = inv.Items.Select(item => new PurchaseItemDto
                {
                    Id              = item.Id,
                    ProductId       = item.ProductId,
                    ProductNameAr   = item.ProductNameAr,
                    Quantity        = item.Quantity,
                    UnitCost        = item.UnitCost,
                    DiscountPercent = item.DiscountPercent,
                    TaxAmount       = item.TaxAmount,
                    TotalCost       = item.TotalCost,
                    SalePriceAfter  = item.SalePriceAfter
                }).ToList()
            };

            return Result<PurchaseInvoiceDto>.Success(dto);
        }
        catch (Exception ex) { return Result<PurchaseInvoiceDto>.Failure(ex.Message); }
    }
}
