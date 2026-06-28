using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using CorePOS.Application.Interfaces;
using CorePOS.Infrastructure.Persistence;

namespace CorePOS.Infrastructure.Repositories;

/// <summary>
/// Settings repository — reads/writes from Settings table.
/// Uses in-memory cache (60 seconds) to avoid hitting DB on every print.
/// </summary>
public class SettingsRepository : ISettingsRepository
{
    private readonly AppDbContext  _db;
    private readonly IMemoryCache  _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(60);

    public SettingsRepository(AppDbContext db, IMemoryCache cache)
    {
        _db    = db;
        _cache = cache;
    }

    // ── Read ──────────────────────────────────────────────────────
    public async Task<string> GetStringAsync(
        string key, string defaultValue = "", CancellationToken ct = default)
    {
        var cacheKey = $"setting:{key}";
        if (_cache.TryGetValue(cacheKey, out string? cached))
            return cached ?? defaultValue;

        var row = await _db.Settings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SettingKey == key, ct);

        var value = row?.SettingValue ?? defaultValue;
        _cache.Set(cacheKey, value, CacheDuration);
        return value;
    }

    public async Task<bool> GetBoolAsync(
        string key, bool defaultValue = false, CancellationToken ct = default)
    {
        var s = await GetStringAsync(key, defaultValue.ToString(), ct);
        return bool.TryParse(s, out var b) ? b : defaultValue;
    }

    public async Task<int> GetIntAsync(
        string key, int defaultValue = 0, CancellationToken ct = default)
    {
        var s = await GetStringAsync(key, defaultValue.ToString(), ct);
        return int.TryParse(s, out var i) ? i : defaultValue;
    }

    public async Task<decimal> GetDecimalAsync(
        string key, decimal defaultValue = 0, CancellationToken ct = default)
    {
        var s = await GetStringAsync(key, defaultValue.ToString(), ct);
        return decimal.TryParse(s, out var d) ? d : defaultValue;
    }

    // ── Write ─────────────────────────────────────────────────────
    public async Task SetStringAsync(string key, string value, CancellationToken ct = default)
    {
        var row = await _db.Settings.FirstOrDefaultAsync(s => s.SettingKey == key, ct);
        if (row == null)
        {
            row = new Domain.Entities.Setting { SettingKey = key, SettingValue = value };
            _db.Settings.Add(row);
        }
        else
        {
            row.SettingValue = value;
        }
        await _db.SaveChangesAsync(ct);
        _cache.Remove($"setting:{key}");
    }

    public async Task SetBoolAsync(string key, bool value, CancellationToken ct = default)
        => await SetStringAsync(key, value.ToString(), ct);

    public async Task SetIntAsync(string key, int value, CancellationToken ct = default)
        => await SetStringAsync(key, value.ToString(), ct);
}
