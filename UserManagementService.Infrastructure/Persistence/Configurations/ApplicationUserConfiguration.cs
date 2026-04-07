using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagementService.Domain.Entities.Identity;

namespace UserManagementService.Infrastructure.Persistence.Configurations;

public class ApplicationUserConfiguration
    : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("aspnetusers");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.BranchId);
        builder.Property(e => e.FirstName).HasMaxLength(100);
        builder.Property(e => e.LastName).HasMaxLength(100);
        builder.Property(e => e.IsSuperAdmin).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.DeletedAt);
        builder.Property(e => e.DeletedBy).HasMaxLength(450);
        builder.Property(e => e.IsTemporaryPassword).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.MustChangePassword).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.TemporaryPasswordExpiresAt);
        builder.Property(e => e.LastPasswordChangedAt);
        builder.Property(e => e.PasswordChangedCount).IsRequired().HasDefaultValue(0);
        builder.Property(e => e.LastLoginAt);
        builder.Property(e => e.FailedLoginAttempts).IsRequired().HasDefaultValue(0);
        builder.Property(e => e.LastFailedLoginAt);
        builder.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).IsRequired().HasDefaultValueSql("NOW()");
        builder.Property(e => e.CreatedBy).HasMaxLength(450);
        builder.Property(e => e.UpdatedBy).HasMaxLength(450);

        builder.HasIndex(e => e.Email).IsUnique()
            .HasDatabaseName("ix_aspnetusers_email");
        builder.HasIndex(e => e.TenantId)
            .HasDatabaseName("ix_aspnetusers_tenantid");
        builder.HasIndex(e => e.UserName).IsUnique()
            .HasDatabaseName("ix_aspnetusers_username");
        builder.HasIndex(e => e.IsDeleted)
            .HasDatabaseName("ix_aspnetusers_isdeleted");
        builder.HasIndex(e => new { e.TenantId, e.IsDeleted })
            .HasDatabaseName("ix_aspnetusers_tenantid_isdeleted");

        builder.HasMany(e => e.RefreshTokens)
            .WithOne(e => e.User)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}