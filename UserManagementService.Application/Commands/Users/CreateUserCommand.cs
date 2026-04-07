using MediatR;
using UserManagementService.Application.DTOs.Users;

namespace UserManagementService.Application.Commands.Users;

public class CreateUserCommand : IRequest<UserResponse>
{
    public string Email { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public Guid TenantId { get; set; }
    public Guid? BranchId { get; set; }
    public bool IsSuperAdmin { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public List<string> RoleNames { get; set; } = new();

    // ✅ AUDIT FIELD (Added)
    public string? CreatedBy { get; set; }
}