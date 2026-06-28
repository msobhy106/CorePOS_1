using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CorePOS.Domain.Entities;
namespace CorePOS.Persistence.Configurations;
public class WarehouseTransferItemConfiguration : IEntityTypeConfiguration<WarehouseTransferItem>
{
    public void Configure(EntityTypeBuilder<WarehouseTransferItem> b)
    {
        b.ToTable("WarehouseTransferItems");
        b.HasKey(e => e.Id);
        b.Property(e => e.ProductNameAr).IsRequired().HasMaxLength(300).UseCollation("Arabic_CI_AS");
        b.Property(e => e.Quantity).HasPrecision(18, 4);
        b.Property(e => e.UnitCost).HasPrecision(18, 4);
        b.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(e => e.TransferId);
    }
}
