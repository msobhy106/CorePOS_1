using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Interfaces;
using CorePOS.Application.Features.License.Commands;
using CorePOS.Application.Features.License.Queries;

namespace CorePOS.Application.Features.License.Handlers;

// ── GET LICENSE STATUS ──────────────────────────────────────────
public class GetLicenseStatusHandler : IRequestHandler<GetLicenseStatusQuery, Result<LicenseStatus>>
{
    private readonly ILicenseService _licenseService;
    public GetLicenseStatusHandler(ILicenseService licenseService) => _licenseService = licenseService;

    public async Task<Result<LicenseStatus>> Handle(GetLicenseStatusQuery request, CancellationToken ct)
    {
        try
        {
            var info = await _licenseService.ValidateLicenseAsync(ct);
            return Result<LicenseStatus>.Success(info.Status);
        }
        catch (Exception ex) { return Result<LicenseStatus>.Failure(ex.Message); }
    }
}

// ── ACTIVATE LICENSE ────────────────────────────────────────────
public class ActivateLicenseHandler : IRequestHandler<ActivateLicenseCommand, Result>
{
    private readonly ILicenseService _licenseService;
    public ActivateLicenseHandler(ILicenseService licenseService) => _licenseService = licenseService;

    public async Task<Result> Handle(ActivateLicenseCommand request, CancellationToken ct)
    {
        try
        {
            var result = await _licenseService.ActivateLicenseAsync(request.ActivationCode, ct);
            return result.Success ? Result.Success() : Result.Failure(result.Error ?? "فشل التفعيل");
        }
        catch (Exception ex) { return Result.Failure(ex.Message); }
    }
}
