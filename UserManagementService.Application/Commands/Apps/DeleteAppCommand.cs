using MediatR;

namespace UserManagementService.Application.Commands.Apps;

public class DeleteAppCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public string DeletedBy { get; set; } = default!;  // UserId
}