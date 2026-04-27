using UserManagementService.Application.DTOs.Auth;

namespace UserManagementService.Application.Services;

public interface IRefreshTokenService
{
    Task<LoginResponse> RefreshTokenAsync(string refreshToken);
}