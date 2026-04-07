using MediatR;
using UserManagementService.Application.Commands.Pages;
using UserManagementService.Application.DTOs.Pages;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.Pages;

public class TogglePageStatusCommandHandler : IRequestHandler<TogglePageStatusCommand, PageDto>
{
    private readonly IPageService _pageService;

    public TogglePageStatusCommandHandler(IPageService pageService)
    {
        _pageService = pageService;
    }

    public async Task<PageDto> Handle(TogglePageStatusCommand request, CancellationToken cancellationToken)
    {
try
        {

        return await _pageService.TogglePageStatusAsync(request.Id, request.IsActive, request.UpdatedBy, cancellationToken);
            _logPublisher.PublishActivity(new ActivityLogEvent
            {
                ActionType = 33,  // PageStatusToggled
                EntityType = 3,
                EntityId = request.Id?.ToString() ?? "unknown",
                Description = "PageStatusToggled completed",
                UserId = request.UpdatedBy ?? request.DeletedBy ?? request.CreatedBy,
                IsSuccess = true
            });

        }
        catch (Exception ex)
        {
            _logPublisher.PublishActivity(new ActivityLogEvent
            {
                ActionType = 33,  // PageStatusToggled
                EntityType = 3,
                Description = $"Unexpected error in PageStatusToggled",
                UserId = request.UpdatedBy ?? request.DeletedBy ?? request.CreatedBy,
                IsSuccess = false,
                FailureReason = ex.Message
            });
            throw;
        }
    }
}