using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Application.DTOs.AppActions;

public class CreateAppActionRequest
{
    [Required]
    public Guid PageId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = default!;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(20)]
    public string Type { get; set; } = "Button";  // Button, Menu, API

    [Range(1, 999)]
    public int DisplayOrder { get; set; } = 1;
}