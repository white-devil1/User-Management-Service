namespace UserManagementService.Application.DTOs.Users;

public class RoleDto
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public Guid TenantId { get; set; }
    public Guid? BranchId { get; set; }
    public string? Description { get; set; }
    public string Scope { get; set; } = default!;
    public bool IsDefault { get; set; }
}
