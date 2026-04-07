using MediatR;
using UserManagementService.Application.Commands.Roles;
using UserManagementService.Application.DTOs.Roles;
using UserManagementService.Application.Events;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.Roles;

public class UpdateRoleCommandHandler
    : IRequestHandler<UpdateRoleCommand, RoleResponse>
{
    private readonly IRoleService _roleService;
    private readonly ILogPublisher _logPublisher;

    public UpdateRoleCommandHandler(
        IRoleService roleService, ILogPublisher logPublisher)
    { _roleService = roleService; _logPublisher = logPublisher; }

    public async Task<RoleResponse> Handle(
        UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var result = await _roleService.UpdateRoleAsync(
            request.Id, request.Name, request.Description,
            request.CallerUserId, request.CallerIsSuperAdmin,
            request.CallerTenantId, cancellationToken);

        _logPublisher.PublishActivity(new ActivityLogEvent
        {
            ActionType = 11,  // RoleUpdated
            EntityType = 1,   // Role
            EntityId = result.Id,
            Description = $"Role '{result.Name}' was updated",
            UserId = request.CallerUserId,
            TenantId = request.CallerTenantId
        });
        return result;
    }
}