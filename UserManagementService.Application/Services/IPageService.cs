using UserManagementService.Application.DTOs.Pages;

namespace UserManagementService.Application.Services;

public interface IPageService
{
    // Query Operations
    Task<PageListResponse> GetPagesAsync(
        Guid? appId,
        string? search,
        bool? isActive,
        bool includeDeleted,
        int page,
        int pageSize,
        string sortBy,
        string sortOrder,
        CancellationToken cancellationToken = default);

    Task<PageDto> GetPageByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Command Operations (with audit user tracking)
    Task<PageDto> CreatePageAsync(
        CreatePageRequest request,
        string createdBy,
        CancellationToken cancellationToken = default);

    Task<PageDto> UpdatePageAsync(
        Guid id,
        UpdatePageRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<bool> DeletePageAsync(
        Guid id,
        string deletedBy,
        CancellationToken cancellationToken = default);

    Task<PageDto> TogglePageStatusAsync(
        Guid id,
        bool isActive,
        string updatedBy,
        CancellationToken cancellationToken = default);
}