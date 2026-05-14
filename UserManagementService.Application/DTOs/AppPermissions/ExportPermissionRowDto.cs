namespace UserManagementService.Application.DTOs.AppPermissions;

public class ExportPermissionRowDto
{
    public string AppName       { get; set; } = string.Empty;
    public string AppCode       { get; set; } = string.Empty;
    public string PageName      { get; set; } = string.Empty;
    public string PageCode      { get; set; } = string.Empty;
    public string ActionName    { get; set; } = string.Empty;
    public string ActionCode    { get; set; } = string.Empty;
    public string PermissionName { get; set; } = string.Empty;
    public string PermissionCode { get; set; } = string.Empty;
}
