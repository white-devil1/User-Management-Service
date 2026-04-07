using MediatR;
using UserManagementService.Application.Commands.Apps;
using UserManagementService.Application.DTOs.Apps;
using UserManagementService.Application.Events;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.Apps;

public class ToggleAppStatusCommandHandler
    : IRequestHandler<ToggleAppStatusCommand, AppDto>
{
    private readonly IAppService _appService;
    private readonly ILogPublisher _logPublisher;

    public ToggleAppStatusCommandHandler(
        IAppService appService, ILogPublisher logPublisher)
    { _appService = appService; _logPublisher = logPublisher; }

    public async Task<AppDto> Handle(
        ToggleAppStatusCommand request, CancellationToken cancellationToken)
    {
        var result = await _appService.ToggleAppStatusAsync(
            request.Id, request.IsActive, request.UpdatedBy, cancellationToken);

        _logPublisher.PublishActivity(new ActivityLogEvent
        {
            ActionType = 21,  // AppUpdated (status toggle)
            EntityType = 2,   // App
            EntityId = result.Id.ToString(),
            Description = $"App '{result.Name}' was {(request.IsActive ? "activated" : "deactivated")}",
            UserId = request.UpdatedBy
        });
        return result;
    }
}