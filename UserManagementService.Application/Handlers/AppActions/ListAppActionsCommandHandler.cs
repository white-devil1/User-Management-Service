using MediatR;
using UserManagementService.Application.Commands.AppActions;
using UserManagementService.Application.DTOs.AppActions;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.AppActions;

public class ListAppActionsCommandHandler : IRequestHandler<ListAppActionsCommand, AppActionListResponse>
{
    private readonly IAppActionService _appActionService;

    public ListAppActionsCommandHandler(IAppActionService appActionService)
    {
        _appActionService = appActionService;
    }

    public async Task<AppActionListResponse> Handle(ListAppActionsCommand request, CancellationToken cancellationToken)
    {
        return await _appActionService.GetActionsAsync(
            request.AppId,      // ✅ NEW: Pass appId first
            request.PageId,     // ✅ Now passes Guid? (nullable)
            request.Search,
            request.IsActive,
            request.IncludeDeleted,
            request.Page,
            request.PageSize,
            request.SortBy,
            request.SortOrder,
            cancellationToken
        );
    }
}