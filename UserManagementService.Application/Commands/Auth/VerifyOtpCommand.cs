using MediatR;

namespace UserManagementService.Application.Commands.Auth;

public class VerifyOtpCommand : IRequest<bool>
{
    public string Email { get; set; } = default!;
    public string OTP { get; set; } = default!;
    public string Purpose { get; set; } = default!; // "ForgotPassword" or "AdminReset"
}