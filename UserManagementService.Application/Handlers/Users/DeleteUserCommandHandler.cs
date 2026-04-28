using MediatR;
using Microsoft.AspNetCore.Identity;
using UserManagementService.Application.Commands.Users;
using UserManagementService.Application.Common.Exceptions;
using UserManagementService.Application.Events;
using UserManagementService.Application.Services;
using UserManagementService.Domain.Entities.Identity;


namespace UserManagementService.Application.Handlers.Users;

public class DeleteUserCommandHandler
    : IRequestHandler<DeleteUserCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogPublisher _logPublisher;

    public DeleteUserCommandHandler(
        UserManager<ApplicationUser> userManager,
        IEventPublisher eventPublisher,
        ILogPublisher logPublisher)
    {
        _userManager = userManager;
        _eventPublisher = eventPublisher;
        _logPublisher = logPublisher;
    }

    public async Task<bool> Handle(
        DeleteUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(request.Id);
            if (user == null)
            {
                _logPublisher.PublishActivity(new ActivityLogEvent
                {
                    ActionType = 2,  // UserDeleted
                    EntityType = 0,  // User
                    EntityId = request.Id,
                    Description = "Failed to delete user - user not found",
                    UserId = request.DeletedBy,
                    IsSuccess = false,
                    FailureReason = "User not found"
                });
                throw new NotFoundException("User", request.Id);
            }
            if (user.IsDeleted)
            {
                _logPublisher.PublishActivity(new ActivityLogEvent
                {
                    ActionType = 2,  // UserDeleted
                    EntityType = 0,  // User
                    EntityId = user.Id,
                    Description = $"Failed to delete user {user.Email} - already deleted",
                    UserId = request.DeletedBy,
                    IsSuccess = false,
                    FailureReason = "User is already deleted."
                });
                throw new ValidationException("User is already deleted.");
            }

            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            user.DeletedBy = request.DeletedBy;
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = request.DeletedBy;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                _logPublisher.PublishActivity(new ActivityLogEvent
                {
                    ActionType = 2,  // UserDeleted
                    EntityType = 0,  // User
                    EntityId = user.Id,
                    Description = $"Failed to delete user {user.Email}",
                    UserId = request.DeletedBy,
                    IsSuccess = false,
                    FailureReason = errors
                });
                throw new ValidationException(
                    result.Errors.Select(e => e.Description).ToList());
            }

            await _eventPublisher.PublishAsync(new UserDeletedEvent
            {
                UserId = user.Id,
                TenantId = user.TenantId,
                BranchId = user.BranchId,
                IsDeleted = true,
                IsActive = false,
                DeletedAt = user.DeletedAt!.Value,
                DeletedBy = user.DeletedBy
            }, cancellationToken);

            _logPublisher.PublishActivity(new ActivityLogEvent
            {
                ActionType = 2,  // UserDeleted
                EntityType = 0,  // User
                EntityId = user.Id,
                Description = $"User {user.Email} was deleted",
                UserId = request.DeletedBy,
                UserEmail = user.Email,
                TenantId = user.TenantId,
                BranchId = user.BranchId,
                IsSuccess = true
            });

            return true;
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
                ActionType = 2,  // UserDeleted
                EntityType = 0,  // User
                EntityId = request.Id,
                Description = $"Unexpected error deleting user {request.Id}",
                UserId = request.DeletedBy,
                IsSuccess = false,
                FailureReason = ex.Message
            });
            throw;
        }
    }
}