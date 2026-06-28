using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CorePOS.Domain.Entities;

namespace CorePOS.Persistence.Configurations;

public class SalesReturnConfiguration : IEntityTypeConfiguration<SalesReturn>
{
    public void Configure(EntityTypeBuilder<SalesReturn> b)
    {
        b.ToTable("SalesReturns");
        b.HasKey(e => e.Id);
        b.Property(e => e.ReturnNo).IsRequired().HasMaxLength(50);
        b.Property(e => e.TotalAmount).HasColumnType("decimal(18,4)");
        b.Property(e => e.ReturnType).HasConversion<byte>();
        b.Property(e => e.RefundMethod).HasConversion<byte>();
        b.HasIndex(e => e.ReturnNo).IsUnique();
        b.HasIndex(e => e.OriginalInvoiceId);
        b.HasOne(e => e.OriginalInvoice).WithMany()
            .HasForeignKey(e => e.OriginalInvoiceId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(e => e.Customer).WithMany()
            .HasForeignKey(e => e.CustomerId).OnDelete(DeleteBehavior.SetNull);
        b.HasMany(e => e.Items).WithOne(i => i.Return)
            .HasForeignKey(i => i.ReturnId).OnDelete(DeleteBehavior.Cascade);
    }
}
