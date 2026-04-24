using UserManagementService.Application.DTOs.Roles;

namespace UserManagementService.Application.Services;

public interface IRoleService
{
    Task<RoleResponse> CreateRoleAsync(
        string name, string? description,
        string callerUserId, bool callerIsSuperAdmin,
        Guid callerTenantId, Guid? callerBranchId,
        CancellationToken ct = default);

    Task<RoleResponse> UpdateRoleAsync(
        string roleId, string name, string? description,
        string callerUserId, bool callerIsSuperAdmin,
        Guid callerTenantId, CancellationToken ct = default);

    Task<bool> DeleteRoleAsync(
        string roleId, string callerUserId,
        bool callerIsSuperAdmin, Guid callerTenantId,
        CancellationToken ct = default);

    Task<RoleResponse> GetRoleByIdAsync(
        string roleId, bool callerIsSuperAdmin,
        Guid callerTenantId, CancellationToken ct = default);

    Task<RoleListResponse> ListRolesAsync(
        string? search, bool? isDeleted,
        bool callerIsSuperAdmin, Guid callerTenantId,
        int page, int pageSize, string sortBy, string sortOrder,
        CancellationToken ct = default);

    Task<RoleResponse> AssignPermissionsAsync(
        string roleId, List<Guid> permissionIds,
        string callerUserId, bool callerIsSuperAdmin,
        Guid callerTenantId, CancellationToken ct = default);

    Task<RolePermissionsGrouped> GetAvailablePermissionsAsync(
        string callerUserId, bool callerIsSuperAdmin,
        CancellationToken ct = default);
}
