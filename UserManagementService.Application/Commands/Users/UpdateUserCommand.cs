using MediatR;
using UserManagementService.Application.DTOs.Users;

namespace UserManagementService.Application.Commands.Users;

public class UpdateUserCommand : IRequest<UserResponse>
{
    public string Id { get; set; } = default!;
    public string? Email { get; set; }
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool? IsActive { get; set; }
    public Guid? BranchId { get; set; }
    public List<Guid>? RoleIds { get; set; }

    // ✅ AUDIT FIELD (Added)
    public string? UpdatedBy { get; set; }
}