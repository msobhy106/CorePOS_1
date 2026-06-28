using MediatR;
using CorePOS.Application.Interfaces;

namespace CorePOS.Application.Behaviors;

/// <summary>Marks commands that should be audit-logged.</summary>
public interface IAuditableCommand
{
    string EntityName { get; }
    string? EntityId  { get; }
}

public class AuditBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IAuditService      _audit;
    private readonly ICurrentUserService _user;

    public AuditBehavior(IAuditService audit, ICurrentUserService user)
    {
        _audit = audit;
        _user  = user;
    }

    public async Task<TResponse> Handle(TRequest request,
        RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var response = await next();

        if (request is IAuditableCommand cmd && _user.UserId > 0)
        {
            await _audit.LogAsync(_user.UserId, typeof(TRequest).Name,
                cmd.EntityName, cmd.EntityId,
                cancellationToken: ct);
        }

        return response;
    }
}
