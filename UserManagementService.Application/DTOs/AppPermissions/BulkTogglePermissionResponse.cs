namespace UserManagementService.Application.DTOs.AppPermissions;

public class BulkTogglePermissionResponse
{
    public int UpdatedCount { get; set; }
    public List<TogglePermissionResponseDto> Results { get; set; } = new();
}
