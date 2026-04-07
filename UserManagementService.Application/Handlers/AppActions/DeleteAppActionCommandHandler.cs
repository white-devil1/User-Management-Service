using MediatR;
using UserManagementService.Application.Commands.AppActions;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.AppActions;

public class DeleteAppActionCommandHandler : IRequestHandler<DeleteAppActionCommand, bool>
{
    private readonly IAppActionService _appActionService;

    public DeleteAppActionCommandHandler(IAppActionService appActionService)
    {
        _appActionService = appActionService;
    }

    public async Task<bool> Handle(DeleteAppActionCommand request, CancellationToken cancellationToken)
    {
        return await _appActionService.DeleteActionAsync(request.Id, request.DeletedBy, cancellationToken);
    }
}