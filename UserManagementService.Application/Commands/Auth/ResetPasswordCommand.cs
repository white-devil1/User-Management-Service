using MediatR;

namespace UserManagementService.Application.Commands.Auth;

public class ResetPasswordCommand : IRequest<bool>
{
    public string Email { get; set; } = default!;
    public string OTP { get; set; } = default!;
    public string NewPassword { get; set; } = default!;
    public string ConfirmPassword { get; set; } = default!;
}