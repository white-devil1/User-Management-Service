using UserManagementService.Application.DTOs.Subscriptions;

namespace UserManagementService.Application.Services;

public interface ISubscriptionService
{
    Task<SubscriptionListResponse> GetSubscriptionsAsync(
        string? search,
        bool? isActive,
        bool includeDeleted,
        int page,
        int pageSize,
        string sortBy,
        string sortOrder,
        CancellationToken cancellationToken = default);

    Task<SubscriptionDto> GetSubscriptionByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<SubscriptionDto> CreateSubscriptionAsync(
        CreateSubscriptionRequest request, string createdBy, CancellationToken cancellationToken = default);

    Task<SubscriptionDto> UpdateSubscriptionAsync(
        Guid id, UpdateSubscriptionRequest request, string updatedBy, CancellationToken cancellationToken = default);

    Task<bool> DeleteSubscriptionAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default);

    Task<SubscriptionDto> ToggleSubscriptionStatusAsync(
        Guid id, bool isActive, string updatedBy, CancellationToken cancellationToken = default);
}
