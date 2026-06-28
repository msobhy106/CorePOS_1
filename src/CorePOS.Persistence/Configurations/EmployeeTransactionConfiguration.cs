using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CorePOS.Domain.Entities;
namespace CorePOS.Persistence.Configurations;
public class EmployeeTransactionConfiguration : IEntityTypeConfiguration<EmployeeTransaction>
{
    public void Configure(EntityTypeBuilder<EmployeeTransaction> b)
    {
        b.ToTable("EmployeeTransactions");
        b.HasKey(e => e.Id);
        b.Property(e => e.Amount).HasPrecision(18, 2);
        b.Property(e => e.Type).HasConversion<byte>();
        b.Property(e => e.Notes).HasMaxLength(500);
        b.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(e => new { e.EmployeeId, e.TransactionDate });
    }
}
