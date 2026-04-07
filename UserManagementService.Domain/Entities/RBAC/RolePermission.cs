using UserManagementService.Domain.Common;
using UserManagementService.Domain.Entities.Identity;

namespace UserManagementService.Domain.Entities.RBAC;

public class RolePermission : BaseEntity
{
    // Foreign Keys
    public string RoleId { get; set; } = default!;      // ASP.NET Identity Role Id (string)
    public Guid PermissionId { get; set; }              // Permission Id (Guid)

    // Assignment Tracking
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public string? AssignedBy { get; set; }             // User who assigned this permission

    // Navigation Properties
    public virtual ApplicationRole Role { get; set; } = default!;
    public virtual Permission Permission { get; set; } = default!;
}