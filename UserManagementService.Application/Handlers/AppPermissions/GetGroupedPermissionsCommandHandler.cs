using MediatR;
using UserManagementService.Application.Commands.AppPermissions;
using UserManagementService.Application.DTOs.AppPermissions;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.AppPermissions;

public class GetGroupedPermissionsCommandHandler
    : IRequestHandler<GetGroupedPermissionsCommand, GroupedPermissionResponse>
{
    private readonly IAppPermissionService _permissionService;

    public GetGroupedPermissionsCommandHandler(IAppPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    public async Task<GroupedPermissionResponse> Handle(
        GetGroupedPermissionsCommand request,
        CancellationToken cancellationToken)
    {
        return await _permissionService.GetGroupedPermissionsAsync(
            request.AppId,
            request.PageId,
            request.IsEnabled,
            cancellationToken
        );
    }
}
