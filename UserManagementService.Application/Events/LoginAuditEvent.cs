namespace UserManagementService.Application.Events;

// Published to RabbitMQ exchange: logging.loginaudit
// EventType: Login=0  Logout=1
// DeviceType: Unknown=0  Desktop=1  Mobile=2  Tablet=3
public class LoginAuditEvent
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int EventType { get; set; }
    public string? UserId { get; set; }
    public string Email { get; set; } = default!;
    public Guid? TenantId { get; set; }
    public Guid? BranchId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public int DeviceType { get; set; } = 0;
}
