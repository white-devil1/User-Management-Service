using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Application.DTOs.Users;

public class UpdateUserRequest
{
    [EmailAddress]
    [StringLength(256)]
    public string? Email { get; set; }

    [StringLength(256)]
    public string? UserName { get; set; }

    [StringLength(100)]
    public string? FirstName { get; set; }

    [StringLength(100)]
    public string? LastName { get; set; }

    public bool? IsActive { get; set; }
    public Guid? BranchId { get; set; }
    public List<string>? RoleIds { get; set; }
}
