using MediatR;
using UserManagementService.Application.Commands.AppActions;
using UserManagementService.Application.DTOs.AppActions;
using UserManagementService.Application.Events;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.AppActions;

public class CreateAppActionCommandHandler
    : IRequestHandler<CreateAppActionCommand, AppActionDto>
{
    private readonly IAppActionService _appActionService;
    private readonly ILogPublisher _logPublisher;

    public CreateAppActionCommandHandler(
        IAppActionService appActionService, ILogPublisher logPublisher)
    { _appActionService = appActionService; _logPublisher = logPublisher; }

    public async Task<AppActionDto> Handle(
        CreateAppActionCommand request, CancellationToken cancellationToken)
    {
        var result = await _appActionService.CreateActionAsync(
            new CreateAppActionRequest
            {
                PageId = request.PageId,
                Name = request.Name,
                Description = request.Description,
                Type = request.Type,
                DisplayOrder = request.DisplayOrder
            },
            request.CreatedBy, cancellationToken);

        _logPublisher.PublishActivity(new ActivityLogEvent
        {
            ActionType = 40,  // ActionCreated
            EntityType = 4,   // AppAction
            EntityId = result.Id.ToString(),
            Description = $"Action '{result.Name}' was created",
            UserId = request.CreatedBy
        });
        return result;
    }
}