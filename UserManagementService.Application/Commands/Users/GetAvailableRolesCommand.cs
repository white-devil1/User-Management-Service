using MediatR;
using UserManagementService.Application.DTOs.Users;

namespace UserManagementService.Application.Commands.Users;

public class GetAvailableRolesCommand : IRequest<List<RoleDto>>
{
    public bool IsSuperAdmin { get; set; }
    public string? CallerTenantId { get; set; }
}
