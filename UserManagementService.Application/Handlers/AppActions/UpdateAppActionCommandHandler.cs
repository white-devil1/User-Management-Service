using MediatR;
using UserManagementService.Application.Commands.AppActions;
using UserManagementService.Application.DTOs.AppActions;
using UserManagementService.Application.Events;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.AppActions;

public class UpdateAppActionCommandHandler : IRequestHandler<UpdateAppActionCommand, AppActionDto>
{
    private readonly IAppActionService _appActionService;
    private readonly ILogPublisher _logPublisher;

    public UpdateAppActionCommandHandler(IAppActionService appActionService, ILogPublisher logPublisher)
    {
        _appActionService = appActionService;
        _logPublisher = logPublisher;
    }

    public async Task<AppActionDto> Handle(UpdateAppActionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var updateRequest = new UpdateAppActionRequest
            {
                Name = request.Name,
                Description = request.Description,
                Type = request.Type,
                DisplayOrder = request.DisplayOrder,
                IsActive = request.IsActive
            };

            var result = await _appActionService.UpdateActionAsync(request.Id, updateRequest, request.UpdatedBy, cancellationToken);

            _logPublisher.PublishActivity(new ActivityLogEvent
            {
                ActionType = 41,  // ActionUpdated
                EntityType = 4,   // AppAction
                EntityId = result.Id.ToString(),
                Description = $"Action '{result.Name}' was updated",
                UserId = request.UpdatedBy,
                IsSuccess = true
            });

            return result;
        }
        catch (Exception ex)
        {
            _logPublisher.PublishActivity(new ActivityLogEvent
            {
                ActionType = 41,  // ActionUpdated
                EntityType = 4,   // AppAction
                EntityId = request.Id.ToString(),
                Description = "Action update failed",
                UserId = request.UpdatedBy,
                IsSuccess = false,
                FailureReason = ex.Message
            });
            throw;
        }
    }
}