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

    private readonly IUserDisplayNameResolver _resolver;

    public ToggleUserStatusCommandHandler(
        UserManager<ApplicationUser> userManager,
        IEventPublisher eventPublisher,
        ILogPublisher logPublisher,
        IUserDisplayNameResolver resolver)
    {
        _userManager = userManager;
        _eventPublisher = eventPublisher;
        _logPublisher = logPublisher;
        _resolver = resolver;
    }

    public async Task<bool> Handle(
        ToggleUserStatusCommand request, CancellationToken cancellationToken)
    {
try
        {
        
                var user = await _userManager.FindByIdAsync(request.Id);
                if (user == null || user.IsDeleted)
                    throw new NotFoundException("User", request.Id);
        
                user.IsActive = request.IsActive;
                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedBy = await _resolver.ResolveAsync(request.UpdatedBy, cancellationToken);
        
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
                ActionType = 3,  // UserStatusToggled
                EntityType = 0,
                Description = $"Unexpected error in UserStatusToggled",
                UserId = request.UpdatedBy,
                IsSuccess = false,
                FailureReason = ex.Message
            });
            throw;
        }
    }
}