using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CorePOS.Domain.Entities;

namespace CorePOS.Persistence.Configurations;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> b)
    {
        b.ToTable("Warehouses");
        b.HasKey(e => e.Id);
        b.Property(e => e.Code).IsRequired().HasMaxLength(20).UseCollation("Arabic_CI_AS");
        b.Property(e => e.Name).IsRequired().HasMaxLength(200).UseCollation("Arabic_CI_AS");
        b.Property(e => e.NameAr).IsRequired().HasMaxLength(200).UseCollation("Arabic_CI_AS");
        b.Property(e => e.Address).HasMaxLength(500);
        b.Property(e => e.ManagerName).HasMaxLength(200);
        b.HasIndex(e => e.Code).IsUnique();
        b.HasIndex(e => e.BranchId);
        b.HasOne(e => e.Branch).WithMany(b => b.Warehouses)
            .HasForeignKey(e => e.BranchId).OnDelete(DeleteBehavior.Restrict);
    }
}
