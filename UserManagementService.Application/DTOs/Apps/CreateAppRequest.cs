using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Application.DTOs.Apps;

public class CreateAppRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = default!;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(200)]
    public string? Icon { get; set; }

    [Range(1, 999)]
    public int DisplayOrder { get; set; } = 1;
}