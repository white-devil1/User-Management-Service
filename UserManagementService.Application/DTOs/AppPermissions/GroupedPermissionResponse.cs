namespace UserManagementService.Application.DTOs.AppPermissions;

public class GroupedPermissionResponse
{
    public List<GroupedAppDto> Apps { get; set; } = new();
}
