using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CorePOS.Domain.Entities;

namespace CorePOS.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> b)
    {
        b.ToTable("Categories");
        b.HasKey(e => e.Id);
        b.Property(e => e.Code).IsRequired().HasMaxLength(20).UseCollation("Arabic_CI_AS");
        b.Property(e => e.Name).IsRequired().HasMaxLength(200).UseCollation("Arabic_CI_AS");
        b.Property(e => e.NameAr).IsRequired().HasMaxLength(200).UseCollation("Arabic_CI_AS");
        b.Property(e => e.Level).HasConversion<byte>();
        b.HasIndex(e => e.Code).IsUnique();
        b.HasIndex(e => e.ParentId).HasFilter("[ParentId] IS NOT NULL");
        b.HasOne(e => e.Parent).WithMany(c => c.Children)
            .HasForeignKey(e => e.ParentId).OnDelete(DeleteBehavior.Restrict);
    }
}
