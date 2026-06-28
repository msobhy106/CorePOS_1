using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CorePOS.Domain.Entities;

namespace CorePOS.Persistence.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> b)
    {
        b.ToTable("Suppliers");
        b.HasKey(e => e.Id);
        b.Property(e => e.Code).IsRequired().HasMaxLength(50).UseCollation("Arabic_CI_AS");
        b.Property(e => e.Name).IsRequired().HasMaxLength(300).UseCollation("Arabic_CI_AS");
        b.Property(e => e.Phone).HasMaxLength(50);
        b.Property(e => e.Phone2).HasMaxLength(50);
        b.Property(e => e.Email).HasMaxLength(200);
        b.Property(e => e.TaxNumber).HasMaxLength(100);
        b.Property(e => e.ContactPerson).HasMaxLength(200).UseCollation("Arabic_CI_AS");
        b.Property(e => e.OpeningBalance).HasColumnType("decimal(18,4)");
        b.Property(e => e.CurrentBalance).HasColumnType("decimal(18,4)");
        b.Property(e => e.CreditLimit).HasColumnType("decimal(18,4)");
        b.HasIndex(e => e.Code).IsUnique();
        b.HasIndex(e => e.Name).HasFilter("[IsDeleted] = 0");
    }
}
