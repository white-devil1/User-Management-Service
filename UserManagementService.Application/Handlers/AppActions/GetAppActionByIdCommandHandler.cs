using MediatR;
using UserManagementService.Application.Commands.AppActions;
using UserManagementService.Application.DTOs.AppActions;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.AppActions;

public class GetAppActionByIdCommandHandler : IRequestHandler<GetAppActionByIdCommand, AppActionDto>
{
    private readonly IAppActionService _appActionService;

    public GetAppActionByIdCommandHandler(IAppActionService appActionService)
    {
        _appActionService = appActionService;
    }

    public async Task<AppActionDto> Handle(GetAppActionByIdCommand request, CancellationToken cancellationToken)
    {
        return await _appActionService.GetActionByIdAsync(request.Id, cancellationToken);
    }
}