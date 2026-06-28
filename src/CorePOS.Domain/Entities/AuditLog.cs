using CorePOS.Domain.Common;

namespace CorePOS.Domain.Entities;

public class AuditLog : BaseEntity
{
    public int     UserId      { get; private set; }
    public string  Action      { get; private set; } = string.Empty;
    public string  EntityName  { get; private set; } = string.Empty;
    public string? EntityId    { get; private set; }
    public string? OldValues   { get; private set; }
    public string? NewValues   { get; private set; }
    public string? IPAddress   { get; private set; }
    public string? MachineName { get; private set; }
    public DateTime CreatedAt  { get; private set; } = DateTime.UtcNow;

    public User? User { get; private set; }

    protected AuditLog() { }

    public static AuditLog Create(int userId, string action, string entityName,
        string? entityId = null, string? oldValues = null, string? newValues = null,
        string? machineName = null, string? ipAddress = null)
        => new()
        {
            UserId = userId, Action = action, EntityName = entityName,
            EntityId = entityId, OldValues = oldValues, NewValues = newValues,
            MachineName = machineName, IPAddress = ipAddress
        };
}
