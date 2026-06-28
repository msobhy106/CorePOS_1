using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CorePOS.Domain.Entities;

namespace CorePOS.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("Users");
        b.HasKey(e => e.Id);
        b.Property(e => e.Username).IsRequired().HasMaxLength(100);
        b.Property(e => e.PasswordHash).IsRequired().HasMaxLength(256);
        b.Property(e => e.FullName).IsRequired().HasMaxLength(200).UseCollation("Arabic_CI_AS");
        b.Property(e => e.FullNameAr).HasMaxLength(200).UseCollation("Arabic_CI_AS");
        b.Property(e => e.Email).HasMaxLength(200);
        b.Property(e => e.Phone).HasMaxLength(50);
        b.Property(e => e.PhotoPath).HasMaxLength(500);
        b.HasIndex(e => e.Username).IsUnique().HasFilter("[IsDeleted] = 0");
        b.HasOne(e => e.Role).WithMany()
            .HasForeignKey(e => e.RoleId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(e => e.Branch).WithMany()
            .HasForeignKey(e => e.BranchId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(e => e.Warehouse).WithMany()
            .HasForeignKey(e => e.WarehouseId).OnDelete(DeleteBehavior.SetNull);
    }
}
