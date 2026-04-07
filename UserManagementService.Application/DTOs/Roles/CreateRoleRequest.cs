using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Application.DTOs.Roles;

public class CreateRoleRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = default!;

    [StringLength(500)]
    public string? Description { get; set; }

    // Caller does not choose scope — it is derived from caller's identity:
    // SuperAdmin -> Global, TenantAdmin -> Tenant, BranchAdmin -> Branch
    // BranchId is only set if caller has a BranchId (Branch scope)
}
