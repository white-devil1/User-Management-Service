using UserManagementService.Application.DTOs.Auth;
using UserManagementService.Domain.Entities.Identity;

namespace UserManagementService.Application.Services;

public interface ITokenService
{
    // ✅ Main Identity JWT (small, ~500 bytes)
    string GenerateAccessToken(ApplicationUser user, IList<string> roles, IList<string> permissions);

    // Permissions JWT — embeds permission codes plus the apps and pages the
    // user can access, so the frontend can render its menu from a single token.
    string GeneratePermissionsToken(string userId, UserAccessDto access);

    string GenerateRefreshToken();
    DateTime GetAccessTokenExpiration();
    DateTime GetRefreshTokenExpiration();
}