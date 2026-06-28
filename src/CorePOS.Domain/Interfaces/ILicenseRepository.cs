using CorePOS.Domain.Entities;

namespace CorePOS.Domain.Interfaces;

public interface ILicenseRepository
{
    Task<License?> GetActiveAsync(CancellationToken ct = default);
    Task<License?> GetByKeyAsync(string licenseKey, CancellationToken ct = default);
    Task AddAsync(License license, CancellationToken ct = default);
    Task UpdateAsync(License license, CancellationToken ct = default);
    Task<bool> HasValidLicenseAsync(CancellationToken ct = default);
}
