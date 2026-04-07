using MediatR;
using UserManagementService.Application.DTOs.Users;

namespace UserManagementService.Application.Commands.Users;

public class GetUserByIdCommand : IRequest<UserResponse>
{
    public string Id { get; set; } = default!;
    public bool IsSuperAdmin { get; set; }
    public string? CallerTenantId { get; set; }
}
