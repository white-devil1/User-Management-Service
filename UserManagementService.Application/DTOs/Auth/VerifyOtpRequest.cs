namespace UserManagementService.Application.DTOs.Auth;

public class VerifyOtpRequest
{
    public string Email { get; set; } = default!;
    public string OTP { get; set; } = default!;
    public string? Purpose { get; set; } = "ForgotPassword";
}