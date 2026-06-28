using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CorePOS.Domain.Entities;

namespace CorePOS.Persistence.Configurations;

public class ProductStockConfiguration : IEntityTypeConfiguration<ProductStock>
{
    public void Configure(EntityTypeBuilder<ProductStock> b)
    {
        b.ToTable("ProductStock");
        b.HasKey(e => e.Id);
        b.HasIndex(e => new { e.ProductId, e.WarehouseId }).IsUnique();
        b.Property(e => e.Quantity).HasColumnType("decimal(18,3)");
        b.Property(e => e.AverageCost).HasColumnType("decimal(18,4)");
        b.Property(e => e.LastCost).HasColumnType("decimal(18,4)");
        b.HasOne(e => e.Warehouse).WithMany()
            .HasForeignKey(e => e.WarehouseId).OnDelete(DeleteBehavior.Restrict);
    }
}
