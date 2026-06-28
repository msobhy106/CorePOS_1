using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CorePOS.Domain.Entities;

namespace CorePOS.Persistence.Configurations;

public class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> b)
    {
        b.ToTable("Units");
        b.HasKey(e => e.Id);
        b.Property(e => e.Code).IsRequired().HasMaxLength(20).UseCollation("Arabic_CI_AS");
        b.Property(e => e.Name).IsRequired().HasMaxLength(100).UseCollation("Arabic_CI_AS");
        b.Property(e => e.NameAr).IsRequired().HasMaxLength(100).UseCollation("Arabic_CI_AS");
        b.Property(e => e.Abbreviation).HasMaxLength(20);
        b.HasIndex(e => e.Code).IsUnique();
    }
}
