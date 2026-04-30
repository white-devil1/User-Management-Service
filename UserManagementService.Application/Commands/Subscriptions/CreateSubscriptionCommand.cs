using MediatR;
using UserManagementService.Application.DTOs.Subscriptions;

namespace UserManagementService.Application.Commands.Subscriptions;

public class CreateSubscriptionCommand : IRequest<SubscriptionDto>
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public bool IsActive { get; set; } = true;
    public List<Guid> AppIds { get; set; } = new();
    public string CreatedBy { get; set; } = default!;
}
