using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Settings.Commands;
using CorePOS.Application.Features.Settings.Queries;
using CorePOS.Domain.Entities;

namespace CorePOS.Application.Features.Settings.Handlers;

// ── GET SETTING ─────────────────────────────────────────────────
public class GetSettingHandler : IRequestHandler<GetSettingQuery, Result<Setting?>>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public GetSettingHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result<Setting?>> Handle(GetSettingQuery request, CancellationToken ct)
    {
        try
        {
            var setting = await _uow.Settings.GetByKeyAsync(request.Key, ct);
            return Result<Setting?>.Success(setting);
        }
        catch (Exception ex) { return Result<Setting?>.Failure(ex.Message); }
    }
}

// ── GET SETTINGS BY GROUP ────────────────────────────────────────
public class GetSettingsByGroupHandler : IRequestHandler<GetSettingsByGroupQuery, Result<IReadOnlyList<Setting>>>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public GetSettingsByGroupHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result<IReadOnlyList<Setting>>> Handle(GetSettingsByGroupQuery request, CancellationToken ct)
    {
        try
        {
            var settings = await _uow.Settings.GetByGroupAsync(request.Group, ct);
            return Result<IReadOnlyList<Setting>>.Success(settings);
        }
        catch (Exception ex) { return Result<IReadOnlyList<Setting>>.Failure(ex.Message); }
    }
}

// ── UPDATE SETTING ───────────────────────────────────────────────
public class UpdateSettingHandler : IRequestHandler<UpdateSettingCommand, Result>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public UpdateSettingHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(UpdateSettingCommand request, CancellationToken ct)
    {
        try
        {
            await _uow.Settings.UpsertAsync(request.Key, request.Value, ct);
            await _uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}

// ── UPDATE SETTINGS (BATCH) ──────────────────────────────────────
public class UpdateSettingsHandler : IRequestHandler<UpdateSettingsCommand, Result>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public UpdateSettingsHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(UpdateSettingsCommand request, CancellationToken ct)
    {
        try
        {
            await _uow.Settings.UpsertRangeAsync(request.KeyValues, ct);
            await _uow.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}
