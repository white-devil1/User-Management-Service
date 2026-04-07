using MediatR;
using UserManagementService.Application.DTOs.Apps;

namespace UserManagementService.Application.Commands.Apps;

public class ToggleAppStatusCommand : IRequest<AppDto>
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
    public string UpdatedBy { get; set; } = default!;  // UserId
}