using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UserManagementService.Application.DTOs.Auth;
using UserManagementService.Application.Services;
using UserManagementService.Domain.Entities.Auth;
using UserManagementService.Domain.Entities.Identity;
using UserManagementService.Infrastructure.Persistence;
using UserManagementService.Application.Common.Exceptions;

namespace UserManagementService.Infrastructure.Services.Token;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenStorageService _refreshTokenStorage;

    public RefreshTokenService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IRefreshTokenStorageService refreshTokenStorage)
    {
        _context = context;
        _userManager = userManager;
        _tokenService = tokenService;
        _refreshTokenStorage = refreshTokenStorage;
    }

    public async Task<LoginResponse> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (storedToken == null || storedToken.ExpiresAt < DateTime.UtcNow)
        {
            throw new ValidationException("Invalid or expired refresh token.");
        }

        if (storedToken.RevokedAt != null)
        {
            throw new ValidationException("Refresh token has been revoked.");
        }

        var user = await _userManager.FindByIdAsync(storedToken.UserId);
        if (user == null || !user.IsActive)
        {
            throw new NotFoundException("User account is deactivated or not found.");
        }

        await _refreshTokenStorage.RevokeRefreshTokenAsync(refreshToken);

        var roles = await _userManager.GetRolesAsync(user);

        var permissions = await _context.RolePermissions
            .Include(rp => rp.Permission)
            .Where(rp => roles.Contains(rp.Role.Name!))
            .Where(rp => rp.Permission.IsEnabled)
            .Select(rp => rp.Permission.PermissionCode)
            .Distinct()
            .ToListAsync();

        var newAccessToken = _tokenService.GenerateAccessToken(user, roles, new List<string>());
        var newRefreshTokenValue = _tokenService.GenerateRefreshToken();
        var accessTokenExpires = _tokenService.GetAccessTokenExpiration();
        var refreshTokenExpires = _tokenService.GetRefreshTokenExpiration();

        var newRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshTokenValue,
            ExpiresAt = refreshTokenExpires,
            CreatedAt = DateTime.UtcNow,
            ReplacedByTokenId = storedToken.Id
        };

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync();

        // Generate permissions token
        var permissionsToken = _tokenService.GeneratePermissionsToken(user.Id, permissions);

        // ... then in the return:
        return new LoginResponse
        {
            UserId = user.Id,
            Email = user.Email!,
            AccessToken = newAccessToken,
            PermissionsToken = permissionsToken,   // ADD THIS
            RefreshToken = newRefreshTokenValue,
            AccessTokenExpires = accessTokenExpires,
            RefreshTokenExpires = refreshTokenExpires,
            PermissionsTokenExpires = accessTokenExpires,  // ADD THIS
            TenantId = user.TenantId,
            BranchId = user.BranchId,
            IsSuperAdmin = user.IsSuperAdmin,
            RequiresPasswordChange = user.IsTemporaryPassword && user.MustChangePassword,
            Roles = roles.ToList()
            // Permissions property removed — use PermissionsToken instead
        };

    }
}