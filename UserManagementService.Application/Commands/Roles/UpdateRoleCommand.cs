using MediatR;
using UserManagementService.Application.DTOs.Roles;

namespace UserManagementService.Application.Commands.Roles;

public class UpdateRoleCommand : IRequest<RoleResponse>
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string CallerUserId { get; set; } = default!;
    public bool CallerIsSuperAdmin { get; set; }
    public Guid CallerTenantId { get; set; }
}
