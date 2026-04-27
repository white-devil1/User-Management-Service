using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Application.DTOs.Users;

public class CreateUserRequest
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = default!;

    [Required]
    [StringLength(256)]
    public string UserName { get; set; } = default!;

    [StringLength(100)]
    public string? FirstName { get; set; }

    [StringLength(100)]
    public string? LastName { get; set; }

    [Required]
    public Guid TenantId { get; set; }

    public Guid? BranchId { get; set; }
    public bool IsActive { get; set; } = true;
    public List<string> RoleIds { get; set; } = new();
}
