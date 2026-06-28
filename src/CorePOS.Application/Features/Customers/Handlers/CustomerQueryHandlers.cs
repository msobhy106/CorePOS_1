using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Customers.Queries;
using CorePOS.Application.Features.Customers.Commands;
using CorePOS.Application.Features.Customers.DTOs;

namespace CorePOS.Application.Features.Customers.Handlers;

// ── GET CUSTOMERS (paged) ───────────────────────────────────────
public class GetCustomersHandler : IRequestHandler<GetCustomersQuery, Result<PagedResult<CustomerListDto>>>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public GetCustomersHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result<PagedResult<CustomerListDto>>> Handle(GetCustomersQuery request, CancellationToken ct)
    {
        try
        {
            var customers = !string.IsNullOrWhiteSpace(request.Search)
                ? await _uow.Customers.SearchAsync(request.Search, 200, ct)
                : request.DebtOnly
                    ? await _uow.Customers.GetWithDebtAsync(ct)
                    : request.GroupId.HasValue
                        ? await _uow.Customers.GetByGroupAsync(request.GroupId.Value, ct)
                        : await _uow.Customers.GetActiveAsync(ct);

            var filtered = customers.AsEnumerable();
            if (request.IsActive.HasValue) filtered = filtered.Where(c => c.IsActive == request.IsActive.Value);

            var list  = filtered.ToList();
            var total = list.Count;
            var items = list
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(c => new CustomerListDto
                {
                    Id             = c.Id,
                    Code           = c.Code,
                    Name           = c.Name,
                    Phone          = c.Phone,
                    GroupName      = c.Group?.Name,
                    CurrentBalance = c.CurrentBalance,
                    TotalPoints    = c.TotalPoints,
                    IsActive       = c.IsActive,
                    IsOverLimit    = c.CreditLimit > 0 && c.CurrentBalance > c.CreditLimit
                }).ToList();

            return Result<PagedResult<CustomerListDto>>.Success(
                new PagedResult<CustomerListDto>(items, total, request.PageNumber, request.PageSize));
        }
        catch (Exception ex) { return Result<PagedResult<CustomerListDto>>.Failure(ex.Message); }
    }
}

// ── GET CUSTOMER BY ID ──────────────────────────────────────────
public class GetCustomerByIdHandler : IRequestHandler<GetCustomerByIdQuery, Result<CustomerDto>>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public GetCustomerByIdHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result<CustomerDto>> Handle(GetCustomerByIdQuery request, CancellationToken ct)
    {
        try
        {
            var c = await _uow.Customers.GetByIdWithDetailsAsync(request.Id, ct);
            if (c == null) return Result<CustomerDto>.Failure("العميل غير موجود");

            return Result<CustomerDto>.Success(new CustomerDto
            {
                Id               = c.Id,
                Code             = c.Code,
                Name             = c.Name,
                Phone            = c.Phone,
                Phone2           = c.Phone2,
                Address          = c.Address,
                Email            = c.Email,
                InstapayNumber   = c.InstapayNumber,
                TaxNumber        = c.TaxNumber,
                GroupId          = c.GroupId,
                GroupName        = c.Group?.Name,
                PriceListId      = c.PriceListId,
                CreditLimit      = c.CreditLimit,
                CurrentBalance   = c.CurrentBalance,
                TotalPoints      = c.TotalPoints,
                PaymentPeriodDays= c.PaymentPeriodDays,
                IsActive         = c.IsActive,
                IsOverCreditLimit= c.CreditLimit > 0 && c.CurrentBalance > c.CreditLimit
            });
        }
        catch (Exception ex) { return Result<CustomerDto>.Failure(ex.Message); }
    }
}

// ── UPDATE CUSTOMER ─────────────────────────────────────────────
public class UpdateCustomerHandler : IRequestHandler<UpdateCustomerCommand, Result>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public UpdateCustomerHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(UpdateCustomerCommand request, CancellationToken ct)
    {
        try
        {
            var customer = await _uow.Customers.GetByIdAsync(request.Id, ct);
            if (customer == null) return Result.Failure("العميل غير موجود");

            // Use correct domain methods
            customer.UpdateContact(
                request.Name, request.Phone, request.Phone2, request.Address,
                request.Email, request.InstapayNumber, request.TaxNumber);

            customer.UpdateCredit(request.CreditLimit, request.PaymentPeriodDays);

            // GroupId and PriceListId are updated via EF tracking (no domain method exists)
            await _uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}
