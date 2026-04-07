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
    // NOTE: Permissions are NOT in this response.
    // Decode the PermissionsToken JWT on the frontend to get permission codes.
    // SuperAdmin users skip permission checks via IsSuperAdmin = true.
}
