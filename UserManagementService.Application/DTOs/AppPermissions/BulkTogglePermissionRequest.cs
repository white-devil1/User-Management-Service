using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Application.DTOs.AppPermissions;

public class BulkTogglePermissionRequest
{
    [Required]
    public List<BulkToggleItem> PermissionStatuses { get; set; } = new();
}
