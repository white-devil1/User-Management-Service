using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using UserManagementService.Domain.Common;
using UserManagementService.Domain.Entities.Auth;
using UserManagementService.Domain.Entities.Identity;
using UserManagementService.Domain.Entities.RBAC;
using UserManagementService.Domain.Entities.Subscriptions;

namespace UserManagementService.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string,
    IdentityUserClaim<string>, ApplicationUserRole, IdentityUserLogin<string>,
    IdentityRoleClaim<string>, IdentityUserToken<string>>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<App> Apps => Set<App>();
    public DbSet<Page> Pages => Set<Page>();
    public DbSet<AppAction> Actions => Set<AppAction>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<OtpVerification> OtpVerifications => Set<OtpVerification>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<SubscriptionApp> SubscriptionApps => Set<SubscriptionApp>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        ConfigureIdentityTables(modelBuilder);
        ConfigureRBACTables(modelBuilder);
        ConfigureAuthTables(modelBuilder);
    }

    private void ConfigureIdentityTables(ModelBuilder modelBuilder)
    {
        // ApplicationUser is configured in ApplicationUserConfiguration.cs
        // Do NOT add any ApplicationUser configuration here — it will conflict.

        // ApplicationRole is fully configured in ApplicationRoleConfiguration.cs
        // Do NOT add any ApplicationRole configuration here — it will conflict.

        // ApplicationUserRole - composite key only, no relationships
        modelBuilder.Entity<ApplicationUserRole>(entity =>
        {
            entity.ToTable("aspnetuserroles");
            entity.HasKey(e => new { e.UserId, e.RoleId });
        });

        // Identity built-in tables
        modelBuilder.Entity<IdentityUserClaim<string>>(e => { e.ToTable("aspnetuserclaims"); });
        modelBuilder.Entity<IdentityRoleClaim<string>>(e => { e.ToTable("aspnetroleclaims"); });
        modelBuilder.Entity<IdentityUserLogin<string>>(e => { e.ToTable("aspnetuserlogins"); });
        modelBuilder.Entity<IdentityUserToken<string>>(e => { e.ToTable("aspnetusertokens"); });
    }

    private void ConfigureRBACTables(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<App>(entity =>
        {
            entity.ToTable("apps");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
            entity.Property(e => e.DisplayOrder).IsRequired().HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).IsRequired().HasDefaultValueSql("NOW()");
            entity.HasIndex(e => e.Code).IsUnique().HasDatabaseName("ix_apps_code");
            entity.HasMany(e => e.Pages).WithOne(e => e.App).HasForeignKey(e => e.AppId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Permissions).WithOne(e => e.App).HasForeignKey(e => e.AppId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Page>(entity =>
        {
            entity.ToTable("pages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
            entity.Property(e => e.DisplayOrder).IsRequired().HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).IsRequired().HasDefaultValueSql("NOW()");
            entity.HasIndex(e => new { e.AppId, e.Code }).IsUnique().HasDatabaseName("ix_pages_appid_code");
            entity.HasMany(e => e.Actions).WithOne(e => e.Page).HasForeignKey(e => e.PageId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Permissions).WithOne(e => e.Page).HasForeignKey(e => e.PageId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AppAction>(entity =>
        {
            entity.ToTable("appactions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(20).HasDefaultValue("Button");
            entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).IsRequired().HasDefaultValueSql("NOW()");
            entity.HasIndex(e => new { e.PageId, e.Code }).IsUnique().HasDatabaseName("ix_appactions_pageid_code");
            entity.HasMany(e => e.Permissions).WithOne(e => e.Action).HasForeignKey(e => e.ActionId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("permissions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PermissionCode).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(500);
            entity.Property(e => e.IsEnabled).IsRequired().HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).IsRequired().HasDefaultValueSql("NOW()");
            entity.HasIndex(e => new { e.AppId, e.PageId, e.ActionId }).IsUnique().HasDatabaseName("ix_permissions_appid_pageid_actionid");
            entity.HasIndex(e => e.PermissionCode).IsUnique().HasDatabaseName("ix_permissions_permissioncode");
            entity.HasMany(e => e.RolePermissions).WithOne(e => e.Permission).HasForeignKey(e => e.PermissionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("rolepermissions");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.RoleId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.PermissionId).IsRequired();
            entity.Property(e => e.AssignedAt).IsRequired().HasDefaultValueSql("NOW()");
            entity.Property(e => e.AssignedBy).HasMaxLength(450);

            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).IsRequired().HasDefaultValueSql("NOW()");
            entity.Property(e => e.CreatedBy).HasMaxLength(450);
            entity.Property(e => e.UpdatedBy).HasMaxLength(450);
            entity.Property(e => e.DeletedBy).HasMaxLength(450);
            entity.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);

            entity.HasIndex(e => new { e.RoleId, e.PermissionId })
                .IsUnique()
                .HasDatabaseName("ix_rolepermissions_roleid_permissionid");
            entity.HasIndex(e => e.RoleId).HasDatabaseName("ix_rolepermissions_roleid");
            entity.HasIndex(e => e.PermissionId).HasDatabaseName("ix_rolepermissions_permissionid");
            entity.HasIndex(e => e.IsDeleted).HasDatabaseName("ix_rolepermissions_isdeleted");

            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.HasOne(e => e.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureAuthTables(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refreshtokens");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Token).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ExpiresAt).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("NOW()");
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.RevokedAt);
            entity.Property(e => e.ReplacedByTokenId);
            entity.HasIndex(e => e.Token).IsUnique().HasDatabaseName("ix_refreshtokens_token");
            entity.HasIndex(e => e.UserId).HasDatabaseName("ix_refreshtokens_userid");
            entity.HasIndex(e => e.ExpiresAt).HasDatabaseName("ix_refreshtokens_expiresat");
        });

        modelBuilder.Entity<OtpVerification>(entity =>
        {
            entity.ToTable("otpverifications");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.OTP).IsRequired().HasMaxLength(10);
            entity.Property(e => e.ExpiresAt).IsRequired();
            entity.Property(e => e.IsUsed).IsRequired().HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("NOW()");
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=UserManagementDb;Username=postgres;Password=Aneesh@123");
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}