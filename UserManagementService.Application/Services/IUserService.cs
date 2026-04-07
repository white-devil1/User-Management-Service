using UserManagementService.Application.DTOs.Users;
using UserManagementService.Domain.Entities.Identity;

namespace UserManagementService.Application.Services;

public interface IUserService
{
    Task<UserListResponse> GetUsersAsync(
        string? search, List<string> status, Guid? tenantId,
        Guid? branchId, string? roleId, int page, int pageSize,
        string sortBy, string sortOrder, CancellationToken cancellationToken);

    Task<UserResponse?> GetUserByIdWithRolesAsync(
        string userId, CancellationToken cancellationToken);

    Task<List<RoleDto>> GetAvailableRolesAsync(
        bool isSuperAdmin, string? tenantId, CancellationToken cancellationToken);
}
