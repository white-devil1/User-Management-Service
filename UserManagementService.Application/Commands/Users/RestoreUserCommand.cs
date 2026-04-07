using MediatR;

namespace UserManagementService.Application.Commands.Users;

public class RestoreUserCommand : IRequest<bool>
{
    public string Id { get; set; } = default!;
}