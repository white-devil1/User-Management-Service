using UserManagementService.Application.DTOs.AppActions;

namespace UserManagementService.Application.Services;

public interface IAppActionService
{
    // Query Operations
    Task<AppActionListResponse> GetActionsAsync(
        Guid? appId,    // ✅ NEW: Optional filter by App
        Guid? pageId,   // ✅ CHANGED: Nullable Guid
        string? search,
        bool? isActive,
        bool includeDeleted,
        int page,
        int pageSize,
        string sortBy,
        string sortOrder,
        CancellationToken cancellationToken = default);

    Task<AppActionDto> GetActionByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Command Operations (with audit user tracking)
    Task<AppActionDto> CreateActionAsync(
        CreateAppActionRequest request,
        string createdBy,
        CancellationToken cancellationToken = default);

    Task<AppActionDto> UpdateActionAsync(
        Guid id,
        UpdateAppActionRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteActionAsync(
        Guid id,
        string deletedBy,
        CancellationToken cancellationToken = default);

    Task<AppActionDto> ToggleActionStatusAsync(
        Guid id,
        bool isActive,
        string updatedBy,
        CancellationToken cancellationToken = default);
}