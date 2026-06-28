using MediatR;
using CorePOS.Application.Common;
using CorePOS.Application.Interfaces;

namespace CorePOS.Application.Features.License.Queries;

public record GetLicenseStatusQuery : IRequest<Result<LicenseStatus>>;
