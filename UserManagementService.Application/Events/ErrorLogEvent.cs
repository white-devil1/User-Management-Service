namespace UserManagementService.Application.Events;

// Published to RabbitMQ exchange: logging.error
// int values match LoggingService.Domain.Enums
public class ErrorLogEvent
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int Severity { get; set; } = 3;   // Error=3
    public int Source { get; set; } = 0;      // Backend=0
    public int Category { get; set; } = 0;    // ServerError=0
    public string ServiceName { get; set; } = "UserManagementService";
    public string? Environment { get; set; }
    public string Message { get; set; } = default!;
    public string? StackTrace { get; set; }
    public string? RequestPath { get; set; }
    public string? RequestMethod { get; set; }
    public int? StatusCode { get; set; }
    public string? UserId { get; set; }
    public string? UserEmail { get; set; }
    public Guid? TenantId { get; set; }
    public Guid? BranchId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? AdditionalData { get; set; }
}
