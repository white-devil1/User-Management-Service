using MediatR;

namespace UserManagementService.Application.Commands.Subscriptions;

public class DeleteSubscriptionCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public string DeletedBy { get; set; } = default!;
}
