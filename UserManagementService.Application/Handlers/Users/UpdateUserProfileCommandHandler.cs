using MediatR;
using Microsoft.AspNetCore.Identity;
using UserManagementService.Application.Commands.Users;
using UserManagementService.Application.Common.Exceptions;
using UserManagementService.Application.DTOs.Users;
using UserManagementService.Application.Services;
using UserManagementService.Domain.Entities.Identity;

namespace UserManagementService.Application.Handlers.Users;

public class UpdateUserProfileCommandHandler
    : IRequestHandler<UpdateUserProfileCommand, UserResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogPublisher _logPublisher;

    private readonly IUserDisplayNameResolver _resolver;

    public UpdateUserProfileCommandHandler(
        UserManager<ApplicationUser> userManager,
        IFileStorageService fileStorage,
        ILogPublisher logPublisher,
        IUserDisplayNameResolver resolver)
    {
        _userManager = userManager;
        _fileStorage = fileStorage;
        _logPublisher = logPublisher;
        _resolver = resolver;
    }

    public async Task<UserResponse> Handle(
        UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null || user.IsDeleted)
            throw new NotFoundException("User", request.UserId);

        if (request.FirstName != null) user.FirstName = request.FirstName;
        if (request.LastName != null) user.LastName = request.LastName;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = await _resolver.ResolveAsync(request.UserId, cancellationToken);

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

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new ValidationException(
                result.Errors.Select(e => e.Description).ToList());
        }

        _logPublisher.PublishActivity(new Events.ActivityLogEvent
        {
            ActionType = 1,
            EntityType = 0,
            EntityId = user.Id,
            Description = $"User {user.Email} updated their own profile",
            UserId = user.Id,
            UserEmail = user.Email,
            TenantId = user.TenantId,
            BranchId = user.BranchId,
            IsSuccess = true
        });

        var roles = await _userManager.GetRolesAsync(user);
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
            CreatedBy = user.CreatedBy,
            UpdatedAt = user.UpdatedAt,
            UpdatedBy = user.UpdatedBy,
            Roles = roles
        };
}
