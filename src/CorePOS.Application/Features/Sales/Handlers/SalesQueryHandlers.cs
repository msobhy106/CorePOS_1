using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Sales.Queries;
using CorePOS.Application.Features.Sales.Commands;
using CorePOS.Application.Features.Sales.DTOs;

namespace CorePOS.Application.Features.Sales.Handlers;

// ── GET SALE INVOICES (paged) ───────────────────────────────────
public class GetSaleInvoicesHandler
    : IRequestHandler<GetSaleInvoicesQuery, Result<PagedResult<SalesInvoiceListDto>>>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public GetSaleInvoicesHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result<PagedResult<SalesInvoiceListDto>>> Handle(
        GetSaleInvoicesQuery request, CancellationToken ct)
    {
        try
        {
            var from = request.From ?? DateTime.MinValue;
            var to   = request.To   ?? DateTime.MaxValue;

            IReadOnlyList<Domain.Entities.SalesInvoice> invoices;
            if (request.CustomerId.HasValue)
                invoices = await _uow.Sales.GetByCustomerAsync(request.CustomerId.Value, from, to, ct);
            else if (request.ShiftId.HasValue)
                invoices = await _uow.Sales.GetByShiftAsync(request.ShiftId.Value, ct);
            else
                invoices = await _uow.Sales.GetByDateRangeAsync(from, to, request.BranchId, ct);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var s = request.Search.ToLower();
                invoices = invoices.Where(i =>
                    i.InvoiceNo.ToLower().Contains(s) ||
                    (i.Customer?.Name?.ToLower().Contains(s) ?? false)).ToList();
            }

            var total = invoices.Count;
            var items = invoices
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(ToListDto).ToList();

            return Result<PagedResult<SalesInvoiceListDto>>.Success(
                new PagedResult<SalesInvoiceListDto>(items, total, request.PageNumber, request.PageSize));
        }
        catch (Exception ex) { return Result<PagedResult<SalesInvoiceListDto>>.Failure(ex.Message); }
    }

    private static SalesInvoiceListDto ToListDto(Domain.Entities.SalesInvoice i) => new()
    {
        Id           = i.Id,
        InvoiceNo    = i.InvoiceNo,
        InvoiceDate  = i.InvoiceDate,
        CustomerName = i.Customer?.Name,
        CashierName  = i.User?.FullName ?? "",
        PaymentMethod= i.PaymentMethod,
        Status       = i.Status,
        TotalAmount     = i.TotalAmount,
                PaidAmount      = i.PaidAmount,
                RemainingAmount = i.RemainingAmount,
                ItemsCount = i.Items.Count
    };
}

// ── GET SALE INVOICE BY ID ──────────────────────────────────────
public class GetSaleInvoiceByIdHandler
    : IRequestHandler<GetSaleInvoiceByIdQuery, Result<SalesInvoiceDto>>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public GetSaleInvoiceByIdHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result<SalesInvoiceDto>> Handle(GetSaleInvoiceByIdQuery request, CancellationToken ct)
    {
        try
        {
            var inv = await _uow.Sales.GetByIdWithItemsAsync(request.Id, ct);
            if (inv == null) return Result<SalesInvoiceDto>.Failure("الفاتورة غير موجودة");
            return Result<SalesInvoiceDto>.Success(MapToDto(inv));
        }
        catch (Exception ex) { return Result<SalesInvoiceDto>.Failure(ex.Message); }
    }

    internal static SalesInvoiceDto MapToDto(Domain.Entities.SalesInvoice i) => new()
    {
        Id              = i.Id,
        InvoiceNo       = i.InvoiceNo,
        InvoiceDate     = i.InvoiceDate,
        CustomerId      = i.CustomerId,
        CustomerName    = i.Customer?.Name,
        CustomerPhone   = i.Customer?.Phone,
        BranchName      = i.Branch?.NameAr ?? "",
        WarehouseName   = i.Warehouse?.NameAr ?? "",
        CashierName     = i.User?.FullName ?? "",
        PaymentMethod   = i.PaymentMethod,
        Status          = i.Status,
        Subtotal        = i.Subtotal,
        TotalAmount     = i.TotalAmount,
        PaidAmount      = i.PaidAmount,
        RemainingAmount = i.RemainingAmount,
        Notes           = i.Notes,
        Items           = i.Items.Select(item => new SalesInvoiceItemDto
        {
            Id              = item.Id,
            ProductId       = item.ProductId,
            ProductNameAr   = item.ProductNameAr,
            Quantity        = item.Quantity,
            UnitPrice       = item.UnitPrice,
            DiscountPercent = item.DiscountPercent,
            DiscountAmount  = item.DiscountAmount,
            TaxAmount       = item.TaxAmount,
            TotalPrice      = item.TotalPrice,
            ReturnedQty     = item.ReturnedQty
        }).ToList()
    };
}

