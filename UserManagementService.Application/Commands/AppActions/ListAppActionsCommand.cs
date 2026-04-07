using MediatR;
using UserManagementService.Application.DTOs.AppActions;

namespace UserManagementService.Application.Commands.AppActions;

public class ListAppActionsCommand : IRequest<AppActionListResponse>
{
    // ✅ NEW: Optional filter by App
    public Guid? AppId { get; set; }

    // ✅ CHANGED: Nullable Guid - optional filter
    public Guid? PageId { get; set; }

    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public bool IncludeDeleted { get; set; } = false;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "DisplayOrder";
    public string SortOrder { get; set; } = "asc";
}