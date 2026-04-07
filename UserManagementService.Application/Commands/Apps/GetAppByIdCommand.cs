using MediatR;
using UserManagementService.Application.DTOs.Apps;

namespace UserManagementService.Application.Commands.Apps;

public class GetAppByIdCommand : IRequest<AppDto>
{
    public Guid Id { get; set; }
}