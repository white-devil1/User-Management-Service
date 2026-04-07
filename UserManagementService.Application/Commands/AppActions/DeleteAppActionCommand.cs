using MediatR;

namespace UserManagementService.Application.Commands.AppActions;

public class DeleteAppActionCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public string DeletedBy { get; set; } = default!;  // UserId
}