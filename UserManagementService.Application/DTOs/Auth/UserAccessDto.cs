using UserManagementService.Application.DTOs.AppPermissions;

namespace UserManagementService.Application.DTOs.Auth;

public class UserAccessDto
{
    public List<string> Permissions { get; set; } = new();

    // Apps -> Pages -> Permissions tree, mirroring the role permissions list
    // page structure (GroupedAppDto / GroupedPageDto / GroupedPermissionDto).
    // Empty for SuperAdmin users — IsSuperAdmin bypasses access checks, so the
    // frontend doesn't need an explicit access tree for them.
    public List<GroupedAppDto> Apps { get; set; } = new();
}
