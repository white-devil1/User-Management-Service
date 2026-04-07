using MediatR;
using UserManagementService.Application.Commands.AppActions;
using UserManagementService.Application.DTOs.AppActions;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.AppActions;

public class ToggleAppActionStatusCommandHandler : IRequestHandler<ToggleAppActionStatusCommand, AppActionDto>
{
    private readonly IAppActionService _appActionService;

    public ToggleAppActionStatusCommandHandler(IAppActionService appActionService)
    {
        _appActionService = appActionService;
    }

    public async Task<AppActionDto> Handle(ToggleAppActionStatusCommand request, CancellationToken cancellationToken)
    {
        // ✅ Correct parameter order: id, isActive, updatedBy, cancellationToken
        return await _appActionService.ToggleActionStatusAsync(
            request.Id,
            request.IsActive,
            request.UpdatedBy,
            cancellationToken);
    }
}