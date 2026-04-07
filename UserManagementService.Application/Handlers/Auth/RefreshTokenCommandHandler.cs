using MediatR;
using UserManagementService.Application.Commands.Auth;
using UserManagementService.Application.DTOs.Auth;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.Auth;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, LoginResponse>
{
    private readonly IRefreshTokenService _refreshTokenService;

    public RefreshTokenCommandHandler(IRefreshTokenService refreshTokenService)
    {
        _refreshTokenService = refreshTokenService;
    }

    public async Task<LoginResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return await _refreshTokenService.RefreshTokenAsync(request.RefreshToken);
    }
}