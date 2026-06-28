using CorePOS.Domain.Entities;

namespace CorePOS.Domain.Interfaces;

public interface ISettingsRepository
{
    Task<Setting?> GetByKeyAsync(string key, CancellationToken ct = default);
    Task<IReadOnlyList<Setting>> GetByGroupAsync(string group, CancellationToken ct = default);
    Task<IReadOnlyList<Setting>> GetAllAsync(CancellationToken ct = default);
    Task UpsertAsync(string key, string? value, CancellationToken ct = default);
    Task UpsertRangeAsync(IDictionary<string, string?> keyValues, CancellationToken ct = default);

    // Typed convenience methods
    Task<string>  GetStringAsync(string key, string defaultValue = "", CancellationToken ct = default);
    Task<int>     GetIntAsync(string key, int defaultValue = 0, CancellationToken ct = default);
    Task<bool>    GetBoolAsync(string key, bool defaultValue = false, CancellationToken ct = default);
    Task<decimal> GetDecimalAsync(string key, decimal defaultValue = 0, CancellationToken ct = default);
}
