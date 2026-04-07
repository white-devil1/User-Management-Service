using MediatR;
using UserManagementService.Application.Commands.Pages;
using UserManagementService.Application.DTOs.Pages;
using UserManagementService.Application.Events;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.Pages;

public class TogglePageStatusCommandHandler : IRequestHandler<TogglePageStatusCommand, PageDto>
{
    private readonly IPageService _pageService;
    private readonly ILogPublisher _logPublisher;

    public TogglePageStatusCommandHandler(IPageService pageService, ILogPublisher logPublisher)
    {
        _pageService = pageService;
        _logPublisher = logPublisher;
    }

    public async Task<PageDto> Handle(TogglePageStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _pageService.TogglePageStatusAsync(request.Id, request.IsActive, request.UpdatedBy, cancellationToken);

            _logPublisher.PublishActivity(new ActivityLogEvent
            {
                ActionType = 33,  // PageStatusToggled
                EntityType = 3,
                EntityId = request.Id.ToString(),
                Description = "PageStatusToggled completed",
                UserId = request.UpdatedBy,
                IsSuccess = true
            });

            return result;
        }
        catch (Exception ex)
        {
            _logPublisher.PublishActivity(new ActivityLogEvent
            {
                ActionType = 33,  // PageStatusToggled
                EntityType = 3,
                Description = $"Unexpected error in PageStatusToggled",
                UserId = request.UpdatedBy,
                IsSuccess = false,
                FailureReason = ex.Message
            });
            throw;
        }
    }
}
