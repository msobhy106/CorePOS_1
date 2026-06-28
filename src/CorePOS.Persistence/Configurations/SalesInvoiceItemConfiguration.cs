using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CorePOS.Domain.Entities;

namespace CorePOS.Persistence.Configurations;

public class SalesInvoiceItemConfiguration : IEntityTypeConfiguration<SalesInvoiceItem>
{
    public void Configure(EntityTypeBuilder<SalesInvoiceItem> b)
    {
        b.ToTable("SalesInvoiceItems");
        b.HasKey(e => e.Id);
        b.HasIndex(e => e.InvoiceId);
        b.HasIndex(e => new { e.ProductId, e.InvoiceId });
        b.Property(e => e.ProductNameAr).IsRequired().HasMaxLength(300).UseCollation("Arabic_CI_AS");
        b.Property(e => e.Barcode).HasMaxLength(100);
        b.Property(e => e.Quantity).HasColumnType("decimal(18,3)");
        b.Property(e => e.UnitPrice).HasColumnType("decimal(18,4)");
        b.Property(e => e.PurchasePrice).HasColumnType("decimal(18,4)");
        b.Property(e => e.DiscountPercent).HasColumnType("decimal(5,2)");
        b.Property(e => e.DiscountAmount).HasColumnType("decimal(18,4)");
        b.Property(e => e.TaxPercent).HasColumnType("decimal(5,2)");
        b.Property(e => e.TaxAmount).HasColumnType("decimal(18,4)");
        b.Property(e => e.TotalPrice).HasColumnType("decimal(18,4)");
        b.Property(e => e.ReturnedQty).HasColumnType("decimal(18,3)");
        b.HasOne(e => e.Product).WithMany()
            .HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(e => e.Unit).WithMany()
            .HasForeignKey(e => e.UnitId).OnDelete(DeleteBehavior.Restrict);
    }
}
