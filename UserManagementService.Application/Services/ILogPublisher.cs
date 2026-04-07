using UserManagementService.Application.Events;

namespace UserManagementService.Application.Services;

public interface ILogPublisher
{
    // All 3 methods are fire-and-forget — never throw, never block the caller
    void PublishError(ErrorLogEvent evt);
    void PublishActivity(ActivityLogEvent evt);
    void PublishLoginAudit(LoginAuditEvent evt);
}
