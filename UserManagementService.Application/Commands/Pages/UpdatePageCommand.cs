using MediatR;
using UserManagementService.Application.DTOs.Pages;

namespace UserManagementService.Application.Commands.Pages;

public class UpdatePageCommand : IRequest<PageDto>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public string UpdatedBy { get; set; } = default!;  // UserId
}