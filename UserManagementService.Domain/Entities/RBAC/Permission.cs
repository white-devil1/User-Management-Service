using UserManagementService.Domain.Common;

namespace UserManagementService.Domain.Entities.RBAC;

public class Permission : BaseEntity
{
    // Foreign Keys
    public Guid AppId { get; set; }
    public Guid PageId { get; set; }
    public Guid ActionId { get; set; }

    // Properties
    public string PermissionCode { get; set; } = default!;  // e.g., "USERMAN_USERPRO_CRE"
    public string Name { get; set; } = default!;             // e.g., "User Management - User Profiles - Create"
    public bool IsEnabled { get; set; } = false;             // ⚠️ Default: FALSE (inactive)

    // Navigation Properties
    public virtual App? App { get; set; }
    public virtual Page? Page { get; set; }
    public virtual AppAction? Action { get; set; }
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}