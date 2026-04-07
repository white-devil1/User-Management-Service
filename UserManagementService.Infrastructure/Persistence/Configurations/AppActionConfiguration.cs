using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagementService.Domain.Entities.RBAC;

namespace UserManagementService.Infrastructure.Persistence.Configurations;

public class AppActionConfiguration : IEntityTypeConfiguration<AppAction>
{
    public void Configure(EntityTypeBuilder<AppAction> builder)
    {
        // Table Name
        builder.ToTable("appactions");

        // Primary Key
        builder.HasKey(a => a.Id);

        // Property Configurations
        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.Type)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("Button");

        builder.Property(a => a.Description)
            .HasMaxLength(500);

        builder.Property(a => a.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(a => a.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Audit Fields (from BaseEntity)
        builder.Property(a => a.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(a => a.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(a => a.CreatedBy)
            .HasMaxLength(450);

        builder.Property(a => a.UpdatedBy)
            .HasMaxLength(450);

        builder.Property(a => a.DeletedBy)
            .HasMaxLength(450);

        // Soft Delete
        builder.Property(a => a.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(a => new { a.PageId, a.Code })
            .IsUnique()
            .HasDatabaseName("ix_appactions_pageid_code");

        builder.HasIndex(a => a.PageId)
            .HasDatabaseName("ix_appactions_pageid");

        builder.HasIndex(a => a.IsActive)
            .HasDatabaseName("ix_appactions_isactive");

        builder.HasIndex(a => a.IsDeleted)
            .HasDatabaseName("ix_appactions_isdeleted");

        // Global Query Filter (exclude soft-deleted by default)
        builder.HasQueryFilter(a => !a.IsDeleted);

        // Relationships
        builder.HasOne(a => a.Page)
            .WithMany(p => p.Actions)
            .HasForeignKey(a => a.PageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Permissions)
            .WithOne(perm => perm.Action)
            .HasForeignKey(perm => perm.ActionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}