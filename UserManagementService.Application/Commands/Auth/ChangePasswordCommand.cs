using MediatR;

namespace UserManagementService.Application.Commands.Auth;

public record ChangePasswordCommand(
    string UserId,
    string CurrentPassword,
    string NewPassword
) : IRequest<bool>;