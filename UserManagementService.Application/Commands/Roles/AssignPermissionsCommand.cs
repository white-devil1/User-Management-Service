using MediatR;
using UserManagementService.Application.DTOs.Roles;

namespace UserManagementService.Application.Commands.Roles;

public class AssignPermissionsCommand : IRequest<RoleResponse>
{
    public string RoleId { get; set; } = default!;
    public List<Guid> PermissionIds { get; set; } = new();
    public string CallerUserId { get; set; } = default!;
    public bool CallerIsSuperAdmin { get; set; }
    public Guid CallerTenantId { get; set; }
}
