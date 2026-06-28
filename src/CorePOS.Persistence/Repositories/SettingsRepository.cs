using Microsoft.EntityFrameworkCore;
using CorePOS.Domain.Entities;
using CorePOS.Domain.Interfaces;
using CorePOS.Persistence.DbContexts;

namespace CorePOS.Persistence.Repositories;

public class SettingsRepository : ISettingsRepository
{
    private readonly CorePOSDbContext _db;
    public SettingsRepository(CorePOSDbContext db) => _db = db;

    public async Task<Setting?> GetByKeyAsync(string key, CancellationToken ct = default)
        => await _db.Settings.FirstOrDefaultAsync(s => s.SettingKey == key, ct);

    public async Task<IReadOnlyList<Setting>> GetByGroupAsync(
        string group, CancellationToken ct = default)
        => await _db.Settings.Where(s => s.SettingGroup == group).ToListAsync(ct);

    public async Task<IReadOnlyList<Setting>> GetAllAsync(CancellationToken ct = default)
        => await _db.Settings.OrderBy(s => s.SettingGroup).ThenBy(s => s.SettingKey).ToListAsync(ct);

    public async Task UpsertAsync(string key, string? value, CancellationToken ct = default)
    {
        var existing = await GetByKeyAsync(key, ct);
        if (existing is null)
        {
            var s = Setting.Create(key, value);
            await _db.Settings.AddAsync(s, ct);
        }
        else
        {
            existing.UpdateValue(value);
            _db.Settings.Update(existing);
        }
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpsertRangeAsync(
        IDictionary<string, string?> keyValues, CancellationToken ct = default)
    {
        foreach (var kv in keyValues)
            await UpsertAsync(kv.Key, kv.Value, ct);
    }

    public async Task<string> GetStringAsync(
        string key, string defaultValue = "", CancellationToken ct = default)
    {
        var s = await GetByKeyAsync(key, ct);
        return s?.AsString(defaultValue) ?? defaultValue;
    }

    public async Task<int> GetIntAsync(
        string key, int defaultValue = 0, CancellationToken ct = default)
    {
        var s = await GetByKeyAsync(key, ct);
        return s?.AsInt(defaultValue) ?? defaultValue;
    }

    public async Task<bool> GetBoolAsync(
        string key, bool defaultValue = false, CancellationToken ct = default)
    {
        var s = await GetByKeyAsync(key, ct);
        return s?.AsBool(defaultValue) ?? defaultValue;
    }

    public async Task<decimal> GetDecimalAsync(
        string key, decimal defaultValue = 0, CancellationToken ct = default)
    {
        var s = await GetByKeyAsync(key, ct);
        return s?.AsDecimal(defaultValue) ?? defaultValue;
    }
}
