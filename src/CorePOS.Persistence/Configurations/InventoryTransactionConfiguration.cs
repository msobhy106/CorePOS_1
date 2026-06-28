using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CorePOS.Domain.Entities;

namespace CorePOS.Persistence.Configurations;

public class InventoryTransactionConfiguration : IEntityTypeConfiguration<InventoryTransaction>
{
    public void Configure(EntityTypeBuilder<InventoryTransaction> b)
    {
        b.ToTable("InventoryTransactions");
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).ValueGeneratedOnAdd();
        b.Property(e => e.Quantity).HasColumnType("decimal(18,3)");
        b.Property(e => e.UnitCost).HasColumnType("decimal(18,4)");
        b.Property(e => e.TotalCost).HasColumnType("decimal(18,4)");
        b.Property(e => e.BalanceAfter).HasColumnType("decimal(18,3)");
        b.Property(e => e.ReferenceType).HasMaxLength(50);
        b.Property(e => e.Notes).HasMaxLength(500);
        b.Property(e => e.TransactionType).HasConversion<byte>();
        b.Property(e => e.Direction).HasConversion<byte>();
        b.HasIndex(e => new { e.ProductId, e.WarehouseId, e.TransactionDate });
        b.HasIndex(e => e.TransactionDate);
        b.HasOne(e => e.Product).WithMany()
            .HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(e => e.Warehouse).WithMany()
            .HasForeignKey(e => e.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(e => e.User).WithMany()
            .HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.SetNull);
    }
}
