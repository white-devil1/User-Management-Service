using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Application.DTOs.Subscriptions;

public class UpdateSubscriptionRequest
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string Name { get; set; } = default!;

    public string? Description { get; set; }

    [Range(0, 9999999999.99)]
    public decimal Price { get; set; }

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string Currency { get; set; } = "USD";

    public bool IsActive { get; set; }

    [Required]
    [MinLength(1)]
    public List<Guid> AppIds { get; set; } = new();
}
