using MediatR;

namespace UserManagementService.Application.Commands.Users;

public class DeleteUserCommand : IRequest<bool>
{
    public string Id { get; set; } = default!;

    // ✅ AUDIT FIELD (Added)
    public string? DeletedBy { get; set; }
}