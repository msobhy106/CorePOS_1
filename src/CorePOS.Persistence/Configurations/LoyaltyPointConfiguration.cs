using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CorePOS.Domain.Entities;
namespace CorePOS.Persistence.Configurations;
public class LoyaltyPointConfiguration : IEntityTypeConfiguration<LoyaltyPoint>
{
    public void Configure(EntityTypeBuilder<LoyaltyPoint> b)
    {
        b.ToTable("LoyaltyPoints");
        b.HasKey(e => e.Id);
        b.Property(e => e.Points).HasPrecision(18, 2);
        b.Property(e => e.TransactionType).HasConversion<byte>();
        b.Property(e => e.ReferenceType).HasMaxLength(50);
        b.Property(e => e.Notes).HasMaxLength(500);
        b.HasOne(e => e.Customer).WithMany().HasForeignKey(e => e.CustomerId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(e => e.CustomerId);
    }
}
