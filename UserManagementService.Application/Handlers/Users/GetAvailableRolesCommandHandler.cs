using MediatR;
using UserManagementService.Application.Commands.Users;
using UserManagementService.Application.DTOs.Users;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.Users;

public class GetAvailableRolesCommandHandler
    : IRequestHandler<GetAvailableRolesCommand, List<RoleDto>>
{
    private readonly IUserService _userService;

    public GetAvailableRolesCommandHandler(IUserService userService)
        => _userService = userService;

    public async Task<List<RoleDto>> Handle(
        GetAvailableRolesCommand request, CancellationToken cancellationToken)
    {
        return await _userService.GetAvailableRolesAsync(
            request.IsSuperAdmin, request.CallerTenantId, cancellationToken);
    }
}
