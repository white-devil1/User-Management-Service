namespace UserManagementService.Application.DTOs.Users;

public class UserListRequest
{
    public string? Search { get; set; }
    public List<string> Status { get; set; } = new() { "active" }; // active, deactivated, deleted
    public Guid? TenantId { get; set; }
    public Guid? BranchId { get; set; }
    public string? RoleId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "createdAt";
    public string SortOrder { get; set; } = "desc"; // asc or desc
}