using MediatR;
using UserManagementService.Application.DTOs.Roles;

namespace UserManagementService.Application.Commands.Roles;

public class ListRolesCommand : IRequest<RoleListResponse>
{
    public string? Search { get; set; }
    public bool? IsDeleted { get; set; }
    public bool CallerIsSuperAdmin { get; set; }
    public Guid CallerTenantId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "name";
    public string SortOrder { get; set; } = "asc";
}
