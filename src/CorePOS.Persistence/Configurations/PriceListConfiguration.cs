using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CorePOS.Domain.Entities;
namespace CorePOS.Persistence.Configurations;
public class PriceListConfiguration : IEntityTypeConfiguration<PriceList>
{
    public void Configure(EntityTypeBuilder<PriceList> b)
    {
        b.ToTable("PriceLists");
        b.HasKey(e => e.Id);
        b.Property(e => e.Name).IsRequired().HasMaxLength(200);
        b.Property(e => e.NameAr).IsRequired().HasMaxLength(200).UseCollation("Arabic_CI_AS");
        b.Property(e => e.DiscountPercent).HasPrecision(5, 2);
    }
}
