using MediatR;
using UserManagementService.Application.Commands.AppPermissions;
using UserManagementService.Application.Common.Exceptions;
using UserManagementService.Application.DTOs.AppPermissions;
using UserManagementService.Application.Events;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.AppPermissions;

public class BulkTogglePermissionStatusCommandHandler
    : IRequestHandler<BulkTogglePermissionStatusCommand, BulkTogglePermissionResponse>
{
    private readonly IAppPermissionService _permissionService;
    private readonly ILogPublisher _logPublisher;

    public BulkTogglePermissionStatusCommandHandler(
        IAppPermissionService permissionService, ILogPublisher logPublisher)
    {
        _permissionService = permissionService;
        _logPublisher = logPublisher;
    }

    public async Task<BulkTogglePermissionResponse> Handle(
        BulkTogglePermissionStatusCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _permissionService.BulkTogglePermissionStatusAsync(
            request.PermissionStatuses, request.UpdatedBy, cancellationToken);

        // Log each permission toggle as an activity
        foreach (var item in result.Results)
        {
            var originalItem = request.PermissionStatuses.First(x => x.PermissionId == item.Id);
            _logPublisher.PublishActivity(new ActivityLogEvent
            {
                ActionType = 43,  // PermissionToggled
                EntityType = 5,   // Permission
                EntityId = item.Id.ToString(),
                Description = $"Permission '{item.ActionName}' was {(originalItem.IsEnabled ? "enabled" : "disabled")}",
                UserId = request.UpdatedBy
            });
        }

        return result;
    }
}
