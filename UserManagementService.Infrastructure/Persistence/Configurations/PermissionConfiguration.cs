using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagementService.Domain.Entities.RBAC;

namespace UserManagementService.Infrastructure.Persistence.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        // Table Name
        builder.ToTable("permissions");

        // Primary Key
        builder.HasKey(p => p.Id);

        // Property Configurations
        builder.Property(p => p.PermissionCode)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.IsEnabled)
            .IsRequired()
            .HasDefaultValue(false);  // ⚠️ Default: INACTIVE (must be explicitly enabled)

        // Audit Fields (from BaseEntity)
        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(p => p.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(p => p.CreatedBy)
            .HasMaxLength(450);

        builder.Property(p => p.UpdatedBy)
            .HasMaxLength(450);

        builder.Property(p => p.DeletedBy)
            .HasMaxLength(450);

        // Soft Delete
        builder.Property(p => p.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(p => p.PermissionCode)
            .IsUnique()
            .HasDatabaseName("ix_permissions_permissioncode");

        builder.HasIndex(p => new { p.AppId, p.PageId, p.ActionId })
            .IsUnique()
            .HasDatabaseName("ix_permissions_appid_pageid_actionid");

        builder.HasIndex(p => p.AppId)
            .HasDatabaseName("ix_permissions_appid");

        builder.HasIndex(p => p.PageId)
            .HasDatabaseName("ix_permissions_pageid");

        builder.HasIndex(p => p.ActionId)
            .HasDatabaseName("ix_permissions_actionid");

        builder.HasIndex(p => p.IsEnabled)
            .HasDatabaseName("ix_permissions_isenabled");

        builder.HasIndex(p => p.IsDeleted)
            .HasDatabaseName("ix_permissions_isdeleted");

        // Global Query Filter (exclude soft-deleted by default)
        builder.HasQueryFilter(p => !p.IsDeleted);

        // Relationships
        builder.HasOne(p => p.App)
            .WithMany()
            .HasForeignKey(p => p.AppId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Page)
            .WithMany(pg => pg.Permissions)
            .HasForeignKey(p => p.PageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Action)
            .WithMany(a => a.Permissions)
            .HasForeignKey(p => p.ActionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.RolePermissions)
            .WithOne(rp => rp.Permission)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}