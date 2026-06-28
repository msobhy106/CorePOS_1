namespace CorePOS.Domain.Common;

/// <summary>
/// Base class for all domain entities.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; protected set; }

    private readonly List<BaseDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<BaseDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(BaseDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();

    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        if (Id == 0 || other.Id == 0) return false;
        return Id == other.Id;
    }
    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
    public static bool operator ==(BaseEntity? a, BaseEntity? b)
    {
        if (a is null && b is null) return true;
        if (a is null || b is null) return false;
        return a.Equals(b);
    }
    public static bool operator !=(BaseEntity? a, BaseEntity? b) => !(a == b);
}
