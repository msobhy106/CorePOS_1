using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CorePOS.Domain.Entities;
namespace CorePOS.Persistence.Configurations;
public class PurchaseReturnItemConfiguration : IEntityTypeConfiguration<PurchaseReturnItem>
{
    public void Configure(EntityTypeBuilder<PurchaseReturnItem> b)
    {
        b.ToTable("PurchaseReturnItems");
        b.HasKey(e => e.Id);
        b.Property(e => e.ProductNameAr).IsRequired().HasMaxLength(300).UseCollation("Arabic_CI_AS");
        b.Property(e => e.Quantity).HasPrecision(18, 4);
        b.Property(e => e.UnitCost).HasPrecision(18, 4);
        b.Property(e => e.TotalCost).HasPrecision(18, 2);
        b.HasIndex(e => e.ReturnId);
    }
}
