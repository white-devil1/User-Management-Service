using MediatR;
using UserManagementService.Application.Commands.Pages;
using UserManagementService.Application.DTOs.Pages;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.Pages;

public class GetPageByIdCommandHandler : IRequestHandler<GetPageByIdCommand, PageDto>
{
    private readonly IPageService _pageService;

    public GetPageByIdCommandHandler(IPageService pageService)
    {
        _pageService = pageService;
    }

    public async Task<PageDto> Handle(GetPageByIdCommand request, CancellationToken cancellationToken)
    {
        return await _pageService.GetPageByIdAsync(request.Id, cancellationToken);
    }
}