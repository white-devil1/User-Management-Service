namespace UserManagementService.Application.Events;

public class UserUpdatedEvent
{
    public string UserId { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public Guid TenantId { get; set; }
    public Guid? BranchId { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public List<string> Roles { get; set; } = new();
    public DateTime UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
