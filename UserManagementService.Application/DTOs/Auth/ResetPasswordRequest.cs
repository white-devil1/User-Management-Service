namespace UserManagementService.Application.DTOs.Auth;

public class ResetPasswordRequest
{
    public string Email { get; set; } = default!;
    public string OTP { get; set; } = default!;
    public string NewPassword { get; set; } = default!;
    public string ConfirmPassword { get; set; } = default!;
}