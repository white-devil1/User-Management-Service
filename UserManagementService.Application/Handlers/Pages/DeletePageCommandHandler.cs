using MediatR;
using UserManagementService.Application.Commands.Pages;
using UserManagementService.Application.Events;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.Pages;

public class DeletePageCommandHandler : IRequestHandler<DeletePageCommand, bool>
{
    private readonly IPageService _pageService;
    private readonly ILogPublisher _logPublisher;

    public DeletePageCommandHandler(IPageService pageService, ILogPublisher logPublisher)
    {
        _pageService = pageService;
        _logPublisher = logPublisher;
    }

    public async Task<bool> Handle(DeletePageCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _pageService.DeletePageAsync(request.Id, request.DeletedBy, cancellationToken);

            _logPublisher.PublishActivity(new ActivityLogEvent
            {
                ActionType = 32,  // PageDeleted
                EntityType = 3,
                EntityId = request.Id.ToString(),
                Description = "PageDeleted completed",
                UserId = request.DeletedBy,
                IsSuccess = true
            });

            return result;
        }
        catch (Exception ex)
        {
            _logPublisher.PublishActivity(new ActivityLogEvent
            {
                ActionType = 32,  // PageDeleted
                EntityType = 3,
                Description = $"Unexpected error in PageDeleted",
                UserId = request.DeletedBy,
                IsSuccess = false,
                FailureReason = ex.Message
            });
            throw;
        }
    }
}
