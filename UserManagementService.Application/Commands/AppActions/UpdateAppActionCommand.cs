using MediatR;
using UserManagementService.Application.DTOs.AppActions;

namespace UserManagementService.Application.Commands.AppActions;

public class UpdateAppActionCommand : IRequest<AppActionDto>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string Type { get; set; } = "Button";
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public string UpdatedBy { get; set; } = default!;  // UserId
}