using MediatR;
using UserManagementService.Application.DTOs.AppActions;

namespace UserManagementService.Application.Commands.AppActions;

public class CreateAppActionCommand : IRequest<AppActionDto>
{
    public Guid PageId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string Type { get; set; } = "Button";
    public int DisplayOrder { get; set; } = 0;
    public string CreatedBy { get; set; } = default!;  // UserId
}