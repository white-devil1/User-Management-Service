using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Application.DTOs.Pages;

public class CreatePageRequest
{
    [Required]
    public Guid AppId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = default!;

    [StringLength(500)]
    public string? Description { get; set; }

    [Range(1, 999)]
    public int DisplayOrder { get; set; } = 1;
}