using MediatR;
using UserManagementService.Application.Commands.AppPermissions;
using UserManagementService.Application.DTOs.AppPermissions;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.AppPermissions;

public class ListAppPermissionsCommandHandler : IRequestHandler<ListAppPermissionsCommand, AppPermissionListResponse>
{
    private readonly IAppPermissionService _permissionService;

    public ListAppPermissionsCommandHandler(IAppPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    public async Task<AppPermissionListResponse> Handle(ListAppPermissionsCommand request, CancellationToken cancellationToken)
    {
        return await _permissionService.GetPermissionsAsync(
            request.AppId,
            request.PageId,
            request.ActionId,
            request.IsEnabled,
            request.IncludeDeleted,
            request.Page,
            request.PageSize,
            request.SortBy,
            request.SortOrder,
            cancellationToken
        );
    }
}