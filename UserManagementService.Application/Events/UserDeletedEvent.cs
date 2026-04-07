namespace UserManagementService.Application.Events;

public class UserDeletedEvent
{
    public string UserId { get; set; } = default!;
    public Guid TenantId { get; set; }
    public Guid? BranchId { get; set; }
    public bool IsDeleted { get; set; } = true;
    public bool IsActive { get; set; } = false;
    public DateTime DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
