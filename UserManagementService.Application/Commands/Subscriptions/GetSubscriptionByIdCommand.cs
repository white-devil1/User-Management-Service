using MediatR;
using UserManagementService.Application.DTOs.Subscriptions;

namespace UserManagementService.Application.Commands.Subscriptions;

public class GetSubscriptionByIdCommand : IRequest<SubscriptionDto>
{
    public Guid Id { get; set; }
}
