using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Application.DTOs.AppActions;

public class UpdateAppActionRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = default!;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(20)]
    public string Type { get; set; } = "Button";

    [Range(1, 999)]
    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }
}