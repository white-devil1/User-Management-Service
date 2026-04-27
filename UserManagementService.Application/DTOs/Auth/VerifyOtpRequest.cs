using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Application.DTOs.Auth;

public class VerifyOtpRequest
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = default!;

    [Required]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be exactly 6 characters.")]
    public string OTP { get; set; } = default!;

    public string? Purpose { get; set; } = "ForgotPassword";
}
