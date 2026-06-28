using Microsoft.EntityFrameworkCore;
using CorePOS.Domain.Entities;
using CorePOS.Domain.Interfaces;
using CorePOS.Persistence.DbContexts;

namespace CorePOS.Persistence.Repositories;

public class LicenseRepository : ILicenseRepository
{
    private readonly CorePOSDbContext _db;
    public LicenseRepository(CorePOSDbContext db) => _db = db;

    public async Task<License?> GetActiveAsync(CancellationToken ct = default)
        => await _db.Licenses
            .Where(l => l.IsActive)
            .OrderByDescending(l => l.CreatedAt)
            .FirstOrDefaultAsync(ct);

    public async Task<License?> GetByKeyAsync(
        string licenseKey, CancellationToken ct = default)
        => await _db.Licenses.FirstOrDefaultAsync(l => l.LicenseKey == licenseKey, ct);

    public async Task AddAsync(License license, CancellationToken ct = default)
        => await _db.Licenses.AddAsync(license, ct);

    public Task UpdateAsync(License license, CancellationToken ct = default)
    {
        _db.Licenses.Update(license);
        return Task.CompletedTask;
    }

    public async Task<bool> HasValidLicenseAsync(CancellationToken ct = default)
    {
        var license = await GetActiveAsync(ct);
        return license?.IsValid ?? false;
    }
}
