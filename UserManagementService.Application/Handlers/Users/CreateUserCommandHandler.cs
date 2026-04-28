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
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogPublisher _logPublisher;
    private readonly IEmailService _emailService;
    private readonly IPasswordGenerator _passwordGenerator;
    private readonly IFileStorageService _fileStorage;

    private readonly IUserDisplayNameResolver _resolver;

    public CreateUserCommandHandler(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IEventPublisher eventPublisher,
        ILogPublisher logPublisher,
        IEmailService emailService,
        IPasswordGenerator passwordGenerator,
        IFileStorageService fileStorage,
        IUserDisplayNameResolver resolver)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _eventPublisher = eventPublisher;
        _logPublisher = logPublisher;
        _emailService = emailService;
        _passwordGenerator = passwordGenerator;
        _fileStorage = fileStorage;
        _resolver = resolver;
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
                    ActionType = 0,
                    EntityType = 0,
                    Description = $"Failed to create user - email already exists: {request.Email}",
                    UserId = request.CreatedBy,
                    IsSuccess = false,
                    FailureReason = $"A user with email '{request.Email}' already exists."
                });
                throw new ConflictException(
                    $"A user with email '{request.Email}' already exists.");
            }

            // Resolve role IDs to role names before any DB writes
            var resolvedRoleNames = new List<string>();
            foreach (var roleId in request.RoleIds)
            {
                var role = await _roleManager.FindByIdAsync(roleId);
                if (role == null)
                    throw new NotFoundException($"Role with ID '{roleId}' was not found.");
                resolvedRoleNames.Add(role.Name!);
            }

            var tempPassword = _passwordGenerator.GenerateTempPassword();

            var user = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                TenantId = request.TenantId,
                BranchId = request.BranchId,
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
                    ActionType = 0,
                    EntityType = 0,
                    Description = $"Failed to create user - validation errors",
                    UserId = request.CreatedBy,
                    IsSuccess = false,
                    FailureReason = errors
                });
                throw new ValidationException(
                    result.Errors.Select(e => e.Description).ToList());
            }

            if (resolvedRoleNames.Any())
            {
                var addRolesResult = await _userManager.AddToRolesAsync(user, resolvedRoleNames);
                if (!addRolesResult.Succeeded)
                {
                    var errors = string.Join("; ", addRolesResult.Errors.Select(e => e.Description));
                    _logPublisher.PublishActivity(new ActivityLogEvent
                    {
                        ActionType = 0,
                        EntityType = 0,
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

            // Save profile images if provided
            if (request.ProfileImageBytes != null && request.ProfileImageExtension != null)
            {
                user.ProfileImagePath = await _fileStorage.SaveProfileImageAsync(
                    request.ProfileImageBytes, user.Id, "big",
                    request.ProfileImageExtension, cancellationToken);
            }
            if (request.ProfileThumbBytes != null && request.ProfileThumbExtension != null)
            {
                user.ProfileThumbPath = await _fileStorage.SaveProfileImageAsync(
                    request.ProfileThumbBytes, user.Id, "thumb",
                    request.ProfileThumbExtension, cancellationToken);
            }
            if (user.ProfileImagePath != null || user.ProfileThumbPath != null)
                await _userManager.UpdateAsync(user);

            // Send welcome email with email as login credential
            var displayName = user.FirstName ?? user.UserName;
            await _emailService.SendWelcomeEmailAsync(
                user.Email!,
                displayName ?? user.UserName ?? "User",
                user.Email!,
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
                Roles = resolvedRoleNames,
                CreatedAt = user.CreatedAt,
                CreatedBy = user.CreatedBy
            }, cancellationToken);

            _logPublisher.PublishActivity(new ActivityLogEvent
            {
                ActionType = 0,
                EntityType = 0,
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

            var createdByName = await _resolver.ResolveAsync(user.CreatedBy, cancellationToken);
            var updatedByName = await _resolver.ResolveAsync(user.UpdatedBy, cancellationToken);
            return MapToUserResponse(user, resolvedRoleNames, createdByName, updatedByName);
        }
        catch (ConflictException)
        {
            throw;
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logPublisher.PublishActivity(new ActivityLogEvent
            {
                ActionType = 0,
                EntityType = 0,
                Description = $"Unexpected error creating user {request.Email}",
                UserId = request.CreatedBy,
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
