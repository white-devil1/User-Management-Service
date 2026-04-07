using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagementService.Domain.Entities.RBAC;

namespace UserManagementService.Infrastructure.Persistence.Configurations;

public class PageConfiguration : IEntityTypeConfiguration<Page>
{
    public void Configure(EntityTypeBuilder<Page> builder)
    {
        // Table Name
        builder.ToTable("pages");

        // Primary Key
        builder.HasKey(p => p.Id);

        // Property Configurations
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Audit Fields (from BaseEntity)
        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(p => p.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(p => p.CreatedBy)
            .HasMaxLength(450);  // Matches ASP.NET Identity UserId length

        builder.Property(p => p.UpdatedBy)
            .HasMaxLength(450);

        builder.Property(p => p.DeletedBy)
            .HasMaxLength(450);

        // Soft Delete
        builder.Property(p => p.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(p => new { p.AppId, p.Code })
            .IsUnique()
            .HasDatabaseName("ix_pages_appid_code");

        builder.HasIndex(p => p.AppId)
            .HasDatabaseName("ix_pages_appid");

        builder.HasIndex(p => p.IsActive)
            .HasDatabaseName("ix_pages_isactive");

        builder.HasIndex(p => p.IsDeleted)
            .HasDatabaseName("ix_pages_isdeleted");

        // Global Query Filter (exclude soft-deleted by default)
        builder.HasQueryFilter(p => !p.IsDeleted);

        // Relationships
        builder.HasOne(p => p.App)
            .WithMany(a => a.Pages)
            .HasForeignKey(p => p.AppId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Actions)
            .WithOne(a => a.Page)
            .HasForeignKey(a => a.PageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Permissions)
            .WithOne(perm => perm.Page)
            .HasForeignKey(perm => perm.PageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}