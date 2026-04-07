using MediatR;
using UserManagementService.Application.DTOs.Apps;

namespace UserManagementService.Application.Commands.Apps;

public class CreateAppCommand : IRequest<AppDto>
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public string CreatedBy { get; set; } = default!;  // UserId
}