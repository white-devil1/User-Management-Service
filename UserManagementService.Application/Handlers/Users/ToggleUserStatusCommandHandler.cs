using MediatR;
using Microsoft.AspNetCore.Identity;
using UserManagementService.Application.Commands.Users;
using UserManagementService.Application.Common.Exceptions;
using UserManagementService.Application.Events;
using UserManagementService.Application.Services;
using UserManagementService.Domain.Entities.Identity;

namespace UserManagementService.Application.Handlers.Users;

public class ToggleUserStatusCommandHandler
    : IRequestHandler<ToggleUserStatusCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogPublisher _logPublisher;

    public ToggleUserStatusCommandHandler(
        UserManager<ApplicationUser> userManager,
        IEventPublisher eventPublisher,
        ILogPublisher logPublisher)
    {
        _userManager = userManager;
        _eventPublisher = eventPublisher;
        _logPublisher = logPublisher;
    }

    public async Task<bool> Handle(
        ToggleUserStatusCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id);
        if (user == null || user.IsDeleted)
            throw new NotFoundException("User", request.Id);

        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = request.UpdatedBy;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new ValidationException(
                result.Errors.Select(e => e.Description).ToList());

        await _eventPublisher.PublishAsync(new UserStatusChangedEvent
        {
            UserId = user.Id,
            TenantId = user.TenantId,
            BranchId = user.BranchId,
            IsActive = user.IsActive,
            UpdatedAt = user.UpdatedAt,
            UpdatedBy = user.UpdatedBy
        }, cancellationToken);

        _logPublisher.PublishActivity(new ActivityLogEvent
        {
            ActionType = 4,  // UserStatusChanged
            EntityType = 0,  // User
            EntityId = user.Id,
            Description = $"User {user.Email} was {(request.IsActive ? "activated" : "deactivated")}",
            UserId = request.UpdatedBy,
            UserEmail = user.Email,
            TenantId = user.TenantId,
            BranchId = user.BranchId
        });

        return true;
    }
}