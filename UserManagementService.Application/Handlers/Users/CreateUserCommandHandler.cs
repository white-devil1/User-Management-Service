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

    public CreateUserCommandHandler(
        UserManager<ApplicationUser> userManager,
        IEventPublisher eventPublisher,
        ILogPublisher logPublisher)
    {
        _userManager = userManager;
        _eventPublisher = eventPublisher;
        _logPublisher = logPublisher;
    }

    public async Task<UserResponse> Handle(
        CreateUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            throw new ConflictException(
                $"A user with email '{request.Email}' already exists.");

        var user = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            TenantId = request.TenantId,
            BranchId = request.BranchId,
            IsSuperAdmin = request.IsSuperAdmin,
            IsActive = request.IsActive,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = request.CreatedBy
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new ValidationException(
                result.Errors.Select(e => e.Description).ToList());

        if (request.RoleNames.Any())
            await _userManager.AddToRolesAsync(user, request.RoleNames);

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
            BranchId = user.BranchId
        });

        return MapToUserResponse(user, new List<string>(request.RoleNames));
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