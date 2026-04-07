using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;
using UserManagementService.Application.Commands.Users;
using UserManagementService.Application.Common.Exceptions;
using UserManagementService.Application.DTOs.Users;
using UserManagementService.Application.Events;
using UserManagementService.Application.Services;
using UserManagementService.Domain.Entities.Identity;

namespace UserManagementService.Application.Handlers.Users;

public class UpdateUserCommandHandler
    : IRequestHandler<UpdateUserCommand, UserResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogPublisher _logPublisher;

    public UpdateUserCommandHandler(
        UserManager<ApplicationUser> userManager,
        IEventPublisher eventPublisher,
        ILogPublisher logPublisher)
    {
        _userManager = userManager;
        _eventPublisher = eventPublisher;
        _logPublisher = logPublisher;
    }

    public async Task<UserResponse> Handle(
        UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id);
        if (user == null || user.IsDeleted)
            throw new NotFoundException("User", request.Id);

        var oldValues = JsonSerializer.Serialize(new
        {
            user.Email,
            user.UserName,
            user.FirstName,
            user.LastName,
            user.IsActive,
            user.BranchId
        });

        if (!string.IsNullOrEmpty(request.Email)) user.Email = request.Email;
        if (!string.IsNullOrEmpty(request.UserName)) user.UserName = request.UserName;
        if (request.FirstName != null) user.FirstName = request.FirstName;
        if (request.LastName != null) user.LastName = request.LastName;
        if (request.IsActive.HasValue) user.IsActive = request.IsActive.Value;
        if (request.BranchId.HasValue) user.BranchId = request.BranchId.Value;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = request.UpdatedBy;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new ValidationException(
                result.Errors.Select(e => e.Description).ToList());

        if (request.RoleNames != null)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRolesAsync(user, request.RoleNames);
        }

        var roles = await _userManager.GetRolesAsync(user);

        await _eventPublisher.PublishAsync(new UserUpdatedEvent
        {
            UserId = user.Id,
            Email = user.Email!,
            UserName = user.UserName!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            TenantId = user.TenantId,
            BranchId = user.BranchId,
            IsActive = user.IsActive,
            IsDeleted = user.IsDeleted,
            Roles = roles.ToList(),
            UpdatedAt = user.UpdatedAt,
            UpdatedBy = user.UpdatedBy
        }, cancellationToken);

        _logPublisher.PublishActivity(new ActivityLogEvent
        {
            ActionType = 1,  // UserUpdated
            EntityType = 0,  // User
            EntityId = user.Id,
            Description = $"User {user.Email} was updated",
            OldValues = oldValues,
            NewValues = JsonSerializer.Serialize(new
            {
                user.Email,
                user.UserName,
                user.FirstName,
                user.LastName,
                user.IsActive,
                user.BranchId
            }),
            UserId = request.UpdatedBy,
            UserEmail = user.Email,
            TenantId = user.TenantId,
            BranchId = user.BranchId
        });

        return MapToUserResponse(user, roles.ToList());
    }

    private static UserResponse MapToUserResponse(
        ApplicationUser user, List<string> roles) => new()
        {
            Id = user.Id,
            Email = user.Email!,
            UserName = user.UserName!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            TenantId = user.TenantId,
            BranchId = user.BranchId,
            IsSuperAdmin = user.IsSuperAdmin,
            IsActive = user.IsActive,
            IsDeleted = user.IsDeleted,
            IsTemporaryPassword = user.IsTemporaryPassword,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt,
            CreatedBy = user.CreatedBy,
            UpdatedAt = user.UpdatedAt,
            UpdatedBy = user.UpdatedBy,
            Roles = roles
        };
}