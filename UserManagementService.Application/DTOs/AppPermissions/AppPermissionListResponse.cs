namespace UserManagementService.Application.DTOs.AppPermissions;

public class AppPermissionListResponse
{
    public List<AppPermissionDto> Permissions { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}