using Microsoft.AspNetCore.Identity;
using UserManagementService.Application.Services;
using UserManagementService.Domain.Entities.Identity;

namespace UserManagementService.Infrastructure.Services.Identity;

public class UserDisplayNameResolver : IUserDisplayNameResolver
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserDisplayNameResolver(UserManager<ApplicationUser> userManager)
        => _userManager = userManager;

    public async Task<string> ResolveAsync(string? userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return string.Empty;

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return string.Empty;

        var name = $"{user.FirstName} {user.LastName}".Trim();
        return string.IsNullOrEmpty(name) ? string.Empty : name;
    }
}
