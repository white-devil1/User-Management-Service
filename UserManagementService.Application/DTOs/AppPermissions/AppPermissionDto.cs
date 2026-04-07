namespace UserManagementService.Application.DTOs.AppPermissions;

public class AppPermissionDto
{
    public Guid Id { get; set; }
    public Guid AppId { get; set; }
    public Guid PageId { get; set; }
    public Guid ActionId { get; set; }
    public string PermissionCode { get; set; } = default!;
    public string Name { get; set; } = default!;
    public bool IsEnabled { get; set; }

    // Audit Fields
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Soft Delete Fields
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}