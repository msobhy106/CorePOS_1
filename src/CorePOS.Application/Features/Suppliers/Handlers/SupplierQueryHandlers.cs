using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Suppliers.Queries;
using CorePOS.Application.Features.Suppliers.Commands;
using CorePOS.Application.Features.Suppliers.DTOs;

namespace CorePOS.Application.Features.Suppliers.Handlers;

// ── GET SUPPLIERS (paged) ───────────────────────────────────────
public class GetSuppliersHandler : IRequestHandler<GetSuppliersQuery, Result<PagedResult<SupplierListDto>>>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public GetSuppliersHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result<PagedResult<SupplierListDto>>> Handle(GetSuppliersQuery request, CancellationToken ct)
    {
        try
        {
            var suppliers = !string.IsNullOrWhiteSpace(request.Search)
                ? await _uow.Suppliers.SearchAsync(request.Search, 200, ct)
                : request.DebtOnly
                    ? await _uow.Suppliers.GetWithPayablesAsync(ct)
                    : await _uow.Suppliers.GetActiveAsync(ct);

            var filtered = suppliers.AsEnumerable();
            if (request.IsActive.HasValue) filtered = filtered.Where(s => s.IsActive == request.IsActive.Value);

            var list  = filtered.ToList();
            var total = list.Count;
            var items = list
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(s => new SupplierListDto
                {
                    Id             = s.Id,
                    Code           = s.Code,
                    Name           = s.Name,
                    Phone          = s.Phone,
                    ContactPerson  = s.ContactPerson,
                    CurrentBalance = s.CurrentBalance,
                    IsActive       = s.IsActive
                }).ToList();

            return Result<PagedResult<SupplierListDto>>.Success(
                new PagedResult<SupplierListDto>(items, total, request.PageNumber, request.PageSize));
        }
        catch (Exception ex) { return Result<PagedResult<SupplierListDto>>.Failure(ex.Message); }
    }
}

// ── GET SUPPLIER BY ID ──────────────────────────────────────────
public class GetSupplierByIdHandler : IRequestHandler<GetSupplierByIdQuery, Result<SupplierDto>>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public GetSupplierByIdHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result<SupplierDto>> Handle(GetSupplierByIdQuery request, CancellationToken ct)
    {
        try
        {
            var s = await _uow.Suppliers.GetByIdAsync(request.Id, ct);
            if (s == null) return Result<SupplierDto>.Failure("المورد غير موجود");

            return Result<SupplierDto>.Success(new SupplierDto
            {
                Id               = s.Id,
                Code             = s.Code,
                Name             = s.Name,
                Phone            = s.Phone,
                Phone2           = s.Phone2,
                Address          = s.Address,
                Email            = s.Email,
                TaxNumber        = s.TaxNumber,
                ContactPerson    = s.ContactPerson,
                CurrentBalance   = s.CurrentBalance,
                CreditLimit      = s.CreditLimit,
                PaymentPeriodDays= s.PaymentPeriodDays,
                IsActive         = s.IsActive
            });
        }
        catch (Exception ex) { return Result<SupplierDto>.Failure(ex.Message); }
    }
}

// ── RECORD SUPPLIER PAYMENT ─────────────────────────────────────
public class RecordSupplierPaymentHandler : IRequestHandler<RecordSupplierPaymentCommand, Result<int>>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public RecordSupplierPaymentHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result<int>> Handle(RecordSupplierPaymentCommand request, CancellationToken ct)
    {
        try
        {
            var supplier = await _uow.Suppliers.GetByIdAsync(request.SupplierId, ct);
            if (supplier == null) return Result<int>.Failure("المورد غير موجود");

            // Generate a payment number
            var paymentNo = $"SP-{DateTime.Today:yyyyMMdd}-{request.SupplierId:D4}";

            var payment = Domain.Entities.SupplierPayment.Create(
                paymentNo:  paymentNo,
                supplierId: request.SupplierId,
                amount:     request.Amount,
                method:     request.PaymentMethod,
                userId:     request.UserId == 0 ? 1 : request.UserId,
                cashBoxId:  request.CashBoxId,
                shiftId:    request.ShiftId,
                notes:      request.Notes);

            await _uow.Suppliers.UpdateBalanceAsync(
                request.SupplierId,
                supplier.CurrentBalance - request.Amount, ct);

            await _uow.SaveChangesAsync(ct);
            return Result<int>.Success(payment.Id);
        }
        catch (Exception ex) { return Result<int>.Failure(ex.Message); }
    }
}
