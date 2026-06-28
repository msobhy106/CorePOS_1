using System.Text.Json;
using CorePOS.Application.Interfaces;
using CorePOS.Domain.Interfaces;

namespace CorePOS.Infrastructure.Logging;

public class AuditService : IAuditService
{
    private readonly IUserRepository _users;

    public AuditService(IUserRepository users) => _users = users;

    public async Task LogAsync(int userId, string action, string entityName,
        string? entityId = null, string? oldValues = null, string? newValues = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _users.LogAuditAsync(userId, action, entityName,
                entityId, oldValues, newValues,
                Environment.MachineName, cancellationToken);
        }
        catch
        {
            // Audit logging should never break the main flow
        }
    }

    public string SerializeEntity<T>(T entity)
    {
        try
        {
            return JsonSerializer.Serialize(entity, new JsonSerializerOptions
            {
                WriteIndented          = false,
                PropertyNamingPolicy   = JsonNamingPolicy.CamelCase,
                ReferenceHandler       = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            });
        }
        catch { return string.Empty; }
    }
}
