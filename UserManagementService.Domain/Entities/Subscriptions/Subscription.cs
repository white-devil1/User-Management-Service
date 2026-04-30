using UserManagementService.Domain.Common;

namespace UserManagementService.Domain.Entities.Subscriptions;

public class Subscription : BaseEntity
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; } // HTML allowed
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD"; // ISO-4217 code
    public bool IsActive { get; set; } = true;

    public virtual ICollection<SubscriptionApp> SubscriptionApps { get; set; }
        = new List<SubscriptionApp>();
}
