using MediatR;
using UserManagementService.Application.Commands.Pages;
using UserManagementService.Application.DTOs.Pages;
using UserManagementService.Application.Events;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.Pages;

public class CreatePageCommandHandler
    : IRequestHandler<CreatePageCommand, PageDto>
{
    private readonly IPageService _pageService;
    private readonly ILogPublisher _logPublisher;

    public CreatePageCommandHandler(
        IPageService pageService, ILogPublisher logPublisher)
    { _pageService = pageService; _logPublisher = logPublisher; }

    public async Task<PageDto> Handle(
        CreatePageCommand request, CancellationToken cancellationToken)
    {
        var result = await _pageService.CreatePageAsync(
            new CreatePageRequest
            {
                AppId = request.AppId,
                Name = request.Name,
                Description = request.Description,
                DisplayOrder = request.DisplayOrder
            },
            request.CreatedBy, cancellationToken);

        _logPublisher.PublishActivity(new ActivityLogEvent
        {
            ActionType = 30,  // PageCreated
            EntityType = 3,   // Page
            EntityId = result.Id.ToString(),
            Description = $"Page '{result.Name}' was created",
            UserId = request.CreatedBy
        });
        return result;
    }
}