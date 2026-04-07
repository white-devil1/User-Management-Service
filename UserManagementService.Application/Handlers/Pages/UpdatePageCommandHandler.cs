using MediatR;
using UserManagementService.Application.Commands.Pages;
using UserManagementService.Application.DTOs.Pages;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.Pages;

public class UpdatePageCommandHandler : IRequestHandler<UpdatePageCommand, PageDto>
{
    private readonly IPageService _pageService;

    public UpdatePageCommandHandler(IPageService pageService)
    {
        _pageService = pageService;
    }

    public async Task<PageDto> Handle(UpdatePageCommand request, CancellationToken cancellationToken)
    {
        var updateRequest = new UpdatePageRequest
        {
            Name = request.Name,
            Description = request.Description,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive
        };

        return await _pageService.UpdatePageAsync(request.Id, updateRequest, request.UpdatedBy, cancellationToken);
    }
}