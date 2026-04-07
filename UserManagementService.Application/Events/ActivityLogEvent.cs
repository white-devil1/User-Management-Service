namespace UserManagementService.Application.Events;

// Published to RabbitMQ exchange: logging.activity
// ActionType and EntityType int values:
//   User=0  Role=1  App=2  Page=3  AppAction=4  Permission=5  Tenant=6  Branch=7
//   UserCreated=0  UserUpdated=1  UserDeleted=2  UserRestored=3  UserStatusChanged=4
//   RoleCreated=10  RoleUpdated=11  RoleDeleted=12  PermissionsAssigned=13
//   AppCreated=20  AppUpdated=21  AppDeleted=22
//   PageCreated=30  PageUpdated=31  PageDeleted=32
//   ActionCreated=40  ActionUpdated=41  ActionDeleted=42  PermissionToggled=43
//   ForgotPasswordRequested=70  OtpVerified=71
//   PasswordReset=72  PasswordChanged=73  AdminPasswordReset=74
public class ActivityLogEvent
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int ActionType { get; set; }
    public int EntityType { get; set; }
    public string? EntityId { get; set; }
    public string ServiceName { get; set; } = "UserManagementService";
    public string Description { get; set; } = default!;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? UserRole { get; set; }
    public Guid? TenantId { get; set; }
    public Guid? BranchId { get; set; }
    public string? IpAddress { get; set; }
    public string? CorrelationId { get; set; }
}
