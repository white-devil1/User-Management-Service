using MediatR;
using UserManagementService.Application.DTOs.Auth;

namespace UserManagementService.Application.Commands.Auth;

public record RefreshTokenCommand(string RefreshToken) : IRequest<LoginResponse>;