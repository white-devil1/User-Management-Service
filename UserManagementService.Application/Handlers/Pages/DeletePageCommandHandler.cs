using MediatR;
using UserManagementService.Application.Commands.Pages;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.Pages;

public class DeletePageCommandHandler : IRequestHandler<DeletePageCommand, bool>
{
    private readonly IPageService _pageService;

    public DeletePageCommandHandler(IPageService pageService)
    {
        _pageService = pageService;
    }

    public async Task<bool> Handle(DeletePageCommand request, CancellationToken cancellationToken)
    {
try
        {

        return await _pageService.DeletePageAsync(request.Id, request.DeletedBy, cancellationToken);
            _logPublisher.PublishActivity(new ActivityLogEvent
            {
                ActionType = 32,  // PageDeleted
                EntityType = 3,
                EntityId = request.Id?.ToString() ?? "unknown",
                Description = "PageDeleted completed",
                UserId = request.UpdatedBy ?? request.DeletedBy ?? request.CreatedBy,
                IsSuccess = true
            });

        }
        catch (Exception ex)
        {
            _logPublisher.PublishActivity(new ActivityLogEvent
            {
                ActionType = 32,  // PageDeleted
                EntityType = 3,
                Description = $"Unexpected error in PageDeleted",
                UserId = request.UpdatedBy ?? request.DeletedBy ?? request.CreatedBy,
                IsSuccess = false,
                FailureReason = ex.Message
            });
            throw;
        }
    }
}