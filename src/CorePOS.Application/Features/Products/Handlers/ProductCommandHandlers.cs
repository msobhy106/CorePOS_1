using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Products.Commands;

namespace CorePOS.Application.Features.Products.Handlers;

// ── DELETE PRODUCT ──────────────────────────────────────────────
public class DeleteProductHandler : IRequestHandler<DeleteProductCommand, Result>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public DeleteProductHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken ct)
    {
        try
        {
            var product = await _uow.Products.GetByIdAsync(request.Id, ct);
            if (product == null) return Result.Failure("المنتج غير موجود");
            _uow.Products.Remove(product);
            await _uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}

// ── TOGGLE PRODUCT STATUS ───────────────────────────────────────
public class ToggleProductStatusHandler : IRequestHandler<ToggleProductStatusCommand, Result>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public ToggleProductStatusHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(ToggleProductStatusCommand request, CancellationToken ct)
    {
        try
        {
            var product = await _uow.Products.GetByIdAsync(request.Id, ct);
            if (product == null) return Result.Failure("المنتج غير موجود");
            if (request.IsActive) product.Activate();
            else product.Deactivate();
            await _uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}
