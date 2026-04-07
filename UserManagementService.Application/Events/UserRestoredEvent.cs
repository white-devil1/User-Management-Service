namespace UserManagementService.Application.Events;

public class UserRestoredEvent
{
    public string UserId { get; set; } = default!;
    public Guid TenantId { get; set; }
    public Guid? BranchId { get; set; }
    public bool IsDeleted { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime RestoredAt { get; set; }
}
