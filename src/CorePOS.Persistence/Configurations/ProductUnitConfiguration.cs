using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CorePOS.Domain.Entities;
namespace CorePOS.Persistence.Configurations;
public class ProductUnitConfiguration : IEntityTypeConfiguration<ProductUnit>
{
    public void Configure(EntityTypeBuilder<ProductUnit> b)
    {
        b.ToTable("ProductUnits");
        b.HasKey(e => e.Id);
        b.Property(e => e.ConversionFactor).HasPrecision(18, 6).HasDefaultValue(1m);
        b.Property(e => e.Barcode).HasMaxLength(50);
        b.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(e => e.Unit).WithMany().HasForeignKey(e => e.UnitId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(e => new { e.ProductId, e.UnitId }).IsUnique();
    }
}
