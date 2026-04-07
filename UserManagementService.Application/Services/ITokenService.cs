using UserManagementService.Domain.Entities.Identity;

namespace UserManagementService.Application.Services;

public interface ITokenService
{
    // ✅ Main Identity JWT (small, ~500 bytes)
    string GenerateAccessToken(ApplicationUser user, IList<string> roles, IList<string> permissions);
    
    // ✅ NEW: Permissions JWT (larger, contains all permissions)
    string GeneratePermissionsToken(string userId, IList<string> permissions);

    string GenerateRefreshToken();
    DateTime GetAccessTokenExpiration();
    DateTime GetRefreshTokenExpiration();
}