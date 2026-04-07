using MediatR;
using Microsoft.AspNetCore.Identity;
using UserManagementService.Application.Commands.Users;
using UserManagementService.Application.Common.Exceptions;
using UserManagementService.Application.Events;
using UserManagementService.Application.Services;
using UserManagementService.Domain.Entities.Identity;

namespace UserManagementService.Application.Handlers.Users;

public class RestoreUserCommandHandler
    : IRequestHandler<RestoreUserCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogPublisher _logPublisher;

    public RestoreUserCommandHandler(
        UserManager<ApplicationUser> userManager,
        IEventPublisher eventPublisher,
        ILogPublisher logPublisher)
    {
        _userManager = userManager;
        _eventPublisher = eventPublisher;
        _logPublisher = logPublisher;
    }

    public async Task<bool> Handle(
        RestoreUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id);
        if (user == null)
            throw new NotFoundException("User", request.Id);
        if (!user.IsDeleted)
            throw new ValidationException("User is not deleted. Nothing to restore.");

        user.IsDeleted = false;
        user.DeletedAt = null;
        user.DeletedBy = null;
        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new ValidationException(
                result.Errors.Select(e => e.Description).ToList());

        await _eventPublisher.PublishAsync(new UserRestoredEvent
        {
            UserId = user.Id,
            TenantId = user.TenantId,
            BranchId = user.BranchId,
            IsDeleted = false,
            IsActive = true,
            RestoredAt = user.UpdatedAt
        }, cancellationToken);

        _logPublisher.PublishActivity(new ActivityLogEvent
        {
            ActionType = 3,  // UserRestored
            EntityType = 0,  // User
            EntityId = user.Id,
            Description = $"User {user.Email} was restored",
            UserId = user.Id,
            UserEmail = user.Email,
            TenantId = user.TenantId,
            BranchId = user.BranchId
        });

        return true;
    }
}