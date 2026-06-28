using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Features.Branches.Commands;
using CorePOS.Application.Features.Branches.Queries;
using CorePOS.Application.Features.Branches.DTOs;

namespace CorePOS.Application.Features.Branches.Handlers;

// ── GET BRANCHES ────────────────────────────────────────────────
public class GetBranchesHandler : IRequestHandler<GetBranchesQuery, Result<IReadOnlyList<BranchDto>>>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public GetBranchesHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result<IReadOnlyList<BranchDto>>> Handle(GetBranchesQuery request, CancellationToken ct)
    {
        try
        {
            var branches = request.IsActive.HasValue
                ? await _uow.Branches.GetAsync(b => b.IsActive == request.IsActive.Value, ct)
                : await _uow.Branches.GetAllAsync(ct);

            var dtos = branches.Select(b => new BranchDto
            {
                Id          = b.Id,
                Code        = b.Code,
                NameAr      = b.NameAr,
                Address     = b.Address,
                Phone       = b.Phone,
                ManagerName = b.ManagerName,
                IsMain      = b.IsMain,
                IsActive    = b.IsActive
            }).ToList();

            return Result<IReadOnlyList<BranchDto>>.Success(dtos);
        }
        catch (Exception ex) { return Result<IReadOnlyList<BranchDto>>.Failure(ex.Message); }
    }
}

// ── CREATE BRANCH ───────────────────────────────────────────────
public class CreateBranchHandler : IRequestHandler<CreateBranchCommand, Result<int>>
{
    private readonly Domain.Interfaces.IUnitOfWork _uow;
    public CreateBranchHandler(Domain.Interfaces.IUnitOfWork uow) => _uow = uow;

    public async Task<Result<int>> Handle(CreateBranchCommand request, CancellationToken ct)
    {
        try
        {
            if (await _uow.Branches.CodeExistsAsync(request.Code, null, ct))
                return Result<int>.Failure($"كود الفرع '{request.Code}' مستخدم بالفعل");

            var branch = Domain.Entities.Branch.Create(
                request.Code, request.Name, request.NameAr,
                request.Address, request.Phone, request.ManagerName, request.IsMain);

            await _uow.Branches.AddAsync(branch, ct);
            await _uow.SaveChangesAsync(ct);
            return Result<int>.Success(branch.Id);
        }
        catch (Exception ex) { return Result<int>.Failure(ex.Message); }
    }
}
