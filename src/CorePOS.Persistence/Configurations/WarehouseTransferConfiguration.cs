using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CorePOS.Domain.Entities;

namespace CorePOS.Persistence.Configurations;

public class WarehouseTransferConfiguration : IEntityTypeConfiguration<WarehouseTransfer>
{
    public void Configure(EntityTypeBuilder<WarehouseTransfer> b)
    {
        b.ToTable("WarehouseTransfers");
        b.HasKey(e => e.Id);
        b.Property(e => e.TransferNo).IsRequired().HasMaxLength(50);
        b.HasIndex(e => e.TransferNo).IsUnique();
        b.HasOne(e => e.FromWarehouse).WithMany()
            .HasForeignKey(e => e.FromWarehouseId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(e => e.ToWarehouse).WithMany()
            .HasForeignKey(e => e.ToWarehouseId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(e => e.Items).WithOne()
            .HasForeignKey(i => i.TransferId).OnDelete(DeleteBehavior.Cascade);
    }
}
