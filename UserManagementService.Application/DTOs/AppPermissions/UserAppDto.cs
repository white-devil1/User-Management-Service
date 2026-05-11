namespace UserManagementService.Application.DTOs.AppPermissions;

public class UserAppDto
{
    public Guid AppId { get; set; }
    public string AppName { get; set; } = default!;
    public string Code { get; set; } = default!;
}
