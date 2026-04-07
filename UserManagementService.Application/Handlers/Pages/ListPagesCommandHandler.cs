using MediatR;
using UserManagementService.Application.Commands.Pages;
using UserManagementService.Application.DTOs.Pages;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.Pages;

public class ListPagesCommandHandler : IRequestHandler<ListPagesCommand, PageListResponse>
{
    private readonly IPageService _pageService;

    public ListPagesCommandHandler(IPageService pageService)
    {
        _pageService = pageService;
    }

    public async Task<PageListResponse> Handle(ListPagesCommand request, CancellationToken cancellationToken)
    {
        return await _pageService.GetPagesAsync(
            request.AppId,      // ✅ Now passes Guid? (nullable)
            request.Search,
            request.IsActive,
            request.IncludeDeleted,
            request.Page,
            request.PageSize,
            request.SortBy,
            request.SortOrder,
            cancellationToken
        );
    }
}