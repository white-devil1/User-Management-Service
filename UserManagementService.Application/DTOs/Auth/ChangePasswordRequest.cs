using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Application.DTOs.Auth;

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = default!;

    [Required]
    [MinLength(8)]
    public string NewPassword { get; set; } = default!;
}
