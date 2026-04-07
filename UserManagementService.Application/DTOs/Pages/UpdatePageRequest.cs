using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Application.DTOs.Pages;

public class UpdatePageRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = default!;

    [StringLength(500)]
    public string? Description { get; set; }

    [Range(1, 999)]
    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }
}