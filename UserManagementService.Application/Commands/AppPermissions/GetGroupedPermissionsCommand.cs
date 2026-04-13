using MediatR;
using UserManagementService.Application.DTOs.AppPermissions;

namespace UserManagementService.Application.Commands.AppPermissions;

public class GetGroupedPermissionsCommand : IRequest<GroupedPermissionResponse>
{
    public Guid? AppId { get; set; }
    public Guid? PageId { get; set; }
    public bool? IsEnabled { get; set; }
}
