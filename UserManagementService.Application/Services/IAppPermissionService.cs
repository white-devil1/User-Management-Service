using UserManagementService.Application.DTOs.AppPermissions;

namespace UserManagementService.Application.Services;

public interface IAppPermissionService
{
    // Query Operations
    Task<AppPermissionListResponse> GetPermissionsAsync(
        Guid? appId,
        Guid? pageId,
        Guid? actionId,
        bool? isEnabled,
        bool includeDeleted,
        int page,
        int pageSize,
        string sortBy,
        string sortOrder,
        CancellationToken cancellationToken = default);

    Task<AppPermissionDto> GetPermissionByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Command Operations
    Task<AppPermissionDto> TogglePermissionAsync(
        Guid id,
        bool isEnabled,
        string updatedBy,
        CancellationToken cancellationToken = default);
}