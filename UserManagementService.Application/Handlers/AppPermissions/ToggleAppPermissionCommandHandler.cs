using MediatR;
using UserManagementService.Application.Commands.AppPermissions;
using UserManagementService.Application.DTOs.AppPermissions;
using UserManagementService.Application.Events;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.AppPermissions;

public class ToggleAppPermissionCommandHandler
    : IRequestHandler<ToggleAppPermissionCommand, AppPermissionDto>
{
    private readonly IAppPermissionService _permissionService;
    private readonly ILogPublisher _logPublisher;

    public ToggleAppPermissionCommandHandler(
        IAppPermissionService permissionService, ILogPublisher logPublisher)
    { _permissionService = permissionService; _logPublisher = logPublisher; }

    public async Task<AppPermissionDto> Handle(
        ToggleAppPermissionCommand request, CancellationToken cancellationToken)
    {
        var result = await _permissionService.TogglePermissionAsync(
            request.Id, request.IsEnabled, request.UpdatedBy, cancellationToken);

        _logPublisher.PublishActivity(new ActivityLogEvent
        {
            ActionType = 43,  // PermissionToggled
            EntityType = 5,   // Permission
            EntityId = request.Id.ToString(),
            Description = $"Permission '{result.PermissionCode}' was {(request.IsEnabled ? "enabled" : "disabled")}",
            UserId = request.UpdatedBy
        });
        return result;
    }
}