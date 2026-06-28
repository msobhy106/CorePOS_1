using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CorePOS.Domain.Entities;

namespace CorePOS.Persistence.Configurations;

public class PurchaseInvoiceConfiguration : IEntityTypeConfiguration<PurchaseInvoice>
{
    public void Configure(EntityTypeBuilder<PurchaseInvoice> b)
    {
        b.ToTable("PurchaseInvoices");
        b.HasKey(e => e.Id);
        b.Property(e => e.InvoiceNo).IsRequired().HasMaxLength(50);
        b.Property(e => e.SupplierInvoiceNo).HasMaxLength(100);
        b.HasIndex(e => e.InvoiceNo).IsUnique();
        b.HasIndex(e => new { e.SupplierId, e.InvoiceDate }).HasFilter("[IsDeleted] = 0");
        b.Property(e => e.Status).HasConversion<byte>();
        b.Property(e => e.PaymentMethod).HasConversion<byte>();
        b.Property(e => e.Subtotal).HasColumnType("decimal(18,4)");
        b.Property(e => e.DiscountPercent).HasColumnType("decimal(5,2)");
        b.Property(e => e.DiscountAmount).HasColumnType("decimal(18,4)");
        b.Property(e => e.TaxPercent).HasColumnType("decimal(5,2)");
        b.Property(e => e.TaxAmount).HasColumnType("decimal(18,4)");
        b.Property(e => e.TotalAmount).HasColumnType("decimal(18,4)");
        b.Property(e => e.PaidAmount).HasColumnType("decimal(18,4)");
        b.Property(e => e.RemainingAmount).HasColumnType("decimal(18,4)");
        b.HasOne(e => e.Supplier).WithMany()
            .HasForeignKey(e => e.SupplierId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(e => e.Branch).WithMany()
            .HasForeignKey(e => e.BranchId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(e => e.Warehouse).WithMany()
            .HasForeignKey(e => e.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(e => e.User).WithMany()
            .HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(e => e.Items).WithOne(e => e.Invoice)
            .HasForeignKey(e => e.InvoiceId).OnDelete(DeleteBehavior.Cascade);
    }
}
