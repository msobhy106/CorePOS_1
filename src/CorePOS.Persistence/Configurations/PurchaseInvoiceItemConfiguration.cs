using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CorePOS.Domain.Entities;
namespace CorePOS.Persistence.Configurations;
public class PurchaseInvoiceItemConfiguration : IEntityTypeConfiguration<PurchaseInvoiceItem>
{
    public void Configure(EntityTypeBuilder<PurchaseInvoiceItem> b)
    {
        b.ToTable("PurchaseInvoiceItems");
        b.HasKey(e => e.Id);
        b.Property(e => e.ProductNameAr).IsRequired().HasMaxLength(300).UseCollation("Arabic_CI_AS");
        b.Property(e => e.Quantity).HasPrecision(18, 4);
        b.Property(e => e.UnitCost).HasPrecision(18, 4);
        b.Property(e => e.DiscountPercent).HasPrecision(5, 2);
        b.Property(e => e.DiscountAmount).HasPrecision(18, 2);
        b.Property(e => e.TaxPercent).HasPrecision(5, 2);
        b.Property(e => e.TaxAmount).HasPrecision(18, 2);
        b.Property(e => e.TotalCost).HasPrecision(18, 2);
        b.Property(e => e.SalePriceAfter).HasPrecision(18, 4);
        b.Property(e => e.ReturnedQty).HasPrecision(18, 4);
        b.HasIndex(e => e.InvoiceId);
    }
}
