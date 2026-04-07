using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagementService.Domain.Entities.RBAC;

namespace UserManagementService.Infrastructure.Persistence.Configurations;

public class AppConfiguration : IEntityTypeConfiguration<App>
{
    public void Configure(EntityTypeBuilder<App> builder)
    {
        // ✅ Table Name
        builder.ToTable("apps");

        // ✅ Primary Key
        builder.HasKey(e => e.Id);

        // ✅ Property Configurations
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.Icon)
            .HasMaxLength(200);

        builder.Property(e => e.DisplayOrder)
            .IsRequired();

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // ✅ Soft Delete Fields
        builder.Property(e => e.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.DeletedAt);

        builder.Property(e => e.DeletedBy)
            .HasMaxLength(450);  // Matches ASP.NET Identity UserId length

        // ✅ Audit Fields (from BaseEntity)
        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(e => e.UpdatedAt);

        // ✅ Indexes for Performance
        builder.HasIndex(e => e.Code)
            .IsUnique()
            .HasDatabaseName("ix_apps_code");

        builder.HasIndex(e => e.DisplayOrder)
            .HasDatabaseName("ix_apps_displayorder");

        builder.HasIndex(e => e.IsActive)
            .HasDatabaseName("ix_apps_isactive");

        builder.HasIndex(e => e.IsDeleted)
            .HasDatabaseName("ix_apps_isdeleted");

        // ✅ Global Query Filter (exclude soft-deleted by default)
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}