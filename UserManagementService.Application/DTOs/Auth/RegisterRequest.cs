namespace UserManagementService.Application.DTOs.Auth;

public class RegisterRequest
{
    public string Email { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string Password { get; set; } = default!;
    public Guid TenantId { get; set; }
    public Guid? BranchId { get; set; }
    public bool IsSuperAdmin { get; set; } = false;
}