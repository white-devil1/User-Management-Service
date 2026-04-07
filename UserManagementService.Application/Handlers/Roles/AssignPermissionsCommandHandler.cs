using MediatR;
using UserManagementService.Application.Commands.Roles;
using UserManagementService.Application.DTOs.Roles;
using UserManagementService.Application.Events;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.Roles;

public class AssignPermissionsCommandHandler
    : IRequestHandler<AssignPermissionsCommand, RoleResponse>
{
    private readonly IRoleService _roleService;
    private readonly ILogPublisher _logPublisher;

    public AssignPermissionsCommandHandler(
        IRoleService roleService, ILogPublisher logPublisher)
    { _roleService = roleService; _logPublisher = logPublisher; }

    public async Task<RoleResponse> Handle(
        AssignPermissionsCommand request, CancellationToken cancellationToken)
    {
        var result = await _roleService.AssignPermissionsAsync(
            request.RoleId, request.PermissionIds,
            request.CallerUserId, request.CallerIsSuperAdmin,
            request.CallerTenantId, cancellationToken);

        _logPublisher.PublishActivity(new ActivityLogEvent
        {
            ActionType = 13,  // PermissionsAssigned
            EntityType = 1,   // Role
            EntityId = request.RoleId,
            Description = $"Permissions assigned to role '{result.Name}'",
            UserId = request.CallerUserId,
            TenantId = request.CallerTenantId
        });
        return result;
    }
}