using UserManagementService.Application.DTOs.Auth;
using UserManagementService.Domain.Entities.Identity;

namespace UserManagementService.Application.Services;

public interface IAppPermissionResolverService
{
    Task<List<string>> GetUserPermissionsAsync(
        ApplicationUser user,
        IList<string> roles,
        CancellationToken ct = default);

    // Returns the user's permission codes plus the apps and pages they have
    // access to (derived from those permissions). For SuperAdmin users, all
    // active apps and pages are returned so the frontend can render the full menu.
    Task<UserAccessDto> GetUserAccessAsync(
        ApplicationUser user,
        IList<string> roles,
        CancellationToken ct = default);
}
