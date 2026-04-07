using MediatR;
using UserManagementService.Application.DTOs.Apps;

namespace UserManagementService.Application.Commands.Apps;

public class UpdateAppCommand : IRequest<AppDto>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public string UpdatedBy { get; set; } = default!;  // UserId
}