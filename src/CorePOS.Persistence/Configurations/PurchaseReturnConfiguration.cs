using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CorePOS.Domain.Entities;

namespace CorePOS.Persistence.Configurations;

public class PurchaseReturnConfiguration : IEntityTypeConfiguration<PurchaseReturn>
{
    public void Configure(EntityTypeBuilder<PurchaseReturn> b)
    {
        b.ToTable("PurchaseReturns");
        b.HasKey(e => e.Id);
        b.Property(e => e.ReturnNo).IsRequired().HasMaxLength(50);
        b.Property(e => e.TotalAmount).HasColumnType("decimal(18,4)");
        b.Property(e => e.ReturnType).HasConversion<byte>();
        b.HasIndex(e => e.ReturnNo).IsUnique();
        b.HasOne(e => e.OriginalInvoice).WithMany()
            .HasForeignKey(e => e.OriginalInvoiceId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(e => e.Supplier).WithMany()
            .HasForeignKey(e => e.SupplierId).OnDelete(DeleteBehavior.SetNull);
        b.HasMany(e => e.Items).WithOne()
            .HasForeignKey(i => i.ReturnId).OnDelete(DeleteBehavior.Cascade);
    }
}
