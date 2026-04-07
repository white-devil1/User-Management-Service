using MediatR;
using UserManagementService.Application.DTOs.Pages;

namespace UserManagementService.Application.Commands.Pages;

public class TogglePageStatusCommand : IRequest<PageDto>
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
    public string UpdatedBy { get; set; } = default!;  // UserId
}