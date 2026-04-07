namespace UserManagementService.Application.Services;

public interface IRefreshTokenStorageService
{
    Task SaveRefreshTokenAsync(string userId, string token, DateTime expiresAt);
    Task RevokeRefreshTokenAsync(string token);
}