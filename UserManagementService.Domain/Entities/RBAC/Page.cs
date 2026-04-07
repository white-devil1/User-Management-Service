using UserManagementService.Domain.Common;

namespace UserManagementService.Domain.Entities.RBAC;

public class Page : BaseEntity
{
    // Foreign Key
    public Guid AppId { get; set; }

    // Properties
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;  // Auto-generated from Name (e.g., "USERPRO")
    public string? Description { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public virtual App? App { get; set; }
    public virtual ICollection<AppAction> Actions { get; set; } = new List<AppAction>();
    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}