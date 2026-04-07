using UserManagementService.Domain.Common;

namespace UserManagementService.Domain.Entities.RBAC;

public class App : BaseEntity
{
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!; // e.g., "ASSET"
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;


    // Navigation Properties
    public virtual ICollection<Page> Pages { get; set; } = new List<Page>();
    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}