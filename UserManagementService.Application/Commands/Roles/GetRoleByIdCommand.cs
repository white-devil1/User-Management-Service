using MediatR;
using UserManagementService.Application.DTOs.Roles;

namespace UserManagementService.Application.Commands.Roles;

public class GetRoleByIdCommand : IRequest<RoleResponse>
{
    public string Id { get; set; } = default!;
    public bool CallerIsSuperAdmin { get; set; }
    public Guid CallerTenantId { get; set; }
}
