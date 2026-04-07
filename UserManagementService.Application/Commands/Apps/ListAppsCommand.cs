using MediatR;
using UserManagementService.Application.DTOs.Apps;

namespace UserManagementService.Application.Commands.Apps;

public class ListAppsCommand : IRequest<AppListResponse>
{
    public string? Search { get; set; }
    public string? Code { get; set; }
    public bool? IsActive { get; set; }
    public bool IncludeDeleted { get; set; } = false;  // Super Admin only
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "DisplayOrder";
    public string SortOrder { get; set; } = "asc";
}