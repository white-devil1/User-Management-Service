using MediatR;
using UserManagementService.Application.Commands.AppActions;
using UserManagementService.Application.DTOs.AppActions;
using UserManagementService.Application.Events;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.AppActions;

public class ToggleAppActionStatusCommandHandler : IRequestHandler<ToggleAppActionStatusCommand, AppActionDto>
{
    private readonly IAppActionService _appActionService;
    private readonly ILogPublisher _logPublisher;

    public ToggleAppActionStatusCommandHandler(IAppActionService appActionService, ILogPublisher logPublisher)
    {
        _appActionService = appActionService;
        _logPublisher = logPublisher;
    }

    public async Task<AppActionDto> Handle(ToggleAppActionStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // ✅ Correct parameter order: id, isActive, updatedBy, cancellationToken
            var result = await _appActionService.ToggleActionStatusAsync(
                request.Id,
                request.IsActive,
                request.UpdatedBy,
                cancellationToken);

            _logPublisher.PublishActivity(new ActivityLogEvent
            {
                ActionType = 43,  // PermissionToggled (used for status toggle)
                EntityType = 4,   // AppAction
                EntityId = result.Id.ToString(),
                Description = $"Action '{result.Name}' status was {(request.IsActive ? "activated" : "deactivated")}",
                UserId = request.UpdatedBy,
                IsSuccess = true
            });

            return result;
        }
        catch (Exception ex)
        {
            _logPublisher.PublishActivity(new ActivityLogEvent
            {
                ActionType = 43,  // PermissionToggled (used for status toggle)
                EntityType = 4,   // AppAction
                EntityId = request.Id.ToString(),
                Description = "Action status toggle failed",
                UserId = request.UpdatedBy,
                IsSuccess = false,
                FailureReason = ex.Message
            });
            throw;
        }
    }
}