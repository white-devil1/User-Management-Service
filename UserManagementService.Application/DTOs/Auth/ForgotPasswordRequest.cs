using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Application.DTOs.Auth;

public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = default!;
}
