namespace UserManagementService.Application.DTOs.Auth;

public class UserAccessDto
{
    public List<string> Permissions { get; set; } = new();

    // Flat list of apps the user has access to (derived from permissions).
    // Empty for SuperAdmin users — IsSuperAdmin bypasses access checks.
    public List<AppPermissions.UserAppDto> Apps { get; set; } = new();
}
