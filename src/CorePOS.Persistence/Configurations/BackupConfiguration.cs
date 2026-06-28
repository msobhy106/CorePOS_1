using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CorePOS.Domain.Entities;

namespace CorePOS.Persistence.Configurations;

public class BackupConfiguration : IEntityTypeConfiguration<Backup>
{
    public void Configure(EntityTypeBuilder<Backup> b)
    {
        b.ToTable("Backups");
        b.HasKey(e => e.Id);
        b.Property(e => e.FileName).IsRequired().HasMaxLength(500);
        b.Property(e => e.FilePath).IsRequired().HasMaxLength(1000);
        b.Property(e => e.BackupType).HasConversion<byte>();
        b.Property(e => e.ErrorMessage).HasColumnType("nvarchar(max)");
        b.HasIndex(e => e.CreatedAt);
        b.HasOne(e => e.User).WithMany()
            .HasForeignKey(e => e.CreatedBy).OnDelete(DeleteBehavior.SetNull);
    }
}
