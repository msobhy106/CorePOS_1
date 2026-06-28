using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CorePOS.Domain.Entities;
namespace CorePOS.Persistence.Configurations;
public class StockAdjustmentConfiguration : IEntityTypeConfiguration<StockAdjustment>
{
    public void Configure(EntityTypeBuilder<StockAdjustment> b)
    {
        b.ToTable("StockAdjustments");
        b.HasKey(e => e.Id);
        b.Property(e => e.AdjustmentNo).IsRequired().HasMaxLength(50);
        b.Property(e => e.Notes).HasMaxLength(500);
        b.HasOne(e => e.Warehouse).WithMany().HasForeignKey(e => e.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(e => e.AdjustmentNo).IsUnique();
    }
}
