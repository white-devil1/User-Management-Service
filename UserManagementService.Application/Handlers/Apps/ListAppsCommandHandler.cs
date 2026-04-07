using MediatR;
using UserManagementService.Application.Commands.Apps;
using UserManagementService.Application.DTOs.Apps;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.Apps;

public class ListAppsCommandHandler : IRequestHandler<ListAppsCommand, AppListResponse>
{
    private readonly IAppService _appService;

    public ListAppsCommandHandler(IAppService appService)
    {
        _appService = appService;
    }

    public async Task<AppListResponse> Handle(ListAppsCommand request, CancellationToken cancellationToken)
    {
        return await _appService.GetAppsAsync(
            request.Search,
            request.Code,
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