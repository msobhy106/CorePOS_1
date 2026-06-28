using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CorePOS.Domain.Entities;
namespace CorePOS.Persistence.Configurations;
public class InventorySessionItemConfiguration : IEntityTypeConfiguration<InventorySessionItem>
{
    public void Configure(EntityTypeBuilder<InventorySessionItem> b)
    {
        b.ToTable("InventorySessionItems");
        b.HasKey(e => e.Id);
        b.Property(e => e.SystemQuantity).HasPrecision(18, 4);
        b.Property(e => e.ActualQuantity).HasPrecision(18, 4);
        b.Property(e => e.UnitCost).HasPrecision(18, 4);
        b.Property(e => e.Notes).HasMaxLength(500);
        b.Ignore(e => e.Difference);
        b.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(e => new { e.SessionId, e.ProductId }).IsUnique();
    }
}
