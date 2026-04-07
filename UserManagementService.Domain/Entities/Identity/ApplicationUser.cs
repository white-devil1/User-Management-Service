using Microsoft.AspNetCore.Identity;
using UserManagementService.Domain.Entities.Auth;

namespace UserManagementService.Domain.Entities.Identity;

public class ApplicationUser : IdentityUser
{
    // Multi-Tenancy
    public Guid TenantId { get; set; }
    public Guid? BranchId { get; set; }

    // User Profile
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    // Admin Flags
    public bool IsSuperAdmin { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;

    // Password Management
    public bool IsTemporaryPassword { get; set; } = false;
    public bool MustChangePassword { get; set; } = false;
    public DateTime? TemporaryPasswordExpiresAt { get; set; }
    public DateTime? LastPasswordChangedAt { get; set; }
    public int PasswordChangedCount { get; set; } = 0;

    // Login Tracking
    public DateTime? LastLoginAt { get; set; }
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LastFailedLoginAt { get; set; }

    // Soft Delete Tracking
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // ✅ AUDIT FIELDS (Added for full tracking)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }  // ← NEW: Who created this user
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedBy { get; set; }  // ← NEW: Who updated this user

    // Navigation Properties
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}