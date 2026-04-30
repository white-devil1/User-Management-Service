using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagementService.Domain.Entities.Subscriptions;

namespace UserManagementService.Infrastructure.Persistence.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("subscriptions");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(e => e.Description)
            .HasColumnType("text");

        builder.Property(e => e.Price)
            .IsRequired()
            .HasColumnType("numeric(18,2)");

        builder.Property(e => e.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.DeletedAt);
        builder.Property(e => e.DeletedBy).HasMaxLength(450);

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(e => e.UpdatedAt);

        builder.HasIndex(e => e.Name).HasDatabaseName("ix_subscriptions_name");
        builder.HasIndex(e => e.IsActive).HasDatabaseName("ix_subscriptions_isactive");
        builder.HasIndex(e => e.IsDeleted).HasDatabaseName("ix_subscriptions_isdeleted");

        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.HasMany(e => e.SubscriptionApps)
            .WithOne(sa => sa.Subscription)
            .HasForeignKey(sa => sa.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SubscriptionAppConfiguration : IEntityTypeConfiguration<SubscriptionApp>
{
    public void Configure(EntityTypeBuilder<SubscriptionApp> builder)
    {
        builder.ToTable("subscription_apps");
        builder.HasKey(e => new { e.SubscriptionId, e.AppId });

        builder.HasOne(sa => sa.App)
            .WithMany()
            .HasForeignKey(sa => sa.AppId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.AppId).HasDatabaseName("ix_subscription_apps_appid");
    }
}
