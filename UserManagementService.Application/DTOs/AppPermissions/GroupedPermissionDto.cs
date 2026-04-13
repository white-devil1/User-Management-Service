namespace UserManagementService.Application.DTOs.AppPermissions;

public class GroupedPermissionDto
{
    public Guid Id { get; set; }
    public string ActionName { get; set; } = default!;
    public bool IsEnabled { get; set; }
}
