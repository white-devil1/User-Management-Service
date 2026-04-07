using MediatR;

namespace UserManagementService.Application.Commands.Pages;

public class DeletePageCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public string DeletedBy { get; set; } = default!;  // UserId
}