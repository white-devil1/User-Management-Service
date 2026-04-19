using MediatR;
using UserManagementService.Application.DTOs.AppPermissions;

namespace UserManagementService.Application.Commands.AppPermissions;

public class BulkTogglePermissionStatusCommand : IRequest<BulkTogglePermissionResponse>
{
    public List<BulkToggleItem> PermissionStatuses { get; set; } = new();
    public string UpdatedBy { get; set; } = default!;
}
