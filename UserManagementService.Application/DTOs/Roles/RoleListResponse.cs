namespace UserManagementService.Application.DTOs.Roles;

public class RoleListResponse
{
    public List<RoleResponse> Roles { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
