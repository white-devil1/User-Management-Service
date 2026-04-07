using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserManagementService.Domain.Entities.Auth;

namespace UserManagementService.Infrastructure.Persistence.Configurations;

public class OtpVerificationConfiguration : IEntityTypeConfiguration<OtpVerification>
{
    public void Configure(EntityTypeBuilder<OtpVerification> builder)
    {
        builder.ToTable("otpverifications");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.UserId).HasMaxLength(450);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(255);
        builder.Property(e => e.PhoneNumber).HasMaxLength(20);
        builder.Property(e => e.OTP).IsRequired().HasMaxLength(10);
        builder.Property(e => e.Purpose).IsRequired().HasMaxLength(50);
        builder.Property(e => e.IsUsed).IsRequired().HasDefaultValue(false);
        builder.Property(e => e.ExpiresAt).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("NOW()");
        builder.Property(e => e.IPAddress).HasMaxLength(50);
        builder.Property(e => e.UserAgent).HasMaxLength(500);
        builder.Property(e => e.DeletedBy).HasMaxLength(450);

        builder.HasIndex(e => new { e.Email, e.Purpose }).HasDatabaseName("ix_otpverifications_email_purpose");
        builder.HasIndex(e => e.ExpiresAt).HasDatabaseName("ix_otpverifications_expiresat");
        builder.HasIndex(e => e.IsUsed).HasDatabaseName("ix_otpverifications_isused");

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}