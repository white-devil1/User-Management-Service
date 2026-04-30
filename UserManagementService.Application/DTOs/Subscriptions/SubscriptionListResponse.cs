namespace UserManagementService.Application.DTOs.Subscriptions;

public class SubscriptionListResponse
{
    public List<SubscriptionDto> Subscriptions { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
