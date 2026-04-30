using MediatR;
using UserManagementService.Application.DTOs.Subscriptions;

namespace UserManagementService.Application.Commands.Subscriptions;

public class ListSubscriptionsCommand : IRequest<SubscriptionListResponse>
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public bool IncludeDeleted { get; set; } = false;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "Name";
    public string SortOrder { get; set; } = "asc";
}
