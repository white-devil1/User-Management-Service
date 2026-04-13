using MediatR;
using UserManagementService.Application.DTOs.AppPermissions;

namespace UserManagementService.Application.Commands.AppPermissions;

public class TogglePermissionStatusCommand : IRequest<TogglePermissionResponseDto>
{
    public Guid Id { get; set; }
    public bool IsEnabled { get; set; }
    public string UpdatedBy { get; set; } = default!;
}
