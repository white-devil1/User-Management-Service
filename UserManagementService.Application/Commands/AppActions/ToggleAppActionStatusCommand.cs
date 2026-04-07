using MediatR;
using UserManagementService.Application.DTOs.AppActions;

namespace UserManagementService.Application.Commands.AppActions;

public class ToggleAppActionStatusCommand : IRequest<AppActionDto>
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
    public string UpdatedBy { get; set; } = default!;  // UserId
}