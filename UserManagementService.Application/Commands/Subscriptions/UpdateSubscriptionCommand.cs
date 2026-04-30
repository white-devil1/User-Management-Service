using MediatR;
using UserManagementService.Application.DTOs.Subscriptions;

namespace UserManagementService.Application.Commands.Subscriptions;

public class UpdateSubscriptionCommand : IRequest<SubscriptionDto>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public bool IsActive { get; set; }
    public List<Guid> AppIds { get; set; } = new();
    public string UpdatedBy { get; set; } = default!;
}
