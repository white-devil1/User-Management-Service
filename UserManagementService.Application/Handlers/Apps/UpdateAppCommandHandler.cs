using MediatR;
using UserManagementService.Application.Commands.Apps;
using UserManagementService.Application.DTOs.Apps;
using UserManagementService.Application.Events;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.Apps;

public class UpdateAppCommandHandler
    : IRequestHandler<UpdateAppCommand, AppDto>
{
    private readonly IAppService _appService;
    private readonly ILogPublisher _logPublisher;

    public UpdateAppCommandHandler(
        IAppService appService, ILogPublisher logPublisher)
    { _appService = appService; _logPublisher = logPublisher; }

    public async Task<AppDto> Handle(
        UpdateAppCommand request, CancellationToken cancellationToken)
    {
        var result = await _appService.UpdateAppAsync(
            request.Id,
            new UpdateAppRequest
            {
                Name = request.Name,
                Description = request.Description,
                Icon = request.Icon,
                DisplayOrder = request.DisplayOrder,
                IsActive = request.IsActive
            },
            request.UpdatedBy, cancellationToken);

        _logPublisher.PublishActivity(new ActivityLogEvent
        {
            ActionType = 21,  // AppUpdated
            EntityType = 2,   // App
            EntityId = result.Id.ToString(),
            Description = $"App '{result.Name}' was updated",
            UserId = request.UpdatedBy
        });
        return result;
    }
}