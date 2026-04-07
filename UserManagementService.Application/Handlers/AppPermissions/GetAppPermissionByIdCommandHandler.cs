using MediatR;
using UserManagementService.Application.Commands.AppPermissions;
using UserManagementService.Application.DTOs.AppPermissions;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.AppPermissions;

public class GetAppPermissionByIdCommandHandler : IRequestHandler<GetAppPermissionByIdCommand, AppPermissionDto>
{
    private readonly IAppPermissionService _permissionService;

    public GetAppPermissionByIdCommandHandler(IAppPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    public async Task<AppPermissionDto> Handle(GetAppPermissionByIdCommand request, CancellationToken cancellationToken)
    {
        return await _permissionService.GetPermissionByIdAsync(request.Id, cancellationToken);
    }
}