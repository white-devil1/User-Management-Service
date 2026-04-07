using MediatR;
using UserManagementService.Application.Commands.Roles;
using UserManagementService.Application.DTOs.Roles;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.Roles;

public class ListRolesCommandHandler
    : IRequestHandler<ListRolesCommand, RoleListResponse>
{
    private readonly IRoleService _roleService;
    public ListRolesCommandHandler(IRoleService roleService)
        => _roleService = roleService;

    public Task<RoleListResponse> Handle(
        ListRolesCommand request, CancellationToken cancellationToken)
        => _roleService.ListRolesAsync(
            request.Search, request.IsDeleted,
            request.CallerIsSuperAdmin, request.CallerTenantId,
            request.Page, request.PageSize,
            request.SortBy, request.SortOrder, cancellationToken);
}
