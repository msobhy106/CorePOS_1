using MediatR;
using CorePOS.Application.Common;

namespace CorePOS.Application.Features.Settings.Commands;

public record UpdateSettingCommand(string Key, string? Value) : IRequest<Result>;
public record UpdateSettingsCommand(IDictionary<string, string?> KeyValues) : IRequest<Result>;
