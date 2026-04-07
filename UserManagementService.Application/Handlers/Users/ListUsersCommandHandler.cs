using MediatR;
using UserManagementService.Application.Commands.Users;
using UserManagementService.Application.DTOs.Users;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.Users;

public class ListUsersCommandHandler : IRequestHandler<ListUsersCommand, UserListResponse>
{
    private readonly IUserService _userService;

    public ListUsersCommandHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<UserListResponse> Handle(ListUsersCommand request, CancellationToken cancellationToken)
    {
        return await _userService.GetUsersAsync(
            request.Search,
            request.Status,
            request.TenantId,
            request.BranchId,
            request.RoleId,
            request.Page,
            request.PageSize,
            request.SortBy,
            request.SortOrder,
            cancellationToken
        );
    }
}