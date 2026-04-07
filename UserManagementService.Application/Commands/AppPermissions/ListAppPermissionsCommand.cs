using MediatR;
using UserManagementService.Application.DTOs.AppPermissions;

namespace UserManagementService.Application.Commands.AppPermissions;

public class ListAppPermissionsCommand : IRequest<AppPermissionListResponse>
{
    public Guid? AppId { get; set; }
    public Guid? PageId { get; set; }
    public Guid? ActionId { get; set; }
    public bool? IsEnabled { get; set; }
    public bool IncludeDeleted { get; set; } = false;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "CreatedAt";
    public string SortOrder { get; set; } = "desc";
}