using MediatR;
using Microsoft.AspNetCore.Identity;
using UserManagementService.Application.Commands.Auth;
using UserManagementService.Application.Common.Exceptions;
using UserManagementService.Application.DTOs.Auth;
using UserManagementService.Application.Events;
using UserManagementService.Application.Services;
using UserManagementService.Domain.Entities.Identity;

namespace UserManagementService.Application.Handlers.Auth;

public class LoginCommandHandler
    : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenStorageService _refreshTokenStorage;
    private readonly IAppPermissionResolverService _permissionResolver;
    private readonly ILogPublisher _logPublisher;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IRefreshTokenStorageService refreshTokenStorage,
        IAppPermissionResolverService permissionResolver,
        ILogPublisher logPublisher)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _refreshTokenStorage = refreshTokenStorage;
        _permissionResolver = permissionResolver;
        _logPublisher = logPublisher;
    }

    public async Task<LoginResponse> Handle(
        LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        // Wrong password or user not found — publish AuthFailure error then throw
        if (user == null ||
            !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            _logPublisher.PublishError(new ErrorLogEvent
            {
                Severity = 2,  // Warning
                Category = 1,  // AuthFailure
                Message = $"Login failed: invalid credentials for {request.Email}",
                UserEmail = request.Email
            });
            throw new ValidationException("Invalid email or password.");
        }

        // Deleted account
        if (user.IsDeleted)
        {
            _logPublisher.PublishError(new ErrorLogEvent
            {
                Severity = 2,
                Category = 1,
                Message = $"Login failed: account deleted — {request.Email}",
                UserEmail = request.Email,
                UserId = user.Id,
                TenantId = user.TenantId,
                BranchId = user.BranchId
            });
            throw new NotFoundException("Account not found.");
        }

        // Deactivated account
        if (!user.IsActive)
        {
            _logPublisher.PublishError(new ErrorLogEvent
            {
                Severity = 2,
                Category = 1,
                Message = $"Login failed: account deactivated — {request.Email}",
                UserEmail = request.Email,
                UserId = user.Id,
                TenantId = user.TenantId,
                BranchId = user.BranchId
            });
            throw new ValidationException(
                "Account is deactivated. Contact administrator.");
        }

        // Expired temporary password
        if (user.IsTemporaryPassword &&
            user.TemporaryPasswordExpiresAt < DateTime.UtcNow)
        {
            _logPublisher.PublishError(new ErrorLogEvent
            {
                Severity = 2,
                Category = 1,
                Message = $"Login failed: expired temporary password — {request.Email}",
                UserEmail = request.Email,
                UserId = user.Id,
                TenantId = user.TenantId,
                BranchId = user.BranchId
            });
            throw new ValidationException("Temporary password has expired.");
        }

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

        user.LastLoginAt = DateTime.UtcNow;
        user.FailedLoginAttempts = 0;
        await _userManager.UpdateAsync(user);

        // Login success — publish LoginAudit
        _logPublisher.PublishLoginAudit(new LoginAuditEvent
        {
            EventType = 0,  // Login
            UserId = user.Id,
            Email = user.Email!,
            TenantId = user.TenantId,
            BranchId = user.BranchId
        });

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
            RequiresPasswordChange =
                user.IsTemporaryPassword && user.MustChangePassword,
            Roles = roles.ToList()
        };
    }
}