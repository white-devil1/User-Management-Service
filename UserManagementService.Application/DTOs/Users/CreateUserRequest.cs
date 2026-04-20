namespace UserManagementService.Application.DTOs.Users;

public class CreateUserRequest
{
    public string Email { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public Guid TenantId { get; set; }
    public Guid? BranchId { get; set; }
    public bool IsActive { get; set; } = true;
    public List<string> RoleNames { get; set; } = new();
}