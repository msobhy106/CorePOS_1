using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CorePOS.Domain.Entities;
namespace CorePOS.Persistence.Configurations;
public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> b)
    {
        b.ToTable("Permissions");
        b.HasKey(e => e.Id);
        b.Property(e => e.ModuleKey).IsRequired().HasMaxLength(50);
        b.Property(e => e.ActionKey).IsRequired().HasMaxLength(50);
        b.Property(e => e.ModuleNameAr).IsRequired().HasMaxLength(100).UseCollation("Arabic_CI_AS");
        b.Property(e => e.ActionNameAr).IsRequired().HasMaxLength(100).UseCollation("Arabic_CI_AS");
        b.Ignore(e => e.FullKey);
        b.HasIndex(e => new { e.ModuleKey, e.ActionKey }).IsUnique();
    }
}
