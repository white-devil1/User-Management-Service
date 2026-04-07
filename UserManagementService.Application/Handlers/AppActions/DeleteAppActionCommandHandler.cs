using MediatR;
using UserManagementService.Application.Commands.AppActions;
using UserManagementService.Application.DTOs.AppActions;
using UserManagementService.Application.Events;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.AppActions;

public class DeleteAppActionCommandHandler : IRequestHandler<DeleteAppActionCommand, bool>
{
    private readonly IAppActionService _appActionService;
    private readonly ILogPublisher _logPublisher;

    public DeleteAppActionCommandHandler(IAppActionService appActionService, ILogPublisher logPublisher)
    {
        _appActionService = appActionService;
        _logPublisher = logPublisher;
    }

    public async Task<bool> Handle(DeleteAppActionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _appActionService.DeleteActionAsync(request.Id, request.DeletedBy, cancellationToken);

            _logPublisher.PublishActivity(new ActivityLogEvent
            {
                ActionType = 42,  // ActionDeleted
                EntityType = 4,   // AppAction
                EntityId = request.Id.ToString(),
                Description = $"Action with ID '{request.Id}' was deleted",
                UserId = request.DeletedBy,
                IsSuccess = true
            });

            return result;
        }
        catch (Exception ex)
        {
            _logPublisher.PublishActivity(new ActivityLogEvent
            {
                ActionType = 42,  // ActionDeleted
                EntityType = 4,   // AppAction
                EntityId = request.Id.ToString(),
                Description = "Action deletion failed",
                UserId = request.DeletedBy,
                IsSuccess = false,
                FailureReason = ex.Message
            });
            throw;
        }
    }
}