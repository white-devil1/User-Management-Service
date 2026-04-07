using MediatR;
using Microsoft.AspNetCore.Identity;
using UserManagementService.Application.Commands.Auth;
using UserManagementService.Application.Common.Exceptions;
using UserManagementService.Application.Events;
using UserManagementService.Application.Services;
using UserManagementService.Domain.Entities.Identity;

namespace UserManagementService.Application.Handlers.Auth;

public class ChangePasswordCommandHandler
    : IRequestHandler<ChangePasswordCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogPublisher _logPublisher;

    public ChangePasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        ILogPublisher logPublisher)
    {
        _userManager = userManager;
        _logPublisher = logPublisher;
    }

    public async Task<bool> Handle(
        ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            throw new NotFoundException("User", request.UserId);

        var result = await _userManager.ChangePasswordAsync(
            user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
            throw new ValidationException(
                result.Errors.Select(e => e.Description).ToList());

        user.IsTemporaryPassword = false;
        user.MustChangePassword = false;
        user.LastPasswordChangedAt = DateTime.UtcNow;
        user.PasswordChangedCount++;
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        _logPublisher.PublishActivity(new ActivityLogEvent
        {
            ActionType = 73,  // PasswordChanged
            EntityType = 0,   // User
            EntityId = user.Id,
            Description = $"User {user.Email} changed their password",
            UserId = user.Id,
            UserEmail = user.Email,
            TenantId = user.TenantId,
            BranchId = user.BranchId
        });

        return true;
    }
}