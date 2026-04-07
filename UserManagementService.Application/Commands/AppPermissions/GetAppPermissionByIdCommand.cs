using MediatR;
using UserManagementService.Application.DTOs.AppPermissions;

namespace UserManagementService.Application.Commands.AppPermissions;

public class GetAppPermissionByIdCommand : IRequest<AppPermissionDto>
{
    public Guid Id { get; set; }
}