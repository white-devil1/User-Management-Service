using MediatR;

namespace UserManagementService.Application.Commands.Auth;

public class ForgotPasswordCommand : IRequest<bool>
{
    public string Email { get; set; } = default!;
    public string? IPAddress { get; set; }
    public string? UserAgent { get; set; }
}