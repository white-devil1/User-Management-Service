namespace UserManagementService.Application.DTOs.Roles;

public class AssignPermissionsRequest
{
    // List of Permission IDs (Guids) to assign to this role.
    // The handler will validate every ID is within the caller's own permission set.
    public List<Guid> PermissionIds { get; set; } = new();
}
