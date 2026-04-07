using MediatR;
using UserManagementService.Application.Commands.AppActions;
using UserManagementService.Application.DTOs.AppActions;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.AppActions;

public class UpdateAppActionCommandHandler : IRequestHandler<UpdateAppActionCommand, AppActionDto>
{
    private readonly IAppActionService _appActionService;

    public UpdateAppActionCommandHandler(IAppActionService appActionService)
    {
        _appActionService = appActionService;
    }

    public async Task<AppActionDto> Handle(UpdateAppActionCommand request, CancellationToken cancellationToken)
    {
        var updateRequest = new UpdateAppActionRequest
        {
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive
        };

        return await _appActionService.UpdateActionAsync(request.Id, updateRequest, request.UpdatedBy, cancellationToken);
    }
}