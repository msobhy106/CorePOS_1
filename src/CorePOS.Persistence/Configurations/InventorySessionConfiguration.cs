using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CorePOS.Domain.Entities;

namespace CorePOS.Persistence.Configurations;

public class InventorySessionConfiguration : IEntityTypeConfiguration<InventorySession>
{
    public void Configure(EntityTypeBuilder<InventorySession> b)
    {
        b.ToTable("InventorySessions");
        b.HasKey(e => e.Id);
        b.Property(e => e.SessionNo).IsRequired().HasMaxLength(50);
        b.Property(e => e.CountType).HasConversion<byte>();
        b.HasIndex(e => e.SessionNo).IsUnique();
        b.HasOne(e => e.Warehouse).WithMany()
            .HasForeignKey(e => e.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(e => e.Items).WithOne()
            .HasForeignKey(i => i.SessionId).OnDelete(DeleteBehavior.Cascade);
    }
}
