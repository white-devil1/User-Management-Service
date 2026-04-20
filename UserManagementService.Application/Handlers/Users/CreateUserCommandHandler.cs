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

public class CreateUserCommandHandler
    : IRequestHandler<CreateUserCommand, UserResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogPublisher _logPublisher;
    private readonly IEmailService _emailService;
    private readonly IPasswordGenerator _passwordGenerator;

    public CreateUserCommandHandler(
        UserManager<ApplicationUser> userManager,
        IEventPublisher eventPublisher,
        ILogPublisher logPublisher,
        IEmailService emailService,
        IPasswordGenerator passwordGenerator)
    {
        _userManager = userManager;
        _eventPublisher = eventPublisher;
        _logPublisher = logPublisher;
        _emailService = emailService;
        _passwordGenerator = passwordGenerator;
    }

    public async Task<UserResponse> Handle(
        CreateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                _logPublisher.PublishActivity(new ActivityLogEvent
                {
                    ActionType = 0,  // UserCreated
                    EntityType = 0,  // User
                    Description = $"Failed to create user - email already exists: {request.Email}",
                    UserId = request.CreatedBy,
                    IsSuccess = false,
                    FailureReason = $"A user with email '{request.Email}' already exists."
                });
                throw new ConflictException(
                    $"A user with email '{request.Email}' already exists.");
            }

            // Generate temporary password
            var tempPassword = _passwordGenerator.GenerateTempPassword();

            var user = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                TenantId = request.TenantId,
                BranchId = request.BranchId,
                // IsSuperAdmin is not set - relies on DB default (false)
                IsActive = request.IsActive,
                IsDeleted = false,
                IsTemporaryPassword = true,
                MustChangePassword = true,
                TemporaryPasswordExpiresAt = DateTime.UtcNow.AddHours(24),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.CreatedBy,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = request.CreatedBy
            };

            var result = await _userManager.CreateAsync(user, tempPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                _logPublisher.PublishActivity(new ActivityLogEvent
                {
                    ActionType = 0,  // UserCreated
                    EntityType = 0,  // User
                    Description = $"Failed to create user - validation errors",
                    UserId = request.CreatedBy,
                    IsSuccess = false,
                    FailureReason = errors
                });
                throw new ValidationException(
                    result.Errors.Select(e => e.Description).ToList());
            }

            if (request.RoleNames.Any())
            {
                var addRolesResult = await _userManager.AddToRolesAsync(user, request.RoleNames);
                if (!addRolesResult.Succeeded)
                {
                    var errors = string.Join("; ", addRolesResult.Errors.Select(e => e.Description));
                    _logPublisher.PublishActivity(new ActivityLogEvent
                    {
                        ActionType = 0,  // UserCreated
                        EntityType = 0,  // User
                        EntityId = user.Id,
                        Description = $"Failed to assign roles to user {user.Email}",
                        UserId = request.CreatedBy,
                        IsSuccess = false,
                        FailureReason = errors
                    });
                    throw new ValidationException(
                        addRolesResult.Errors.Select(e => e.Description).ToList());
                }
            }

            // Send welcome email with credentials
            var userName = user.FirstName ?? user.UserName;
            await _emailService.SendWelcomeEmailAsync(
                user.Email!,
                userName ?? user.UserName ?? "User",
                user.UserName!,
                tempPassword,
                cancellationToken);

            await _eventPublisher.PublishAsync(new UserCreatedEvent
            {
                UserId = user.Id,
                Email = user.Email!,
                UserName = user.UserName!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                TenantId = user.TenantId,
                BranchId = user.BranchId,
                IsSuperAdmin = user.IsSuperAdmin,
                IsActive = user.IsActive,
                Roles = request.RoleNames,
                CreatedAt = user.CreatedAt,
                CreatedBy = user.CreatedBy
            }, cancellationToken);

            _logPublisher.PublishActivity(new ActivityLogEvent
            {
                ActionType = 0,  // UserCreated
                EntityType = 0,  // User
                EntityId = user.Id,
                Description = $"User {user.Email} was created",
                NewValues = JsonSerializer.Serialize(new
                {
                    user.Id,
                    user.Email,
                    user.UserName,
                    user.TenantId,
                    user.BranchId,
                    user.IsActive
                }),
                UserId = request.CreatedBy,
                UserEmail = user.Email,
                TenantId = user.TenantId,
                BranchId = user.BranchId,
                IsSuccess = true
            });

            return MapToUserResponse(user, new List<string>(request.RoleNames));
        }
        catch (ConflictException)
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
                ActionType = 0,  // UserCreated
                EntityType = 0,  // User
                Description = $"Unexpected error creating user {request.Email}",
                UserId = request.CreatedBy,
                IsSuccess = false,
                FailureReason = ex.Message
            });
            throw;
        }
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