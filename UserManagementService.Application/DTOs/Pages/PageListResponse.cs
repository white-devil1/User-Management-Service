namespace UserManagementService.Application.DTOs.Pages;

public class PageListResponse
{
    public List<PageDto> Pages { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}