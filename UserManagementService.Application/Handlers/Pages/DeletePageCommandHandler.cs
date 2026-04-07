using MediatR;
using UserManagementService.Application.Commands.Pages;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.Pages;

public class DeletePageCommandHandler : IRequestHandler<DeletePageCommand, bool>
{
    private readonly IPageService _pageService;

    public DeletePageCommandHandler(IPageService pageService)
    {
        _pageService = pageService;
    }

    public async Task<bool> Handle(DeletePageCommand request, CancellationToken cancellationToken)
    {
        return await _pageService.DeletePageAsync(request.Id, request.DeletedBy, cancellationToken);
    }
}