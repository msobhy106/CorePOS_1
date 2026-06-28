using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CorePOS.Domain.Entities;

namespace CorePOS.Persistence.Configurations;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> b)
    {
        b.ToTable("Branches");
        b.HasKey(e => e.Id);
        b.Property(e => e.Code).IsRequired().HasMaxLength(20).UseCollation("Arabic_CI_AS");
        b.Property(e => e.Name).IsRequired().HasMaxLength(200).UseCollation("Arabic_CI_AS");
        b.Property(e => e.NameAr).IsRequired().HasMaxLength(200).UseCollation("Arabic_CI_AS");
        b.Property(e => e.Address).HasMaxLength(500);
        b.Property(e => e.Phone).HasMaxLength(50);
        b.Property(e => e.ManagerName).HasMaxLength(200).UseCollation("Arabic_CI_AS");
        b.HasIndex(e => e.Code).IsUnique();
    }
}
