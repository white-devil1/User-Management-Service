namespace UserManagementService.Application.DTOs.AppPermissions;

public class BulkToggleItem
{
    public Guid PermissionId { get; set; }
    public bool IsEnabled { get; set; }
}
