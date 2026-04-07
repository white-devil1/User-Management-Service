using MediatR;
using UserManagementService.Application.DTOs.Roles;

namespace UserManagementService.Application.Commands.Roles;

public class CreateRoleCommand : IRequest<RoleResponse>
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string CallerUserId { get; set; } = default!;
    public bool CallerIsSuperAdmin { get; set; }
    public Guid CallerTenantId { get; set; }
    public Guid? CallerBranchId { get; set; }
}
