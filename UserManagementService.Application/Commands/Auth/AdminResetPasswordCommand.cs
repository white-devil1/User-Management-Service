using MediatR;

namespace UserManagementService.Application.Commands.Auth;

public class AdminResetPasswordCommand : IRequest<bool>
{
    public string UserId { get; set; } = default!;
    public string? NewPassword { get; set; } // Optional - if null, system generates
    public bool ForceChangeOnLogin { get; set; } = true;
    public string? ResetByUserId { get; set; } // Who performed the reset
}