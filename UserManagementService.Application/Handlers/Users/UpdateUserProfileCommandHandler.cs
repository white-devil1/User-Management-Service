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
        user.UpdatedBy = request.UserId;

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
            throw new ValidationException(result.Errors.Select(e => e.Description).ToList());

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

        var createdByName = await _resolver.ResolveAsync(user.CreatedBy, cancellationToken);
        var updatedByName = await _resolver.ResolveAsync(user.UpdatedBy, cancellationToken);
        var roles = await _userManager.GetRolesAsync(user);
        return MapToUserResponse(user, roles.ToList(), createdByName, updatedByName);
    }

    private static UserResponse MapToUserResponse(
        ApplicationUser user, List<string> roles,
        string? createdByName, string? updatedByName) => new()
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
