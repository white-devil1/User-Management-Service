using MediatR;
using UserManagementService.Application.Commands.Roles;
using UserManagementService.Application.DTOs.Roles;
using UserManagementService.Application.Events;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.Roles;

public class CreateRoleCommandHandler
    : IRequestHandler<CreateRoleCommand, RoleResponse>
{
    private readonly IRoleService _roleService;
    private readonly ILogPublisher _logPublisher;

    public CreateRoleCommandHandler(
        IRoleService roleService, ILogPublisher logPublisher)
    { _roleService = roleService; _logPublisher = logPublisher; }

    public async Task<RoleResponse> Handle(
        CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var result = await _roleService.CreateRoleAsync(
            request.Name, request.Description,
            request.CallerUserId, request.CallerIsSuperAdmin,
            request.CallerTenantId, request.CallerBranchId, cancellationToken);

        _logPublisher.PublishActivity(new ActivityLogEvent
        {
            ActionType = 10,  // RoleCreated
            EntityType = 1,   // Role
            EntityId = result.Id,
            Description = $"Role '{result.Name}' was created",
            UserId = request.CallerUserId,
            TenantId = request.CallerTenantId
        });
        return result;
    }
}