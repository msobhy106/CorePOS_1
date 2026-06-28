using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CorePOS.Domain.Entities;
using CorePOS.Domain.Enums;

namespace CorePOS.Persistence.Configurations;

public class SalesInvoiceConfiguration : IEntityTypeConfiguration<SalesInvoice>
{
    public void Configure(EntityTypeBuilder<SalesInvoice> b)
    {
        b.ToTable("SalesInvoices");
        b.HasKey(e => e.Id);
        b.Property(e => e.InvoiceNo).IsRequired().HasMaxLength(50);
        b.HasIndex(e => e.InvoiceNo).IsUnique();
        b.HasIndex(e => new { e.InvoiceDate, e.BranchId }).HasFilter("[IsDeleted] = 0");
        b.HasIndex(e => e.CustomerId).HasFilter("[CustomerId] IS NOT NULL AND [IsDeleted] = 0");
        b.HasIndex(e => e.ShiftId).HasFilter("[ShiftId] IS NOT NULL");

        b.Property(e => e.InvoiceType).HasConversion<byte>();
        b.Property(e => e.PaymentMethod).HasConversion<byte>();
        b.Property(e => e.Status).HasConversion<byte>();

        b.Property(e => e.Subtotal).HasColumnType("decimal(18,4)");
        b.Property(e => e.DiscountPercent).HasColumnType("decimal(5,2)");
        b.Property(e => e.DiscountAmount).HasColumnType("decimal(18,4)");
        b.Property(e => e.TaxPercent).HasColumnType("decimal(5,2)");
        b.Property(e => e.TaxAmount).HasColumnType("decimal(18,4)");
        b.Property(e => e.DeliveryCost).HasColumnType("decimal(18,4)");
        b.Property(e => e.TotalAmount).HasColumnType("decimal(18,4)");
        b.Property(e => e.PaidAmount).HasColumnType("decimal(18,4)");
        b.Property(e => e.VisaAmount).HasColumnType("decimal(18,4)");
        b.Property(e => e.BankTransferAmount).HasColumnType("decimal(18,4)");
        b.Property(e => e.EWalletAmount).HasColumnType("decimal(18,4)");
        b.Property(e => e.RemainingAmount).HasColumnType("decimal(18,4)");

        b.HasOne(e => e.Customer).WithMany()
            .HasForeignKey(e => e.CustomerId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(e => e.Branch).WithMany()
            .HasForeignKey(e => e.BranchId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(e => e.Warehouse).WithMany()
            .HasForeignKey(e => e.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(e => e.User).WithMany()
            .HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(e => e.Shift).WithMany()
            .HasForeignKey(e => e.ShiftId).OnDelete(DeleteBehavior.SetNull);

        b.HasMany(e => e.Items).WithOne(e => e.Invoice)
            .HasForeignKey(e => e.InvoiceId).OnDelete(DeleteBehavior.Cascade);
    }
}
