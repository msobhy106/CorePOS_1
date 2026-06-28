using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CorePOS.Domain.Entities;

namespace CorePOS.Persistence.Configurations;

public class ShiftConfiguration : IEntityTypeConfiguration<Shift>
{
    public void Configure(EntityTypeBuilder<Shift> b)
    {
        b.ToTable("Shifts");
        b.HasKey(e => e.Id);
        b.Property(e => e.ShiftNo).IsRequired().HasMaxLength(50);
        b.HasIndex(e => e.ShiftNo).IsUnique();
        b.HasIndex(e => new { e.Status, e.UserId });
        b.Property(e => e.OpeningBalance).HasColumnType("decimal(18,4)");
        b.Property(e => e.ClosingBalance).HasColumnType("decimal(18,4)");
        b.Property(e => e.ActualBalance).HasColumnType("decimal(18,4)");
        b.Property(e => e.Status).HasConversion<byte>();
        b.HasOne(e => e.User).WithMany()
            .HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(e => e.Branch).WithMany()
            .HasForeignKey(e => e.BranchId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(e => e.CashBox).WithMany()
            .HasForeignKey(e => e.CashBoxId).OnDelete(DeleteBehavior.Restrict);
    }
}
