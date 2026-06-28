using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CorePOS.Domain.Entities;

namespace CorePOS.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.ToTable("AuditLogs");
        b.HasKey(e => e.Id);
        b.Property(e => e.Action).IsRequired().HasMaxLength(100);
        b.Property(e => e.EntityName).IsRequired().HasMaxLength(100);
        b.Property(e => e.EntityId).HasMaxLength(50);
        b.Property(e => e.IPAddress).HasMaxLength(50);
        b.Property(e => e.MachineName).HasMaxLength(100);
        b.HasIndex(e => e.UserId);
        b.HasIndex(e => new { e.EntityName, e.EntityId });
        b.HasIndex(e => e.CreatedAt);
        b.HasOne(e => e.User).WithMany()
            .HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}