// ── GET SALE INVOICE BY NO ──────────────────────────────────────
public class GetSaleInvoiceByNoHandler
    : IRequestHandler<GetSaleInvoiceByNoQuery, Result<SalesInvoiceDto>>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public GetSaleInvoiceByNoHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result<SalesInvoiceDto>> Handle(GetSaleInvoiceByNoQuery request, CancellationToken ct)
    {
        try
        {
            var inv = await _uow.Sales.GetByInvoiceNoAsync(request.InvoiceNo, ct);
            if (inv == null) return Result<SalesInvoiceDto>.Failure("الفاتورة غير موجودة");
            return Result<SalesInvoiceDto>.Success(GetSaleInvoiceByIdHandler.MapToDto(inv));
        }
        catch (Exception ex) { return Result<SalesInvoiceDto>.Failure(ex.Message); }
    }
}

// ── GET HELD INVOICES ───────────────────────────────────────────
public class GetHeldInvoicesHandler
    : IRequestHandler<GetHeldInvoicesQuery, Result<IReadOnlyList<SalesInvoiceListDto>>>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public GetHeldInvoicesHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result<IReadOnlyList<SalesInvoiceListDto>>> Handle(
        GetHeldInvoicesQuery request, CancellationToken ct)
    {
        try
        {
            var held = await _uow.Sales.GetHeldInvoicesAsync(request.UserId, ct);
            var dtos = held.Select(i => new SalesInvoiceListDto
            {
                Id           = i.Id,
                InvoiceNo    = i.InvoiceNo,
                InvoiceDate  = i.InvoiceDate,
                CustomerName = i.Customer?.Name,
                CashierName  = i.User?.FullName ?? "",
                PaymentMethod= i.PaymentMethod,
                Status       = i.Status,
                TotalAmount     = i.TotalAmount,
                PaidAmount      = i.PaidAmount,
                RemainingAmount = i.RemainingAmount,
                ItemsCount      = i.Items.Count
            }).ToList();
            return Result<IReadOnlyList<SalesInvoiceListDto>>.Success(dtos);
        }
        catch (Exception ex) { return Result<IReadOnlyList<SalesInvoiceListDto>>.Failure(ex.Message); }
    }
}

// ── HOLD / RETRIEVE / CANCEL INVOICE ───────────────────────────
public class HoldInvoiceHandler : IRequestHandler<HoldInvoiceCommand, Result>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public HoldInvoiceHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(HoldInvoiceCommand request, CancellationToken ct)
    {
        try
        {
            var inv = await _uow.Sales.GetByIdAsync(request.InvoiceId, ct);
            if (inv == null) return Result.Failure("الفاتورة غير موجودة");
            inv.Hold();
            await _uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}

public class RetrieveInvoiceHandler : IRequestHandler<RetrieveInvoiceCommand, Result>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public RetrieveInvoiceHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(RetrieveInvoiceCommand request, CancellationToken ct)
    {
        try
        {
            var inv = await _uow.Sales.GetByIdAsync(request.InvoiceId, ct);
            if (inv == null) return Result.Failure("الفاتورة غير موجودة");
            inv.Retrieve();
            await _uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}

public class CancelInvoiceHandler : IRequestHandler<CancelInvoiceCommand, Result>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public CancelInvoiceHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(CancelInvoiceCommand request, CancellationToken ct)
    {
        try
        {
            var inv = await _uow.Sales.GetByIdAsync(request.InvoiceId, ct);
            if (inv == null) return Result.Failure("الفاتورة غير موجودة");
            inv.Cancel();
            await _uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}
