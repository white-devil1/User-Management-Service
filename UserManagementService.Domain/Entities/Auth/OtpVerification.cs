using UserManagementService.Domain.Entities.Identity;

namespace UserManagementService.Domain.Entities.Auth;

public class OtpVerification
{
    // ✅ Primary Key
    public Guid Id { get; set; } = Guid.NewGuid();

    // ✅ User Reference (nullable for forgot password before login)
    public string? UserId { get; set; }

    // ✅ Contact Information
    public string Email { get; set; } = default!;
    public string? PhoneNumber { get; set; }

    // ✅ OTP Details
    public string OTP { get; set; } = default!;  // 6-digit code
    public string Purpose { get; set; } = default!;  // ForgotPassword, AdminReset, EmailVerification, etc.

    // ✅ OTP Status
    public bool IsUsed { get; set; } = false;
    public DateTime? UsedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ✅ Security & Audit
    public string? IPAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime? DeletedAt { get; set; }

    // ✅ Navigation Property (optional - for lookup)
    public virtual ApplicationUser? User { get; set; }
}