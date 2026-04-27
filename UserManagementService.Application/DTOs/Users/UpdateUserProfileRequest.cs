using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Application.DTOs.Users;

public class UpdateUserProfileRequest
{
    [StringLength(100)]
    public string? FirstName { get; set; }

    [StringLength(100)]
    public string? LastName { get; set; }
}
