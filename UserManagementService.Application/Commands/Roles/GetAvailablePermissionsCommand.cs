using MediatR;
using UserManagementService.Application.DTOs.Roles;

namespace UserManagementService.Application.Commands.Roles;

public class GetAvailablePermissionsCommand : IRequest<RolePermissionsGrouped>
{
    public string CallerUserId { get; set; } = default!;
    public bool CallerIsSuperAdmin { get; set; }
}
