using MediatR;
using UserManagementService.Application.DTOs.Pages;

namespace UserManagementService.Application.Commands.Pages;

public class ListPagesCommand : IRequest<PageListResponse>
{
    // ✅ CHANGED: Nullable Guid - optional filter
    public Guid? AppId { get; set; }

    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public bool IncludeDeleted { get; set; } = false;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "DisplayOrder";
    public string SortOrder { get; set; } = "asc";
}