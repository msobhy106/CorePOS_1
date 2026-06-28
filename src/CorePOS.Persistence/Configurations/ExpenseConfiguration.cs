using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CorePOS.Domain.Entities;

namespace CorePOS.Persistence.Configurations;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> b)
    {
        b.ToTable("Expenses");
        b.HasKey(e => e.Id);
        b.Property(e => e.ExpenseNo).IsRequired().HasMaxLength(50);
        b.Property(e => e.Amount).HasColumnType("decimal(18,4)");
        b.Property(e => e.Description).HasMaxLength(500);
        b.HasIndex(e => e.ExpenseNo).IsUnique();
        b.HasIndex(e => new { e.ExpenseDate, e.BranchId });
        b.HasOne(e => e.Category).WithMany()
            .HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(e => e.Branch).WithMany()
            .HasForeignKey(e => e.BranchId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(e => e.CashBox).WithMany()
            .HasForeignKey(e => e.CashBoxId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(e => e.User).WithMany()
            .HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}
