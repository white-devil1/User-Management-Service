using MediatR;
using UserManagementService.Application.Commands.AppPermissions;
using UserManagementService.Application.Common.Exceptions;
using UserManagementService.Application.DTOs.AppPermissions;
using UserManagementService.Application.Events;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.AppPermissions;

public class TogglePermissionStatusCommandHandler
    : IRequestHandler<TogglePermissionStatusCommand, TogglePermissionResponseDto>
{
    private readonly IAppPermissionService _permissionService;
    private readonly ILogPublisher _logPublisher;

    public TogglePermissionStatusCommandHandler(
        IAppPermissionService permissionService, ILogPublisher logPublisher)
    {
        _permissionService = permissionService;
        _logPublisher = logPublisher;
    }

    public async Task<TogglePermissionResponseDto> Handle(
        TogglePermissionStatusCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _permissionService.TogglePermissionWithActionNameAsync(
                request.Id, request.IsEnabled, request.UpdatedBy, cancellationToken);

            _logPublisher.PublishActivity(new ActivityLogEvent
            {
                ActionType = 43,  // PermissionToggled
                EntityType = 5,   // Permission
                EntityId = request.Id.ToString(),
                Description = $"Permission '{result.ActionName}' was {(request.IsEnabled ? "enabled" : "disabled")}",
                UserId = request.UpdatedBy
            });

            return result;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (ConflictException)
        {
            throw;
        }
        catch (UnauthorizedException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logPublisher.PublishActivity(new ActivityLogEvent
            {
                ActionType = 0,
                EntityType = 8,
                Description = "Unexpected error in TogglePermissionStatus",
                UserId = request.UpdatedBy,
                IsSuccess = false,
                FailureReason = ex.Message
            });
            throw;
        }
    }
}
