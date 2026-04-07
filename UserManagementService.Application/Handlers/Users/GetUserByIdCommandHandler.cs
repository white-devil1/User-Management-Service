using MediatR;
using UserManagementService.Application.Commands.Users;
using UserManagementService.Application.Common.Exceptions;
using UserManagementService.Application.DTOs.Users;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.Users;

public class GetUserByIdCommandHandler
    : IRequestHandler<GetUserByIdCommand, UserResponse>
{
    private readonly IUserService _userService;

    public GetUserByIdCommandHandler(IUserService userService)
        => _userService = userService;

    public async Task<UserResponse> Handle(
        GetUserByIdCommand request, CancellationToken cancellationToken)
    {
        var user = await _userService.GetUserByIdWithRolesAsync(
            request.Id, cancellationToken);

        if (user == null)
            throw new NotFoundException("User", request.Id);

        // Tenant guard — non-SuperAdmin cannot see users from other tenants
        if (!request.IsSuperAdmin &&
            user.TenantId.ToString() != request.CallerTenantId)
            throw new UnauthorizedException(
                "You do not have access to this user.");

        // Non-SuperAdmin cannot see soft-deleted users
        if (!request.IsSuperAdmin && user.IsDeleted)
            throw new NotFoundException("User", request.Id);

        return user;
    }
}

