using MediatR;

namespace UserManagementService.Application.Commands.Users;

public class ToggleUserStatusCommand : IRequest<bool>
{
    public string Id { get; set; } = default!;
    public bool IsActive { get; set; }

    // ✅ AUDIT FIELD
    public string? UpdatedBy { get; set; }
}