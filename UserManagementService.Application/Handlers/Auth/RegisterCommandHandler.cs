using MediatR;
using Microsoft.AspNetCore.Identity;
using UserManagementService.Application.Commands.Auth;
using UserManagementService.Application.Common.Exceptions;
using UserManagementService.Application.DTOs.Auth;
using UserManagementService.Application.Services;
using UserManagementService.Domain.Entities.Identity;

namespace UserManagementService.Application.Handlers.Auth;

public class RegisterCommandHandler
    : IRequestHandler<RegisterCommand, LoginResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenStorageService _refreshTokenStorage;
    private readonly IAppPermissionResolverService _permissionResolver;

    public RegisterCommandHandler(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IRefreshTokenStorageService refreshTokenStorage,
        IAppPermissionResolverService permissionResolver)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _refreshTokenStorage = refreshTokenStorage;
        _permissionResolver = permissionResolver;
    }

    public async Task<LoginResponse> Handle(
        RegisterCommand request, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email,
            TenantId = request.TenantId,
            BranchId = request.BranchId,
            IsSuperAdmin = request.IsSuperAdmin,
            IsActive = true,
            EmailConfirmed = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new ValidationException(
                result.Errors.Select(e => e.Description).ToList());

        var roleName = request.IsSuperAdmin ? "Super Admin" : "User";
        await _userManager.AddToRoleAsync(user, roleName);

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = await _permissionResolver
            .GetUserPermissionsAsync(user, roles, cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(
            user, roles, new List<string>());
        var permissionsToken = _tokenService.GeneratePermissionsToken(
            user.Id, permissions);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var accessExpires = _tokenService.GetAccessTokenExpiration();
        var refreshExpires = _tokenService.GetRefreshTokenExpiration();

        await _refreshTokenStorage.SaveRefreshTokenAsync(
            user.Id, refreshToken, refreshExpires);

        return new LoginResponse
        {
            UserId = user.Id,
            Email = user.Email!,
            AccessToken = accessToken,
            PermissionsToken = permissionsToken,
            RefreshToken = refreshToken,
            AccessTokenExpires = accessExpires,
            RefreshTokenExpires = refreshExpires,
            PermissionsTokenExpires = accessExpires,
            TenantId = user.TenantId,
            BranchId = user.BranchId,
            IsSuperAdmin = user.IsSuperAdmin,
            RequiresPasswordChange = false,
            Roles = roles.ToList()
        };
    }
}
