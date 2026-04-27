using MediatR;

namespace UserManagementService.Application.Commands.Users;

public class DeactivateUserCommand : IRequest<bool>
{
    public string Id { get; set; } = default!;
}