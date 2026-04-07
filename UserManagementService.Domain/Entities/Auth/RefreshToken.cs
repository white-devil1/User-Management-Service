using UserManagementService.Domain.Common;
using UserManagementService.Domain.Entities.Identity;

namespace UserManagementService.Domain.Entities.Auth;

public class RefreshToken : BaseEntity
{
    public string UserId { get; set; } = default!;
    public string Token { get; set; } = default!; // Hashed UUID
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public Guid? ReplacedByTokenId { get; set; }
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }

    // Navigation Properties
    public virtual ApplicationUser User { get; set; } = default!;
}