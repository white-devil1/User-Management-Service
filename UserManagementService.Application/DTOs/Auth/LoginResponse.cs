using UserManagementService.Application.DTOs.AppPermissions;

namespace UserManagementService.Application.DTOs.Auth;

public class LoginResponse
{
    public string UserId { get; set; } = default!;
    public string Email { get; set; } = default!;

    // Main JWT — identity, tenant, roles, IsSuperAdmin
    public string AccessToken { get; set; } = default!;

    // Permissions JWT — all permission codes for this user
    // SuperAdmin: this token is empty, IsSuperAdmin claim bypasses checks
    public string PermissionsToken { get; set; } = default!;

    public string RefreshToken { get; set; } = default!;
    public DateTime AccessTokenExpires { get; set; }
    public DateTime RefreshTokenExpires { get; set; }
    public DateTime PermissionsTokenExpires { get; set; }

    public Guid TenantId { get; set; }
    public Guid? BranchId { get; set; }
    public bool IsSuperAdmin { get; set; }
    public bool RequiresPasswordChange { get; set; }

    public List<string> Roles { get; set; } = new();

    // Apps -> Pages -> Permissions tree (same shape as the role permissions
    // list endpoint). Empty for SuperAdmin — IsSuperAdmin grants full access.
    public List<GroupedAppDto> Apps { get; set; } = new();
    // NOTE: Permission codes are NOT in this response — decode PermissionsToken
    // on the frontend. SuperAdmin users skip permission checks via IsSuperAdmin.
}
