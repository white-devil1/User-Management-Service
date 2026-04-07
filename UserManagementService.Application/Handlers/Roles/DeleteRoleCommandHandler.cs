using MediatR;
using UserManagementService.Application.Commands.Roles;
using UserManagementService.Application.Events;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.Roles;

public class DeleteRoleCommandHandler
    : IRequestHandler<DeleteRoleCommand, bool>
{
    private readonly IRoleService _roleService;
    private readonly ILogPublisher _logPublisher;

    public DeleteRoleCommandHandler(
        IRoleService roleService, ILogPublisher logPublisher)
    { _roleService = roleService; _logPublisher = logPublisher; }

    public async Task<bool> Handle(
        DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var result = await _roleService.DeleteRoleAsync(
            request.Id, request.CallerUserId,
            request.CallerIsSuperAdmin, request.CallerTenantId,
            cancellationToken);

        _logPublisher.PublishActivity(new ActivityLogEvent
        {
            ActionType = 12,  // RoleDeleted
            EntityType = 1,   // Role
            EntityId = request.Id,
            Description = "Role was deleted",
            UserId = request.CallerUserId,
            TenantId = request.CallerTenantId
        });
        return result;
    }
}