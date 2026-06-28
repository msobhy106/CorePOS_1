using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CorePOS.Domain.Entities;

namespace CorePOS.Persistence.Configurations;

public class SupplierPaymentConfiguration : IEntityTypeConfiguration<SupplierPayment>
{
    public void Configure(EntityTypeBuilder<SupplierPayment> b)
    {
        b.ToTable("SupplierPayments");
        b.HasKey(e => e.Id);
        b.Property(e => e.PaymentNo).IsRequired().HasMaxLength(50);
        b.Property(e => e.Amount).HasColumnType("decimal(18,4)");
        b.Property(e => e.PaymentMethod).HasConversion<byte>();
        b.HasIndex(e => e.PaymentNo).IsUnique();
        b.HasIndex(e => new { e.SupplierId, e.PaymentDate });
        b.HasOne(e => e.Supplier).WithMany()
            .HasForeignKey(e => e.SupplierId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(e => e.CashBox).WithMany()
            .HasForeignKey(e => e.CashBoxId).OnDelete(DeleteBehavior.SetNull);
    }
}
