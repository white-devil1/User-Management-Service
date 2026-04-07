using MediatR;
using UserManagementService.Application.Commands.Apps;
using UserManagementService.Application.DTOs.Apps;
using UserManagementService.Application.Events;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.Apps;

public class CreateAppCommandHandler
    : IRequestHandler<CreateAppCommand, AppDto>
{
    private readonly IAppService _appService;
    private readonly ILogPublisher _logPublisher;

    public CreateAppCommandHandler(
        IAppService appService, ILogPublisher logPublisher)
    { _appService = appService; _logPublisher = logPublisher; }

    public async Task<AppDto> Handle(
        CreateAppCommand request, CancellationToken cancellationToken)
    {
        var result = await _appService.CreateAppAsync(
            new CreateAppRequest
            {
                Name = request.Name,
                Description = request.Description,
                Icon = request.Icon,
                DisplayOrder = request.DisplayOrder
            },
            request.CreatedBy, cancellationToken);

        _logPublisher.PublishActivity(new ActivityLogEvent
        {
            ActionType = 20,  // AppCreated
            EntityType = 2,   // App
            EntityId = result.Id.ToString(),
            Description = $"App '{result.Name}' was created",
            UserId = request.CreatedBy
        });
        return result;
    }
}