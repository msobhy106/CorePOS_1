using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CorePOS.Domain.Entities;

namespace CorePOS.Persistence.Configurations;

public class LicenseConfiguration : IEntityTypeConfiguration<License>
{
    public void Configure(EntityTypeBuilder<License> b)
    {
        b.ToTable("Licenses");
        b.HasKey(e => e.Id);
        b.Property(e => e.LicenseKey).IsRequired().HasMaxLength(100);
        b.Property(e => e.ActivationCode).HasMaxLength(200);
        b.Property(e => e.MachineId).HasMaxLength(200);
        b.Property(e => e.LicenseType).HasConversion<byte>();
        b.HasIndex(e => e.LicenseKey).IsUnique();
        b.HasIndex(e => e.ExpiryDate);
    }
}
