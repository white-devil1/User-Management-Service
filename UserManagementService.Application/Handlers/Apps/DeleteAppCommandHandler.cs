using MediatR;
using UserManagementService.Application.Commands.Apps;
using UserManagementService.Application.Events;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.Apps;

public class DeleteAppCommandHandler
    : IRequestHandler<DeleteAppCommand, bool>
{
    private readonly IAppService _appService;
    private readonly ILogPublisher _logPublisher;

    public DeleteAppCommandHandler(
        IAppService appService, ILogPublisher logPublisher)
    { _appService = appService; _logPublisher = logPublisher; }

    public async Task<bool> Handle(
        DeleteAppCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _appService.DeleteAppAsync(
                request.Id, request.DeletedBy, cancellationToken);

            _logPublisher.PublishActivity(new ActivityLogEvent
            {
                ActionType = 22,  // AppDeleted
                EntityType = 2,   // App
                EntityId = request.Id.ToString(),
                Description = "App was deleted",
                UserId = request.DeletedBy,
                IsSuccess = true
            });
            return result;
        }
        catch (Exception ex)
        {
            _logPublisher.PublishActivity(new ActivityLogEvent
            {
                ActionType = 22,  // AppDeleted
                EntityType = 2,   // App
                EntityId = request.Id.ToString(),
                Description = "App deletion failed",
                UserId = request.DeletedBy,
                IsSuccess = false,
                FailureReason = ex.Message
            });
            throw;
        }
    }
}
