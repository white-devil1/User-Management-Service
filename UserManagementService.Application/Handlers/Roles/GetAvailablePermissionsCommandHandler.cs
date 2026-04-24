using MediatR;
using UserManagementService.Application.Commands.Roles;
using UserManagementService.Application.DTOs.Roles;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.Roles;

public class GetAvailablePermissionsCommandHandler
    : IRequestHandler<GetAvailablePermissionsCommand, RolePermissionsGrouped>
{
    private readonly IRoleService _roleService;

    public GetAvailablePermissionsCommandHandler(IRoleService roleService)
        => _roleService = roleService;

    public Task<RolePermissionsGrouped> Handle(
        GetAvailablePermissionsCommand request,
        CancellationToken cancellationToken)
        => _roleService.GetAvailablePermissionsAsync(
            request.CallerUserId,
            request.CallerIsSuperAdmin,
            cancellationToken);
}
