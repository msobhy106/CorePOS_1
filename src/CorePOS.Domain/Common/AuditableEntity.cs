namespace CorePOS.Domain.Common;

/// <summary>
/// Auditable entity — tracks creation and modification timestamps and user.
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public int?  CreatedBy     { get; set; }
    public bool  IsDeleted     { get; protected set; } = false;

    public void SoftDelete() => IsDeleted = true;
    public void Restore()    => IsDeleted = false;
}
