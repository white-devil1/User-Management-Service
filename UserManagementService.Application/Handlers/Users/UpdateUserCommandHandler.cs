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
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogPublisher _logPublisher;
    private readonly IFileStorageService _fileStorage;

    private readonly IUserDisplayNameResolver _resolver;

    public UpdateUserCommandHandler(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IEventPublisher eventPublisher,
        ILogPublisher logPublisher,
        IFileStorageService fileStorage,
        IUserDisplayNameResolver resolver)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _eventPublisher = eventPublisher;
        _logPublisher = logPublisher;
        _fileStorage = fileStorage;
        _resolver = resolver;
    }

    public async Task<UserResponse> Handle(
        UpdateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(request.Id);
            if (user == null || user.IsDeleted)
            {
                _logPublisher.PublishActivity(new ActivityLogEvent
                {
                    ActionType = 1,
                    EntityType = 0,
                    EntityId = request.Id,
                    Description = $"Failed to update user - user not found",
                    UserId = request.UpdatedBy,
                    IsSuccess = false,
                    FailureReason = "User not found"
                });
                throw new NotFoundException("User", request.Id);
            }

            // Resolve role IDs to names up-front (before any writes)
            List<string>? resolvedRoleNames = null;
            if (request.RoleIds != null)
            {
                resolvedRoleNames = new List<string>();
                foreach (var roleId in request.RoleIds)
                {
                    var role = await _roleManager.FindByIdAsync(roleId);
                    if (role == null)
                        throw new NotFoundException($"Role with ID '{roleId}' was not found.");
                    resolvedRoleNames.Add(role.Name!);
                }
            }

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
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                _logPublisher.PublishActivity(new ActivityLogEvent
                {
                    ActionType = 1,
                    EntityType = 0,
                    EntityId = user.Id,
                    Description = $"Failed to update user {user.Email} - validation errors",
                    OldValues = oldValues,
                    UserId = request.UpdatedBy,
                    IsSuccess = false,
                    FailureReason = errors
                });
                throw new ValidationException(
                    result.Errors.Select(e => e.Description).ToList());
            }

            if (resolvedRoleNames != null)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    var errors = string.Join("; ", removeResult.Errors.Select(e => e.Description));
                    _logPublisher.PublishActivity(new ActivityLogEvent
                    {
                        ActionType = 1,
                        EntityType = 0,
                        EntityId = user.Id,
                        Description = $"Failed to remove roles from user {user.Email}",
                        UserId = request.UpdatedBy,
                        IsSuccess = false,
                        FailureReason = errors
                    });
                    throw new ValidationException(
                        removeResult.Errors.Select(e => e.Description).ToList());
                }

                if (resolvedRoleNames.Any())
                {
                    var addResult = await _userManager.AddToRolesAsync(user, resolvedRoleNames);
                    if (!addResult.Succeeded)
                    {
                        var errors = string.Join("; ", addResult.Errors.Select(e => e.Description));
                        _logPublisher.PublishActivity(new ActivityLogEvent
                        {
                            ActionType = 1,
                            EntityType = 0,
                            EntityId = user.Id,
                            Description = $"Failed to assign roles to user {user.Email}",
                            UserId = request.UpdatedBy,
                            IsSuccess = false,
                            FailureReason = errors
                        });
                        throw new ValidationException(
                            addResult.Errors.Select(e => e.Description).ToList());
                    }
                }
            }

            // Save profile images if provided
            if (request.ProfileImageBytes != null && request.ProfileImageExtension != null)
            {
                user.ProfileImagePath = await _fileStorage.SaveProfileImageAsync(
                    request.ProfileImageBytes, user.Id, "big",
                    request.ProfileImageExtension, cancellationToken);
                await _userManager.UpdateAsync(user);
            }
            if (request.ProfileThumbBytes != null && request.ProfileThumbExtension != null)
            {
                user.ProfileThumbPath = await _fileStorage.SaveProfileImageAsync(
                    request.ProfileThumbBytes, user.Id, "thumb",
                    request.ProfileThumbExtension, cancellationToken);
                await _userManager.UpdateAsync(user);
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
                ActionType = 1,
                EntityType = 0,
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
                BranchId = user.BranchId,
                IsSuccess = true
            });

            var createdByName = await _resolver.ResolveAsync(user.CreatedBy, cancellationToken);
            var updatedByName = await _resolver.ResolveAsync(user.UpdatedBy, cancellationToken);
            return MapToUserResponse(user, roles.ToList(), createdByName, updatedByName);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logPublisher.PublishActivity(new ActivityLogEvent
            {
                ActionType = 1,
                EntityType = 0,
                EntityId = request.Id,
                Description = $"Unexpected error updating user {request.Id}",
                UserId = request.UpdatedBy,
                IsSuccess = false,
                FailureReason = ex.Message
            });
            throw;
        }
    }

    private static UserResponse MapToUserResponse(
        ApplicationUser user, List<string> roles,
        string? createdByName = null, string? updatedByName = null) => new()
        {
            Id = user.Id,
            Email = user.Email!,
            UserName = user.UserName!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            ProfileImagePath = user.ProfileImagePath,
            ProfileThumbPath = user.ProfileThumbPath,
            TenantId = user.TenantId,
            BranchId = user.BranchId,
            IsSuperAdmin = user.IsSuperAdmin,
            IsActive = user.IsActive,
            IsDeleted = user.IsDeleted,
            IsTemporaryPassword = user.IsTemporaryPassword,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt,
            CreatedBy = createdByName,
            UpdatedAt = user.UpdatedAt,
            UpdatedBy = updatedByName,
            Roles = roles
        };
}
