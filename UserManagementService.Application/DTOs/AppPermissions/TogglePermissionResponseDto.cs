namespace UserManagementService.Application.DTOs.AppPermissions;

public class TogglePermissionResponseDto
{
    public Guid Id { get; set; }
    public string ActionName { get; set; } = default!;
    public bool IsEnabled { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
