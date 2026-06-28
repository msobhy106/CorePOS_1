using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CorePOS.Domain.Entities;
namespace CorePOS.Persistence.Configurations;
public class CustomerGroupConfiguration : IEntityTypeConfiguration<CustomerGroup>
{
    public void Configure(EntityTypeBuilder<CustomerGroup> b)
    {
        b.ToTable("CustomerGroups");
        b.HasKey(e => e.Id);
        b.Property(e => e.Name).IsRequired().HasMaxLength(100).UseCollation("Arabic_CI_AS");
        b.Property(e => e.DiscountPercent).HasPrecision(5, 2);
        b.Property(e => e.PointsMultiplier).HasPrecision(5, 2).HasDefaultValue(1m);
    }
}
