namespace UserManagementService.Application.DTOs.Users;

public class UserResponse
{
    public string Id { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ProfileImagePath { get; set; }
    public string? ProfileThumbPath { get; set; }
    public Guid TenantId { get; set; }
    public Guid? BranchId { get; set; }
    public bool IsSuperAdmin { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsTemporaryPassword { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }  // ← ADD THIS
    public DateTime UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }  // ← ADD THIS
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    public List<string> Roles { get; set; } = new();
}