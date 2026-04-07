namespace UserManagementService.Application.Events;

public class UserStatusChangedEvent
{
    public string UserId { get; set; } = default!;
    public Guid TenantId { get; set; }
    public Guid? BranchId { get; set; }
    public bool IsActive { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
