using UserManagementService.Application.DTOs.Apps;

namespace UserManagementService.Application.Services;

public interface IAppService
{
    Task<AppListResponse> GetAppsAsync(
        string? search,
        bool? isActive,
        bool includeDeleted,
        int page,
        int pageSize,
        string sortBy,
        string sortOrder,
        CancellationToken cancellationToken = default);

    Task<AppDto> GetAppByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AppDto> CreateAppAsync(CreateAppRequest request, string createdBy, CancellationToken cancellationToken = default);
    Task<AppDto> UpdateAppAsync(Guid id, UpdateAppRequest request, string UpdatedBy, CancellationToken cancellationToken = default);
    Task<bool> DeleteAppAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default);
    Task<AppDto> ToggleAppStatusAsync(Guid id, bool isActive, string UpdatedBy, CancellationToken cancellationToken = default);
}