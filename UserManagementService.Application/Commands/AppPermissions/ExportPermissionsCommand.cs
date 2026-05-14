using MediatR;
using UserManagementService.Application.DTOs.AppPermissions;

namespace UserManagementService.Application.Commands.AppPermissions;

public class ExportPermissionsCommand : IRequest<List<ExportPermissionRowDto>>
{
    public Guid? AppId  { get; set; }
    public Guid? PageId { get; set; }
}
