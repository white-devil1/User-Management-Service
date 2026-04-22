namespace UserManagementService.Application.DTOs.Users;

public class UpdateUserRequest
{
    public string? Email { get; set; }
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool? IsActive { get; set; }
    public Guid? BranchId { get; set; }
    public List<string>? RoleIds { get; set; }
}
