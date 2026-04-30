using MediatR;
using UserManagementService.Application.DTOs.Subscriptions;

namespace UserManagementService.Application.Commands.Subscriptions;

public class ToggleSubscriptionStatusCommand : IRequest<SubscriptionDto>
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
    public string UpdatedBy { get; set; } = default!;
}
