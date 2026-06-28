using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CorePOS.Domain.Entities;

namespace CorePOS.Persistence.Configurations;

public class CustomerPaymentConfiguration : IEntityTypeConfiguration<CustomerPayment>
{
    public void Configure(EntityTypeBuilder<CustomerPayment> b)
    {
        b.ToTable("CustomerPayments");
        b.HasKey(e => e.Id);
        b.Property(e => e.PaymentNo).IsRequired().HasMaxLength(50);
        b.Property(e => e.Amount).HasColumnType("decimal(18,4)");
        b.Property(e => e.PaymentMethod).HasConversion<byte>();
        b.HasIndex(e => e.PaymentNo).IsUnique();
        b.HasIndex(e => new { e.CustomerId, e.PaymentDate });
        b.HasOne(e => e.Customer).WithMany()
            .HasForeignKey(e => e.CustomerId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(e => e.CashBox).WithMany()
            .HasForeignKey(e => e.CashBoxId).OnDelete(DeleteBehavior.SetNull);
    }
}
