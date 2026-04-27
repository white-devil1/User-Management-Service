using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Application.DTOs.Roles;

public class AssignPermissionsRequest
{
    [Required]
    public List<Guid> PermissionIds { get; set; } = new();
}
