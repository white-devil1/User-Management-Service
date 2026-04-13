namespace UserManagementService.Application.DTOs.AppActions;

public class AppActionDto
{
    public Guid Id { get; set; }
    public Guid PageId { get; set; }
    public string? PageName { get; set; }
    public Guid? AppId { get; set; }
    public string? AppName { get; set; }
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string Type { get; set; } = "Button";  // Button, Menu, API
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }

    // ✅ Audit Fields
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // ✅ Soft Delete Fields
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}