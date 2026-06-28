using Microsoft.EntityFrameworkCore;
using CorePOS.Domain.Entities;
using CorePOS.Domain.Interfaces;
using CorePOS.Persistence.DbContexts;

namespace CorePOS.Persistence.Repositories;

public class ShiftRepository : BaseRepository<Shift>, IShiftRepository
{
    public ShiftRepository(CorePOSDbContext db) : base(db) { }

    public async Task<Shift?> GetOpenShiftAsync(int userId, CancellationToken ct = default)
        => await _set
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == Domain.Enums.ShiftStatus.Open, ct);

    public async Task<Shift?> GetOpenShiftByBranchAsync(int branchId, CancellationToken ct = default)
        => await _set
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.BranchId == branchId && s.Status == Domain.Enums.ShiftStatus.Open, ct);

    public async Task<IReadOnlyList<Shift>> GetRecentAsync(int branchId, DateTime from, CancellationToken ct = default)
        => await _set
            .Include(s => s.User)
            .Where(s => s.BranchId == branchId && s.StartTime >= from)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync(ct);

    public async Task<string?> GetLastShiftNoAsync(int branchId, CancellationToken ct = default)
        => await _set
            .Where(s => s.BranchId == branchId)
            .OrderByDescending(s => s.Id)
            .Select(s => s.ShiftNo)
            .FirstOrDefaultAsync(ct);

    public async Task<string> GenerateShiftNoAsync(int branchId, CancellationToken ct = default)
    {
        var today    = DateTime.Today.ToString("yyyyMMdd");
        var lastNo   = await GetLastShiftNoAsync(branchId, ct);
        if (string.IsNullOrEmpty(lastNo)) return $"SH-{today}-001";
        var parts    = lastNo.Split('-');
        var lastDate = parts.Length >= 2 ? parts[1] : "";
        if (lastDate != today) return $"SH-{today}-001";
        var seq = parts.Length >= 3 && int.TryParse(parts[2], out var n) ? n + 1 : 1;
        return $"SH-{today}-{seq:D3}";
    }
}
