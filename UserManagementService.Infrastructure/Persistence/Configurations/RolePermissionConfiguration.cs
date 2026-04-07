using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagementService.Domain.Entities.RBAC;

namespace UserManagementService.Infrastructure.Persistence.Configurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        // Table Name
        builder.ToTable("rolepermissions");

        // Primary Key
        builder.HasKey(rp => rp.Id);

        // Property Configurations
        builder.Property(rp => rp.RoleId)
            .IsRequired()
            .HasMaxLength(450);  // Matches ASP.NET Identity Role Id length

        builder.Property(rp => rp.PermissionId)
            .IsRequired();

        builder.Property(rp => rp.AssignedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(rp => rp.AssignedBy)
            .HasMaxLength(450);  // Matches ASP.NET Identity UserId length

        // Audit Fields (from BaseEntity)
        builder.Property(rp => rp.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(rp => rp.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(rp => rp.CreatedBy)
            .HasMaxLength(450);

        builder.Property(rp => rp.UpdatedBy)
            .HasMaxLength(450);

        builder.Property(rp => rp.DeletedBy)
            .HasMaxLength(450);

        // Soft Delete
        builder.Property(rp => rp.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(rp => new { rp.RoleId, rp.PermissionId })
            .IsUnique()
            .HasDatabaseName("ix_rolepermissions_roleid_permissionid");

        builder.HasIndex(rp => rp.RoleId)
            .HasDatabaseName("ix_rolepermissions_roleid");

        builder.HasIndex(rp => rp.PermissionId)
            .HasDatabaseName("ix_rolepermissions_permissionid");

        builder.HasIndex(rp => rp.IsDeleted)
            .HasDatabaseName("ix_rolepermissions_isdeleted");

        // Global Query Filter (exclude soft-deleted by default)
        builder.HasQueryFilter(rp => !rp.IsDeleted);

        // Relationships
        builder.HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}