using MediatR;

namespace UserManagementService.Application.Commands.Roles;

public class DeleteRoleCommand : IRequest<bool>
{
    public string Id { get; set; } = default!;
    public string CallerUserId { get; set; } = default!;
    public bool CallerIsSuperAdmin { get; set; }
    public Guid CallerTenantId { get; set; }
}
