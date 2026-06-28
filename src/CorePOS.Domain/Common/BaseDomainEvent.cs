namespace CorePOS.Domain.Common;

/// <summary>Base class for all domain events.</summary>
public abstract class BaseDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
