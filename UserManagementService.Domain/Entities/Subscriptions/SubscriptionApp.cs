using UserManagementService.Domain.Entities.RBAC;

namespace UserManagementService.Domain.Entities.Subscriptions;

public class SubscriptionApp
{
    public Guid SubscriptionId { get; set; }
    public Guid AppId { get; set; }

    public virtual Subscription Subscription { get; set; } = default!;
    public virtual App App { get; set; } = default!;
}
