using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CorePOS.Domain.Entities;

namespace CorePOS.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> b)
    {
        b.ToTable("Products");
        b.HasKey(e => e.Id);

        b.Property(e => e.Code).IsRequired().HasMaxLength(50).UseCollation("Arabic_CI_AS");
        b.Property(e => e.Barcode).HasMaxLength(100);
        b.Property(e => e.SecondBarcode).HasMaxLength(100);
        b.Property(e => e.NameAr).IsRequired().HasMaxLength(300).UseCollation("Arabic_CI_AS");
        b.Property(e => e.NameEn).HasMaxLength(300);
        b.Property(e => e.Manufacturer).HasMaxLength(200);
        b.Property(e => e.PurchasePrice).HasColumnType("decimal(18,4)");
        b.Property(e => e.SalePrice).HasColumnType("decimal(18,4)");
        b.Property(e => e.WholesalePrice).HasColumnType("decimal(18,4)");
        b.Property(e => e.HalfWholesalePrice).HasColumnType("decimal(18,4)");
        b.Property(e => e.SpecialPrice).HasColumnType("decimal(18,4)");
        b.Property(e => e.TaxPercent).HasColumnType("decimal(5,2)");
        b.Property(e => e.MinStock).HasColumnType("decimal(18,3)");
        b.Property(e => e.ReorderLevel).HasColumnType("decimal(18,3)");

        b.HasIndex(e => e.Code).IsUnique();
        b.HasIndex(e => e.Barcode).HasFilter("[Barcode] IS NOT NULL AND [IsDeleted] = 0");
        b.HasIndex(e => e.NameAr).HasFilter("[IsDeleted] = 0");

        b.HasOne(e => e.Category).WithMany()
            .HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(e => e.BaseUnit).WithMany()
            .HasForeignKey(e => e.BaseUnitId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(e => e.SaleUnit).WithMany()
            .HasForeignKey(e => e.SaleUnitId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(e => e.PurchaseUnit).WithMany()
            .HasForeignKey(e => e.PurchaseUnitId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(e => e.DefaultSupplier).WithMany()
            .HasForeignKey(e => e.DefaultSupplierId).OnDelete(DeleteBehavior.SetNull);

        b.HasMany(e => e.Units).WithOne(e => e.Product)
            .HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(e => e.Images).WithOne(e => e.Product)
            .HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(e => e.Stock).WithOne(e => e.Product)
            .HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Cascade);
    }
}
