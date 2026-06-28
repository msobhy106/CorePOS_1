using MediatR;
using CorePOS.Application.Common;

namespace CorePOS.Application.Features.License.Commands;

public record ActivateLicenseCommand(string ActivationCode) : IRequest<Result>;
