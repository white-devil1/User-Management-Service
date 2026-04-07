using Microsoft.AspNetCore.Identity;
using UserManagementService.Domain.Entities.RBAC;

namespace UserManagementService.Domain.Entities.Identity;

public class ApplicationRole : IdentityRole
{
    // Multi-Tenancy
    public Guid TenantId { get; set; }
    public Guid? BranchId { get; set; }

    // Role Scope (Critical for RBAC)
    public RoleScope Scope { get; set; } = RoleScope.Tenant;
    public string? Description { get; set; }

    // Audit Fields
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // Default role flag
    public bool IsDefault { get; set; } = false;

    // Navigation Properties
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public enum RoleScope
{
    Global,    // Super Admin only (system-wide)
    Tenant,    // Tenant Admin level (within tenant)
    Branch     // Branch Admin level (within branch)
}