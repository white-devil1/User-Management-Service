using MediatR;
using UserManagementService.Application.Commands.Pages;
using UserManagementService.Application.DTOs.Pages;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.Pages;

public class TogglePageStatusCommandHandler : IRequestHandler<TogglePageStatusCommand, PageDto>
{
    private readonly IPageService _pageService;

    public TogglePageStatusCommandHandler(IPageService pageService)
    {
        _pageService = pageService;
    }

    public async Task<PageDto> Handle(TogglePageStatusCommand request, CancellationToken cancellationToken)
    {
        return await _pageService.TogglePageStatusAsync(request.Id, request.IsActive, request.UpdatedBy, cancellationToken);
    }
}