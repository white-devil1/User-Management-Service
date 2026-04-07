using Microsoft.EntityFrameworkCore;
using UserManagementService.Application.Services;
using UserManagementService.Domain.Entities.Auth;
using UserManagementService.Infrastructure.Persistence;

namespace UserManagementService.Infrastructure.Services.Token;

public class RefreshTokenStorageService : IRefreshTokenStorageService
{
    private readonly ApplicationDbContext _context;

    public RefreshTokenStorageService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SaveRefreshTokenAsync(string userId, string token, DateTime expiresAt)
    {
        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task RevokeRefreshTokenAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (refreshToken != null)
        {
            refreshToken.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}