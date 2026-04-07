using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Application.DTOs.Roles;

public class UpdateRoleRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = default!;

    [StringLength(500)]
    public string? Description { get; set; }
}
