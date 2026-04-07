using MediatR;
using UserManagementService.Application.Commands.Pages;
using UserManagementService.Application.DTOs.Pages;
using UserManagementService.Application.Events;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.Pages;

public class UpdatePageCommandHandler : IRequestHandler<UpdatePageCommand, PageDto>
{
    private readonly IPageService _pageService;
    private readonly ILogPublisher _logPublisher;

    public UpdatePageCommandHandler(IPageService pageService, ILogPublisher logPublisher)
    {
        _pageService = pageService;
        _logPublisher = logPublisher;
    }

    public async Task<PageDto> Handle(UpdatePageCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var updateRequest = new UpdatePageRequest
            {
                Name = request.Name,
                Description = request.Description,
                DisplayOrder = request.DisplayOrder,
                IsActive = request.IsActive
            };

            var result = await _pageService.UpdatePageAsync(request.Id, updateRequest, request.UpdatedBy, cancellationToken);

            _logPublisher.PublishActivity(new ActivityLogEvent
            {
                ActionType = 31,  // PageUpdated
                EntityType = 3,
                EntityId = request.Id.ToString(),
                Description = "PageUpdated completed",
                UserId = request.UpdatedBy,
                IsSuccess = true
            });

            return result;
        }
        catch (Exception ex)
        {
            _logPublisher.PublishActivity(new ActivityLogEvent
            {
                ActionType = 31,  // PageUpdated
                EntityType = 3,
                Description = $"Unexpected error in PageUpdated",
                UserId = request.UpdatedBy,
                IsSuccess = false,
                FailureReason = ex.Message
            });
            throw;
        }
    }
}
