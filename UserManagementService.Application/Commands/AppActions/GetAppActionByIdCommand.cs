using MediatR;
using UserManagementService.Application.DTOs.AppActions;

namespace UserManagementService.Application.Commands.AppActions;

public class GetAppActionByIdCommand : IRequest<AppActionDto>
{
    public Guid Id { get; set; }
}