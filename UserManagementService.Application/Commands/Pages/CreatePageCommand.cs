using MediatR;
using UserManagementService.Application.DTOs.Pages;

namespace UserManagementService.Application.Commands.Pages;

public class CreatePageCommand : IRequest<PageDto>
{
    public Guid AppId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public string CreatedBy { get; set; } = default!;  // UserId
}