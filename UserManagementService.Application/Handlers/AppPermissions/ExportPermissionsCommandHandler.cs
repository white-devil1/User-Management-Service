using MediatR;
using UserManagementService.Application.Commands.AppPermissions;
using UserManagementService.Application.DTOs.AppPermissions;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.AppPermissions;

public class ExportPermissionsCommandHandler
    : IRequestHandler<ExportPermissionsCommand, List<ExportPermissionRowDto>>
{
    private readonly IAppPermissionService _permissionService;

    public ExportPermissionsCommandHandler(IAppPermissionService permissionService)
        => _permissionService = permissionService;

    public Task<List<ExportPermissionRowDto>> Handle(
        ExportPermissionsCommand request,
        CancellationToken cancellationToken)
        => _permissionService.GetPermissionsForExportAsync(
            request.AppId,
            request.PageId,
            cancellationToken);
}
