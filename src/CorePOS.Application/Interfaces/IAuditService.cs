namespace CorePOS.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(int userId, string action, string entityName,
        string? entityId = null, string? oldValues = null, string? newValues = null,
        CancellationToken cancellationToken = default);

    string SerializeEntity<T>(T entity);
}
