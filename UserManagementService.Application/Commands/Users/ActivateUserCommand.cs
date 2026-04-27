using MediatR;

namespace UserManagementService.Application.Commands.Users;

public class ActivateUserCommand : IRequest<bool>
{
    public string Id { get; set; } = default!;
}