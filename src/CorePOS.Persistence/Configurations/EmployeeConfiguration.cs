using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CorePOS.Domain.Entities;

namespace CorePOS.Persistence.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> b)
    {
        b.ToTable("Employees");
        b.HasKey(e => e.Id);
        b.Property(e => e.Code).IsRequired().HasMaxLength(50).UseCollation("Arabic_CI_AS");
        b.Property(e => e.Name).IsRequired().HasMaxLength(300).UseCollation("Arabic_CI_AS");
        b.Property(e => e.JobTitle).HasMaxLength(200).UseCollation("Arabic_CI_AS");
        b.Property(e => e.Phone).HasMaxLength(50);
        b.Property(e => e.Salary).HasColumnType("decimal(18,4)");
        b.HasIndex(e => e.Code).IsUnique();
        b.HasMany(e => e.Transactions).WithOne(t => t.Employee)
            .HasForeignKey(t => t.EmployeeId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(e => e.Branch).WithMany()
            .HasForeignKey(e => e.BranchId).OnDelete(DeleteBehavior.SetNull);
    }
}
