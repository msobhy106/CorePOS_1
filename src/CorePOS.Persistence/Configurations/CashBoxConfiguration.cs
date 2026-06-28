using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CorePOS.Domain.Entities;

namespace CorePOS.Persistence.Configurations;

public class CashBoxConfiguration : IEntityTypeConfiguration<CashBox>
{
    public void Configure(EntityTypeBuilder<CashBox> b)
    {
        b.ToTable("CashBoxes");
        b.HasKey(e => e.Id);
        b.Property(e => e.Code).IsRequired().HasMaxLength(20).UseCollation("Arabic_CI_AS");
        b.Property(e => e.Name).IsRequired().HasMaxLength(200).UseCollation("Arabic_CI_AS");
        b.Property(e => e.NameAr).IsRequired().HasMaxLength(200).UseCollation("Arabic_CI_AS");
        b.Property(e => e.OpeningBalance).HasColumnType("decimal(18,4)");
        b.Property(e => e.CurrentBalance).HasColumnType("decimal(18,4)");
        b.HasIndex(e => e.Code).IsUnique();
        b.HasIndex(e => e.BranchId);
        b.HasOne(e => e.Branch).WithMany(br => br.CashBoxes)
            .HasForeignKey(e => e.BranchId).OnDelete(DeleteBehavior.Restrict);
    }
}
