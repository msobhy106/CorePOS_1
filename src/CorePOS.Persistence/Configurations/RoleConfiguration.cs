using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CorePOS.Domain.Entities;

namespace CorePOS.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> b)
    {
        b.ToTable("Roles");
        b.HasKey(e => e.Id);
        b.Property(e => e.Name).IsRequired().HasMaxLength(100);
        b.Property(e => e.NameAr).IsRequired().HasMaxLength(100).UseCollation("Arabic_CI_AS");
        b.Property(e => e.Description).HasMaxLength(500);
        b.HasIndex(e => e.Name).IsUnique();
        b.HasMany(e => e.RolePermissions).WithOne(rp => rp.Role)
            .HasForeignKey(rp => rp.RoleId).OnDelete(DeleteBehavior.Cascade);
    }
}
