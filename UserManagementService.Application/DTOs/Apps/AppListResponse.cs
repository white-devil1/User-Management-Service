namespace UserManagementService.Application.DTOs.Apps;

public class AppListResponse
{
    public List<AppDto> Apps { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}