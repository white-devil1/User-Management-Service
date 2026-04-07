using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagementService.Domain.Entities.Identity;

namespace UserManagementService.Infrastructure.Persistence.Configurations;

public class ApplicationRoleConfiguration
    : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.ToTable("aspnetroles");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.TenantId).IsRequired();
        builder.Property(r => r.BranchId);
        builder.Property(r => r.Scope).IsRequired();
        builder.Property(r => r.Description).HasMaxLength(500);
        builder.Property(r => r.IsDefault)
            .IsRequired().HasDefaultValue(false);
        builder.Property(r => r.IsDeleted)
            .IsRequired().HasDefaultValue(false);
        builder.Property(r => r.DeletedAt);
        builder.Property(r => r.DeletedBy).HasMaxLength(450);
        builder.Property(r => r.CreatedAt)
            .IsRequired().HasDefaultValueSql("NOW()");
        builder.Property(r => r.UpdatedAt)
            .IsRequired().HasDefaultValueSql("NOW()");
        builder.Property(r => r.CreatedBy).HasMaxLength(450);
        builder.Property(r => r.UpdatedBy).HasMaxLength(450);

        // Unique role name per tenant (ignore soft-deleted roles)
        // Two different tenants CAN have a role with the same name — that is correct.
        // The same tenant CANNOT have two active roles with the same name.
        builder.HasIndex(r => new { r.TenantId, r.Name })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false")
            .HasDatabaseName("ix_aspnetroles_tenantid_name_unique");

        builder.HasIndex(r => r.TenantId)
            .HasDatabaseName("ix_aspnetroles_tenantid");
        builder.HasIndex(r => r.IsDeleted)
            .HasDatabaseName("ix_aspnetroles_isdeleted");
        builder.HasIndex(r => r.Scope)
            .HasDatabaseName("ix_aspnetroles_scope");

        builder.HasMany(r => r.RolePermissions)
            .WithOne(rp => rp.Role)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
