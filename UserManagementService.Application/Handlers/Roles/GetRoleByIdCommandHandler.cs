using MediatR;
using UserManagementService.Application.Commands.Roles;
using UserManagementService.Application.DTOs.Roles;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.Roles;

public class GetRoleByIdCommandHandler
    : IRequestHandler<GetRoleByIdCommand, RoleResponse>
{
    private readonly IRoleService _roleService;
    public GetRoleByIdCommandHandler(IRoleService roleService)
        => _roleService = roleService;

    public Task<RoleResponse> Handle(
        GetRoleByIdCommand request, CancellationToken cancellationToken)
        => _roleService.GetRoleByIdAsync(
            request.Id, request.CallerIsSuperAdmin,
            request.CallerTenantId, cancellationToken);
}
