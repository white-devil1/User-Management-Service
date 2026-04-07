using UserManagementService.Domain.Common;

namespace UserManagementService.Domain.Entities.RBAC;

public class AppAction : BaseEntity
{
    // Foreign Key
    public Guid PageId { get; set; }

    // Properties
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;  // Auto-generated from Name (e.g., "CREATE")
    public string Type { get; set; } = "Button";  // Button, Menu, API
    public string? Description { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public virtual Page? Page { get; set; }
    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}