using MediatR;
using Microsoft.EntityFrameworkCore;
using CorePOS.Application.Common;
using CorePOS.Application.Interfaces;
using CorePOS.Application.Features.Units.Queries;
using CorePOS.Application.Features.Units.DTOs;

namespace CorePOS.Application.Features.Units.Handlers;

public class GetUnitsHandler : IRequestHandler<GetUnitsQuery, Result<IReadOnlyList<UnitDto>>>
{
    private readonly IApplicationDbContext _db;
    public GetUnitsHandler(IApplicationDbContext db) => _db = db;

    public async Task<Result<IReadOnlyList<UnitDto>>> Handle(GetUnitsQuery request, CancellationToken ct)
    {
        try
        {
            var query = _db.Units.AsQueryable();
            if (request.IsActive.HasValue)
                query = query.Where(u => u.IsActive == request.IsActive.Value);

            var units = await query.OrderBy(u => u.NameAr).ToListAsync(ct);
            var dtos  = units.Select(u => new UnitDto
            {
                Id           = u.Id,
                Code         = u.Code,
                NameAr       = u.NameAr,
                NameEn       = u.NameAr, // NameEn not on entity — use NameAr
                Abbreviation = u.Abbreviation,
                IsActive     = u.IsActive
            }).ToList();

            return Result<IReadOnlyList<UnitDto>>.Success(dtos);
        }
        catch (Exception ex) { return Result<IReadOnlyList<UnitDto>>.Failure(ex.Message); }
    }
}
