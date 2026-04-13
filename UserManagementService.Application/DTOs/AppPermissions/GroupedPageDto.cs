namespace UserManagementService.Application.DTOs.AppPermissions;

public class GroupedPageDto
{
    public Guid PageId { get; set; }
    public string PageName { get; set; } = default!;
    public List<GroupedPermissionDto> Permissions { get; set; } = new();
}
