namespace UserManagementService.Application.DTOs.AppPermissions;

public class GroupedAppDto
{
    public Guid AppId { get; set; }
    public string AppName { get; set; } = default!;
    public List<GroupedPageDto> Pages { get; set; } = new();
}
