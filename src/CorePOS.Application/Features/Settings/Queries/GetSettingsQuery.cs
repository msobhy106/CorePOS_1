using MediatR;
using CorePOS.Application.Common;
using CorePOS.Domain.Entities;

namespace CorePOS.Application.Features.Settings.Queries;

public record GetSettingQuery(string Key)         : IRequest<Result<Setting?>>;
public record GetSettingsByGroupQuery(string Group): IRequest<Result<IReadOnlyList<Setting>>>;
