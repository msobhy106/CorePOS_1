using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CorePOS.Domain.Entities;

namespace CorePOS.Persistence.Configurations;

public class SettingConfiguration : IEntityTypeConfiguration<Setting>
{
    public void Configure(EntityTypeBuilder<Setting> b)
    {
        b.ToTable("Settings");
        b.HasKey(e => e.Id);
        b.Property(e => e.SettingKey).IsRequired().HasMaxLength(200);
        b.Property(e => e.SettingGroup).HasMaxLength(100);
        b.Property(e => e.DataType).HasMaxLength(50);
        b.Property(e => e.Description).HasMaxLength(500);
        b.HasIndex(e => e.SettingKey).IsUnique();
    }
}
